using SpacetimeDB;
using StdbModule.GamePlayModule.Match;

namespace StdbModule.GamePlayModule.CardData
{
    [Table(Name = "hero_card", Public = true)]
    public partial struct HeroCard
    {
        [PrimaryKey, AutoInc]
        public uint HeroCardId;
        [Unique] 
        public string CardName;
        public string CardDescription;
        public StatsUnion Stats;
    }
}