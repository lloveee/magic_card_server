using SpacetimeDB;

namespace StdbModule;

[Table(Name = "hero_card")]
public partial struct HeroCard
{
    [PrimaryKey, AutoInc]
    public uint HeroCardId;
    [Unique] 
    public string CardName;
    public string CardDescription;
    
}