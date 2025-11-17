using SpacetimeDB;
using StdbModule.GamePlayModule.CardData;
using StdbModule.GamePlayModule.HeroModule.TestHero1;

namespace StdbModule.GamePlayModule.Match
{
    public static partial class MatchModule
    {
        static readonly TimeDuration ONE_SECOND = new TimeDuration { Microseconds = 1_000_000 };
        [Reducer]
        public static void TryEnqueuePlayer(ReducerContext ctx, string username, StatsUnion stats, 
            uint targetPlayerCount, uint totalTime)
        {
            if (!ctx.TryFindPlayerAccount(username, out var player)) throw new Exception("player not found");
            if (targetPlayerCount == 1)
            {
                var m = ctx.InsertMatchContext(targetPlayerCount);
                ctx.InsertPlayerContext(player.Username, m.MatchId, stats, 0, UInt32.MaxValue);
                return;
            }
            uint playerEva = player.Evaluate;
            MatchQueue cur = new MatchQueue
            {
                Username = username,
                Evaluate = player.Evaluate,
                TargetPlayerCount = targetPlayerCount,
                Stats = stats,
                JoinTime = ctx.Timestamp
            };
            int targetCount = (int)targetPlayerCount - 1;
            var q = ctx.Db.match_queue;
            uint playerCount = targetPlayerCount;
            uint playerTime = totalTime;
            var list = q.Iter().OrderBy(x => x.JoinTime)
                .ThenBy(x => Math.Abs((int)(x.Evaluate - playerEva)))
                .SkipWhile(x => x.TargetPlayerCount != playerCount)
                .SkipWhile(x => x.TotalTime != playerTime)
                .Take(targetCount).ToList();
            
            if (list.Count != targetCount)
            {
                ctx.Db.match_queue.Insert(cur);
            }
            else
            {
                var match = ctx.InsertMatchContext(targetPlayerCount);
                var rng = ctx.Rng;
                list.Add(cur);
                var players = list.OrderBy(_ => rng.Next()).ToList();
                for (int i = 0; i < players.Count; i++)
                {
                    var p = players[i];
                    ctx.InsertPlayerContext(p.Username, match.MatchId, p.Stats, (uint)i, totalTime);
                    if (p.Username == username) continue;
                    ctx.TryRemoveMatchQueue(p.Username);
                }
            }
        }

        //TODO Need to handle ready error !!
        [Reducer]
        public static void PlayerReady(ReducerContext ctx, string username)
        {
            if (!ctx.TryFindPlayerContext(username, out var player)) throw new Exception("player not found");
            if (!ctx.TryFindMatchContext(player.MatchId, out var match)) throw new Exception("match not found");
            player.IsReady = true;
            ctx.Db.player_context.Username.Update(player);
            int count = ctx.Db.player_context.match_id.Filter(match.MatchId).Select(p => p.IsReady).Count();
            if (count == match.PlayerCount)
            {
                //Game Ready
                ctx.InitializeGame(match);
            }
        }

        [Reducer]
        public static void CancelMatch(ReducerContext ctx, string username)
        {
            ctx.Db.match_queue.Username.Delete(username);
        }

