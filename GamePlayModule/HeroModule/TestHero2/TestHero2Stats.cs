using SpacetimeDB;
using StdbModule.GamePlayModule.Match;

namespace StdbModule.GamePlayModule.HeroModule.TestHero2;

[Type, Serializable]
public partial struct TestHero2Stats()
{
    public PlayerStats BaseStats = new();
    public uint Heal = 10;

    public void Skill1()
    {
        Heal += 10;
    }
}