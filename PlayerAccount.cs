using SpacetimeDB;

namespace StdbModule;

[Table(Name = "player_account", Public = true)]
public partial struct PlayerAccount
{
    [PrimaryKey, Unique] 
    public string Username;

    public string Nickname;
    public MatchInfo Info;
    public uint Rank;
    public uint Evaluate;
}

[Type]
public partial struct MatchInfo
{
    public uint Win;
    public uint Loss;
}