        private static void RemoveMatchCardWeight(this ReducerContext ctx, uint match_id)
        {
            foreach (var kv in CardModule.CardFactionWeightDefault)
            {
                ctx.TryRemoveCardWeight(match_id, kv.Key);
            }
            
            foreach (var kv in CardModule.CardTypeWeightDefault)
            {
                ctx.TryRemoveCardWeight(match_id, kv.Key);
            }

            foreach (var kv in CardModule.CardLevelWeightDefault)
            {
                ctx.TryRemoveCardWeight(match_id, kv.Key);
            }
        }
        private static void InitializeGame(this ReducerContext ctx, MatchContext match)
        {
            var match_id = match.MatchId;
            foreach (var kv in CardModule.CardFactionWeightDefault)
            {
                ctx.TrySetCardWeight(match_id, kv.Key, kv.Value);
            }

            foreach (var kv in CardModule.CardTypeWeightDefault)
            {
                ctx.TrySetCardWeight(match_id, kv.Key, kv.Value);
            }

            foreach (var kv in CardModule.CardLevelWeightDefault)
            {
                ctx.TrySetCardWeight(match_id, kv.Key, kv.Value);
            }

            match.State = MatchState.Initializing;

            var player = ctx.Db.player_context.match_id.Filter(match_id).First(p => p.PlayerIndex == 0);

            player.Phase = GamePhase.ActionPhase;
            ctx.Db.player_context.Username.Update(player);

            ctx.Db.player_timer_schedule.Insert(new PlayerTimerSchedule
            {
                Id = 0,
                ScheduleAt = new ScheduleAt.Interval(ONE_SECOND),
                Username = player.Username
            });
            
            //Determine Player Index, Create Timer
            ctx.Db.match_context.MatchId.Update(match);
        }

        #region InGame

        [Reducer]
        public static void RequestDealCard(ReducerContext ctx, string username, uint count)
        {
            if (count >= 10) count = 10;
            ctx.DealCard(username, count);
        }

        private static void DealCard(this ReducerContext ctx, string owner, uint count)
        {
            //TODO Get Random Card => 
            if (!ctx.TryFindPlayerContext(owner, out var player)) throw new Exception("player not found");
            if (!ctx.TryFindMatchContext(player.MatchId, out var match)) throw new Exception("match not found");
            for (int i = 0; i < count; i++)
            {
                uint BaseCardId = ctx.GetRandomBaseCard(player.MatchId);
                ctx.TryInsertPlayerHandBaseCard(player.Username, BaseCardId, match.MatchId);
            }
        }

        private static void DiscardHandCard(this ReducerContext ctx, string owner, List<uint> cards)
        {
            var hand_cards = ctx.Db.player_base_hand.owner.Filter(owner)
                .Where(c => cards.Contains(c.CardInstanceId)).ToList();
            foreach (var card in hand_cards)
            {
                ctx.Db.player_base_hand.Delete(card);
            }
        }

        #endregion

        private static uint GetRandomBaseCard(this ReducerContext ctx, uint match_id)
        {
            var baseCard = ctx.Db.base_card.Iter().ToList();
            Dictionary<uint, float> baseCardMap = new Dictionary<uint, float>();
            var rng = ctx.Rng;
            float totalWeight = 0;
            foreach (var card in baseCard)
            {
                if (!ctx.TryGetCardWeight(card, match_id, out var weight))
                    continue;
                if (weight <= 0)
                    continue;
                baseCardMap[card.BaseCardId] = weight;
                totalWeight += weight;
            }
            
            if (totalWeight <= 0 || baseCardMap.Count == 0) throw new Exception("not card found");
            
            float randomWeight = rng.NextSingle() * totalWeight;
            
            float cumulative = 0f;
            foreach (var kvp in baseCardMap)
            {
                cumulative += kvp.Value;
                if (randomWeight <= cumulative)
                {
                    return kvp.Key;
                }
            }

            return baseCardMap.Last().Key;
        }

        private static void TryInsertPlayerHandBaseCard(this ReducerContext ctx, string username, uint base_card,
            uint match_id)
        {
            ctx.Db.player_base_hand.Insert(new PlayerBaseCardHand
            {
                BaseCardId = base_card,
                MatchId = match_id,
                Owner = username
            });
        }

        [Reducer]
        public static void SwitchTime(ReducerContext ctx, string username, uint next = UInt32.MaxValue)
        {
            // stop self-timer, switch to next player
            if (!ctx.TryFindPlayerContext(username, out var player)) throw new Exception("player not found");
            if (!ctx.TryFindMatchContext(player.MatchId, out var match)) throw new Exception("match not found");
            if (player.PlayerIndex != match.CurrentPlayerIndex) throw new Exception("player call switch invalidate");
            ctx.DeleteTimer(username);
            if (next == UInt32.MaxValue)
            {
                next = (match.CurrentPlayerIndex + 1) % match.PlayerCount;
            }
            match.CurrentPlayerIndex = next;
            player.Phase = GamePhase.WaitingPhase;
            ctx.Db.match_context.MatchId.Update(match);
            ctx.Db.player_context.Username.Update(player);
        }

