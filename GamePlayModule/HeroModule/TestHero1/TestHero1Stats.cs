using SpacetimeDB;
using StdbModule.GamePlayModule.Match;

namespace StdbModule.GamePlayModule.HeroModule.TestHero1;

[Type, Serializable]
public partial struct TestHero1Stats()
{
    public PlayerStats BaseStats = new ();
    public uint Heal = 10;

    public void Skill1()
    {
        Heal += 10;
    }
}