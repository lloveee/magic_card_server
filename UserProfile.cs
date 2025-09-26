using SpacetimeDB;

namespace StdbModule;

[Table(Name = "user_profile", Public = true)]
public partial struct UserProfile
{
    [PrimaryKey]
    public uint AccountId;

    public uint Wins;
    public uint Losses;
    public bool InMatch;
    public ulong CurrentMatchId;
}