using SpacetimeDB;

namespace StdbModule;

[Table(Name = "c_connection", Public = true)]
public partial struct Connection
{
    [PrimaryKey] 
    public Identity Identity;
    public bool IsConnected;
}