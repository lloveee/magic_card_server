using SpacetimeDB;

namespace StdbModule;

[Table(Name = "validate_info")]
public partial struct ValidateInfo
{
    [AutoInc, PrimaryKey]
    public uint ValidateInfoId;

    public string HashCode;
}

[Type]
public enum ValidateTarget
{
    BaseCard,
    HeroCard
}