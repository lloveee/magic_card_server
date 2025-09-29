using SpacetimeDB;

namespace StdbModule.AuthModule
{
    public static partial class ConnectionModule
    {
        public static bool TryFindConnection(this ReducerContext ctx, Identity identity, out Connection connection)
        {
            connection = ctx.Db.c_connection.Identity.Find(identity).GetValueOrDefault();
            return connection != default;
        }
    }
}