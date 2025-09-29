using SpacetimeDB;

namespace StdbModule.AuthModule
{
    [Table(Name = "validate_info")]
    public partial struct ValidateInfo
    {
        [PrimaryKey]
        public string ValidateInfoId;

        public string HashCode;
    }

    [Type]
    public enum ValidateTarget
    {
        BaseCard,
        HeroCard
    }
}

