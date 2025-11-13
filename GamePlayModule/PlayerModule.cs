using SpacetimeDB;
using StdbModule.AuthModule;

namespace StdbModule.GamePlayModule
{
    public static partial class PlayerModule
    {
        [Reducer]
        public static void Initialize(ReducerContext ctx, string username, string nickname)
        {
            if (!ValidateString(nickname)) throw new ArgumentException("Invalidate Nickname");
            if (!ctx.ValidateIdentity(username)) throw new ArgumentException("Invalidate Operation");
            ctx.TryInsertPlayer(new PlayerAccount
            {
                Username = username,
                Nickname = nickname,
                Info = new MatchInfo
                {
                    Win = 0,
                    Loss = 0
                },
                Rank = 0,
                Evaluate = 0
            });
        }

        [Reducer]
        public static void PlayerRename(ReducerContext ctx, string nickname)
        {
            if (!ValidateString(nickname)) throw new ArgumentException("Invalidate Nickname");
            if (!ctx.ValidateIdentity()) throw new ArgumentException("Invalidate Operation");
            ctx.TryUpdatePlayer(nickname);
        }

        public static PlayerAccount? FindPlayerAccount(this ReducerContext ctx, string username)
        {
            return ctx.Db.player_account.Username.Find(username);
        }

        public static bool TryFindPlayerAccount(this ReducerContext ctx, string username, out PlayerAccount player)
        {
            player = ctx.Db.player_account.Username.Find(username).GetValueOrDefault();
            return player != default;
        }

        public static void TryInsertPlayer(this ReducerContext ctx, PlayerAccount player)
        {
            if (ctx.FindPlayerAccount(player.Username) != null) return;
            ctx.InsertPlayer(player);
        }
        private static void InsertPlayer(this ReducerContext ctx, PlayerAccount player)
        {
            ctx.Db.player_account.Insert(player);
        }

        public static void TryUpdatePlayer(this ReducerContext ctx, string nickname)
        {
            if (!ctx.TryFindAuthAccount(out var account)) return;
            var p = ctx.FindPlayerAccount(account.Username).GetValueOrDefault();
            if (p == default) return;
            p.Nickname = nickname;
            ctx.UpdatePlayer(p);
        }

        private static void UpdatePlayer(this ReducerContext ctx, PlayerAccount player)
        {
            ctx.Db.player_account.Username.Update(player);
        }

        public static bool ValidateString(string s)
        {
            return s != String.Empty;
        }
    }
}