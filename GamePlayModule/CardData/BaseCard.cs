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