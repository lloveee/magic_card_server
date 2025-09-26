using SpacetimeDB;

namespace StdbModule;

[Table(Name = "auth_account")]
[SpacetimeDB.Index.BTree(Name = "CurrentIdentity", Columns = [nameof(CurrentIdentity)])]
public partial struct AuthAccount
{
    [AutoInc, PrimaryKey] 
    public uint AccountId;
    [Unique]
    public string Username;
    public string Password;
    public bool IsOnline;
    public Timestamp CreatedAt;
    public Identity CurrentIdentity;
}