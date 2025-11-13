using SpacetimeDB;
using SpacetimeDB.BSATN;
using StdbModule.GamePlayModule.CardData;
using StdbModule.GamePlayModule.HeroModule.TestHero1;
using StdbModule.GamePlayModule.HeroModule.TestHero2;

namespace StdbModule.GamePlayModule.Match
{
    [Table(Name = "match_context", Public = true)]
    public partial struct MatchContext
    {
        [AutoInc, PrimaryKey]
        public uint MatchId;
        public MatchState State;
        public uint PlayerCount;
        public uint TurnIndex;
        public uint CurrentPlayerIndex;
        public Timestamp CreatedAt;
    }

    [Table(Name = "match_queue")]
    public partial struct MatchQueue
    {
        [Unique, PrimaryKey]
        public string Username;

        public Timestamp JoinTime;
        public uint Evaluate;
        public uint TargetPlayerCount;
        public uint TotalTime;
        public StatsUnion Stats;
    }
    
    [Table(Name = "player_context", Public = true)]
    [SpacetimeDB.Index.BTree(Name = "match_id", Columns = [nameof(MatchId)])]
    public partial struct PlayerContext
    {
        [PrimaryKey] 
        public string Username;
        public uint MatchId;
        public StatsUnion Stats;
        public uint PlayerIndex;
        public bool IsReady;
        public uint TotalTime;
        public GamePhase Phase;
    }
    
    //Player Timer Scheduler
    [Table(Name = "player_timer_schedule", Scheduled = nameof(MatchModule.PlayerTimer),
        ScheduledAt = nameof(ScheduleAt))]
    [SpacetimeDB.Index.BTree(Name = "username", Columns = [nameof(Username)])]
    public partial struct PlayerTimerSchedule
    {
        [PrimaryKey] [AutoInc] public ulong Id;
        public ScheduleAt ScheduleAt;
        public string Username;
    }
    

    [Table(Name = "player_base_hand", Public = true)]
    [SpacetimeDB.Index.BTree(Name = "match_id", Columns = [nameof(MatchId)])]
    [SpacetimeDB.Index.BTree(Name = "owner", Columns = [nameof(Owner)])]
    [SpacetimeDB.Index.BTree(Name = "base_card_id", Columns = [nameof(BaseCardId)])]
    public partial struct PlayerBaseCardHand
    {
        [PrimaryKey, AutoInc] 
        public uint CardInstanceId;
        public uint BaseCardId;
        public uint MatchId;
        public string Owner;
    }

    [Table(Name = "player_bench", Public = true)]
    [SpacetimeDB.Index.BTree(Name = "username", Columns = [nameof(Username)])]
    [SpacetimeDB.Index.BTree(Name = "bench_index", Columns = [nameof(BenchIndex)])]
    public partial struct PlayerBench
    {
        [PrimaryKey, AutoInc]
        public uint BenchInstanceId;
        public string Username;
        public uint BenchIndex;
        public List<uint> UnionCardsId;
        public uint MaxUnionCount;
    }

    [Type]
    public enum MatchState
    {
        Prepared,
        Initializing,
        Single,
    }
    
    [Type]
    public enum GamePhase
    {
        WaitingPhase,
        ActionPhase,
        ResponsePhase,
        DeathPhase,
    }
    
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