        public static void TryClearPracticeGame(this ReducerContext ctx, string username)
        {
            if (!ctx.TryFindPlayerContext(username, out var player)) throw new Exception("player not found");
            if (!ctx.TryFindMatchContext(player.MatchId, out var match)) throw new Exception("match not found");
            //clear player context
            ctx.DeleteTimer(player.Username);
            ctx.Db.player_base_hand.owner.Delete(player.Username);
            ctx.Db.player_bench.username.Delete(username);
            ctx.Db.player_context.Username.Delete(username);
            //clear match context
            ctx.RemoveMatchCardWeight(match.MatchId);
            ctx.Db.match_context.MatchId.Delete(match.MatchId);
        }

        public static bool TryFindPlayerContext(this ReducerContext ctx, string username, out PlayerContext player)
        {
            player = ctx.Db.player_context.Username.Find(username).GetValueOrDefault();
            return player != default;
        }

        //TODO  
        // Complete Match Player Timer Schedule Logic, Table & Schedule
        public static void TryInsertPlayerContext(this ReducerContext ctx, string username, uint matchId, StatsUnion stats, uint playerIndex, uint totalTime)
        {
            if (ctx.FindPlayerAccount(username) == null) return;
            ctx.InsertPlayerContext(username, matchId, stats, playerIndex, totalTime);
        }

        public static bool TryFindMatchContext(this ReducerContext ctx, uint matchId, out MatchContext matchContext)
        {
            matchContext = ctx.Db.match_context.MatchId.Find(matchId).GetValueOrDefault();
            return matchContext != default;
        }

        private static void TryRemoveMatchQueue(this ReducerContext ctx, string username)
        {
            ctx.Db.match_queue.Username.Delete(username);
        }

        private static MatchContext InsertMatchContext(this ReducerContext ctx, uint playerCount)
        {
            MatchState state = playerCount == 1 ? MatchState.Single : MatchState.Prepared;
            return ctx.Db.match_context.Insert(new MatchContext
            {
                PlayerCount = playerCount,
                State = state,
                CreatedAt = ctx.Timestamp,
                CurrentPlayerIndex = 0,
                TurnIndex = 0
            });
        }

        private static void InsertPlayerContext(this ReducerContext ctx, string username, uint matchId, 
            StatsUnion stats, uint playerIndex, uint totalTime)
        {
            ctx.Db.player_context.Insert(new PlayerContext
            {
                Username = username,
                MatchId = matchId,
                Stats = stats,
                PlayerIndex = playerIndex,
                IsReady = false,
                TotalTime = totalTime,
                Phase = GamePhase.WaitingPhase
            });
        }

        [Reducer]
        public static void PlayerTimer(ReducerContext ctx, PlayerTimerSchedule schedule)
        {
            if (ctx.Sender != ctx.Identity) throw new Exception("Player Timer Can Only call by schedule");
            if (ctx.TryFindPlayerContext(schedule.Username, out var player))
            {
                player.TotalTime -= 1;
                if (player.TotalTime == 0)
                {
                    player.Phase = GamePhase.DeathPhase;
                    ctx.DeleteTimer(schedule.Id);
                }
                ctx.Db.player_context.Username.Update(player);
            }
            else
            {
                ctx.DeleteTimer(schedule.Id);
            }
        }

        private static void DeleteTimer(this ReducerContext ctx, string username)
        {
            ctx.Db.player_timer_schedule.username.Delete(username);
        }
        
        private static void DeleteTimer(this ReducerContext ctx, ulong id)
        {
            ctx.Db.player_timer_schedule.Id.Delete(id);
        }
    }
}

