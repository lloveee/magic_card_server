using SpacetimeDB;

namespace StdbModule.AuthModule
{
    [Table(Name = "c_connection", Public = true)]
    public partial struct Connection
    {
        [PrimaryKey] 
        public Identity Identity;
        public bool IsConnected;
        public bool IsValidated;
    }
}