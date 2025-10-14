using SpacetimeDB;

namespace StdbModule.AuthModule
{
    [Table(Name = "data_signature", Public = true)]
    public partial struct DataSignature
    {
        [PrimaryKey] 
        public string ValidateInfoId;
        public string PlayLoad;
    }
}

