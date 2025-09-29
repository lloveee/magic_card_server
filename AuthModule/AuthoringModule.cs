using SpacetimeDB;

namespace StdbModule.AuthModule
{
    public static partial class AuthoringModule
    {
        [Reducer]
        public static void AuthLogin(ReducerContext ctx, string username, string password)
        {
            var player = ctx.FindAuthAccount(username) ?? throw new ArgumentException("Username not found");
            if (!player.Password.Equals(password)) throw new ArgumentException("Password not correct");
            if (player.IsOnline)
            {
                var old_player = ctx.FindAuthAccount(username) ?? throw new ArgumentException("Username not found");
                if (ctx.TryFindConnection(old_player.CurrentIdentity, out var old_connection)) //try kick
                {
                    old_connection.IsConnected = false;
                    ctx.Db.c_connection.Identity.Update(old_connection);
                }
                old_player.CurrentIdentity = default;
                old_player.IsOnline = false;
                ctx.Db.auth_account.AccountId.Update(old_player);
            } //AuthLogout(ctx, username);
            
            player.CurrentIdentity = ctx.Sender;
            player.IsOnline = true;
            ctx.Db.auth_account.AccountId.Update(player);
        }

        [Reducer]
        public static void AuthLogout(ReducerContext ctx, string username)
        {
            var player = ctx.FindAuthAccount(username) ?? throw new ArgumentException("Username not found");
            player.CurrentIdentity = default;
            player.IsOnline = false;
            ctx.Db.auth_account.AccountId.Update(player);
        }

        [Reducer]
        public static void AuthRegister(ReducerContext ctx, string username, string password)
        {
            if (ctx.TryFindAuthAccount(username, out _)) throw new ArgumentException("Username already exists");
            var account = ctx.Db.auth_account.Insert(new AuthModule.AuthAccount
            {
                Username = username,
                Password = password,
                CreatedAt = ctx.Timestamp,
                IsOnline = false,
                CurrentIdentity = default
            });
        }

        public static void BulkInsertAccount(this ReducerContext ctx, AuthModule.AuthAccount[] accounts)
        {
            foreach (var account in accounts)
            {
                ctx.Db.auth_account.Insert(account);
            }
        }
        
        public static AuthAccount? FindAuthAccount(this ReducerContext ctx, string username)
        {
            return ctx.Db.auth_account.Username.Find(username);
        }
        public static bool TryFindAuthAccount(this ReducerContext ctx, out AuthAccount account)
        {
            account = ctx.Db.auth_account.CurrentIdentity.Filter(ctx.Sender).FirstOrDefault();
            return account != default;
        }
        
        public static bool TryFindAuthAccount(this ReducerContext ctx, string username, out AuthAccount account)
        {
            account = ctx.Db.auth_account.Username.Find(username).GetValueOrDefault();
            return account != default;
        }

        public static bool ValidateIdentity(this ReducerContext ctx, string username)
        {
            if (ctx.TryFindAuthAccount(username, out var account))
            {
                return account.CurrentIdentity == ctx.Sender;
            }
            return false;
        }
        public static bool ValidateIdentity(this ReducerContext ctx)
        {
            return ctx.TryFindAuthAccount(out _);
        }
    }
}


