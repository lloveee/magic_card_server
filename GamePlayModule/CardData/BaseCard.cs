using SpacetimeDB;

namespace StdbModule.GamePlayModule.CardData
{
    [Table(Name = "base_card", Public = true)]
    public partial struct BaseCard
    {
        [AutoInc, PrimaryKey]
        public uint BaseCardId;

        public uint CardLevel;

        public CardType CardType;
        public CardFaction CardFaction;
        public string CardDescription;
    }
    [Table(Name = "card_faction_weight")]
    public partial struct CardFactionWeight
    {
        [PrimaryKey] 
        public string InstanceId;
        public float Weight;
    }

    [Table(Name = "card_type_weight")]
    public partial struct CardTypeWeight
    {
        [PrimaryKey]
        public string InstanceId;
        public float Weight;
    }

    [Table(Name = "card_level_weight")]
    public partial struct CardLevelWeight
    {
        [PrimaryKey]
        public string InstanceId;
        public float Weight;
    }

    [Type]
    public enum CardType
    {
        Attack,
        Skill,
        Energy,
        All
    }

    [Type]
    public enum CardFaction
    {
        Dream,
        Illusion,
        Reshape,
        Rebirth,
        All
    }
}