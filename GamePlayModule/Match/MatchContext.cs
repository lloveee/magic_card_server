using SpacetimeDB;
using SpacetimeDB.BSATN;
using StdbModule.GamePlayModule.HeroModule.TestHero1;
using StdbModule.GamePlayModule.HeroModule.TestHero2;

namespace StdbModule.GamePlayModule.Match
{
    [Table(Name = "match_context", Public = true)]
    public partial struct MatchContext
    {
        [AutoInc, PrimaryKey]
        public uint MatchId;
    }
    
    [Table(Name = "player_context", Public = true)]
    public partial struct PlayerContext
    {
        [PrimaryKey] 
        public string PlayerId;
        public StatsUnion Stats;
    } 
    [Serializable]
    [Type]
    public partial struct PlayerStats()
    {
        public uint MaxHealth = 100;
        public uint CurrentHealth = 100;
        public uint MaxEnergy = 100;
        public uint CurrentEnergy = 0;
    }
    
    [Type]
    public partial record StatsUnion : TaggedEnum<(
        TestHero1Stats Hero1, 
        TestHero2Stats Hero2
    )> { }
}

