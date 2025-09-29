using SpacetimeDB;
using StdbModule.GamePlayModule.HeroModule.TestHero1;

namespace StdbModule.GamePlayModule.Match
{
    public static partial class MatchModule
    {
        [Reducer]
        public static void InitializePlayerContext(ReducerContext ctx, string username, StatsUnion stats)
        {
            ctx.TryInsertPlayerContext(username, stats);
        }

        public static void TryInsertPlayerContext(this ReducerContext ctx, string username, StatsUnion stats)
        {
            if (ctx.FindPlayerAccount(username) == null) return;
            ctx.InsertPlayerContext(username, stats);
        }

        private static void InsertPlayerContext(this ReducerContext ctx, string username, StatsUnion stats)
        {
            ctx.Db.player_context.Insert(new PlayerContext
            {
                PlayerId = username,
                Stats = stats
            });
        }
    }
}

