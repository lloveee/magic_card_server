using SpacetimeDB;
using StdbModule.AuthModule;
using StdbModule.GamePlayModule.CardData;
using StdbModule.Utils;
using BaseCard = StdbModule.GamePlayModule.CardData.BaseCard;
using HeroCard = StdbModule.GamePlayModule.CardData.HeroCard;

namespace StdbModule
{
    public static partial class InitModule
    {
        private static Dictionary<string, (string, string)> RSAKeys = new();
        [Reducer(ReducerKind.Init)]
        public static void Initialization(ReducerContext ctx)
        {
            ctx.InitRSAKey();
            ctx.InitAccount();
            ctx.InitHeroCard();
            ctx.InitBaseCard();
            
        }

        [Reducer(ReducerKind.ClientConnected)]
        public static void ClientConnected(ReducerContext ctx)
        {
            if (ctx.TryFindConnection(ctx.Sender, out var connection))
            {
                connection.IsConnected = true;
                ctx.Db.c_connection.Identity.Update(connection);
            }
            else
            {  
                ctx.Db.c_connection.Insert(new AuthModule.Connection
                {
                    Identity = ctx.Sender,
                    IsConnected = true,
                    IsValidated = false
                });
            }
        }
        
        [Reducer(ReducerKind.ClientDisconnected)]
        public static void ClientDisConnected(ReducerContext ctx)
        {
            if (ctx.TryFindAuthAccount(out var account))
            {
                account.IsOnline = false;
                account.CurrentIdentity = default;
                ctx.Db.auth_account.AccountId.Update(account);
            }

            if (ctx.TryFindConnection(ctx.Sender, out var connection))
            {
                connection.IsConnected = false;
                connection.IsValidated = false;
                ctx.Db.c_connection.Identity.Update(connection);
            }
        }

        private static void InitAccount(this ReducerContext ctx)
        {
            ctx.BulkInsertAccount([
                new AuthAccount
                {
                    Username = "admin",
                    Password = "admin",
                    IsOnline = false,
                    CurrentIdentity = default,
                    CreatedAt = ctx.Timestamp
                },
                new AuthAccount
                {
                    Username = "admin2",
                    Password = "admin2",
                    IsOnline = false,
                    CurrentIdentity = default,
                    CreatedAt = ctx.Timestamp
                }
            ]);
        }

        private static void InitHeroCard(this ReducerContext ctx)
        {
            /*
            ctx.BulkInsertHeroCard([
                new HeroCard()
                {
                    CardName = "Test Hero1",
                    CardDescription = "This is a test for hero1",
                    
                },
                new HeroCard()
                {
                    CardName = "Test Hero2",
                    CardDescription = "This is a test for hero2"
                }
            ]);*/
        }

        private static void InitRSAKey(this ReducerContext ctx)
        {
            ctx.TryUpdateSignature(nameof(ValidateTarget.HeroCard));
        }

        private static void InitBaseCard(this ReducerContext ctx)
        {
            List<BaseCard> initCard = new List<BaseCard>();
            initCard.AddRange(GetAllLevelWithDetail(CardFaction.Dream, CardType.Attack, "梦境攻击"));
            initCard.AddRange(GetAllLevelWithDetail(CardFaction.Dream, CardType.Skill, "梦境技能"));
            initCard.AddRange(GetAllLevelWithDetail(CardFaction.Dream, CardType.Energy, "梦境能耗"));
            initCard.AddRange(GetAllLevelWithDetail(CardFaction.Illusion, CardType.Attack, "虚幻攻击"));
            initCard.AddRange(GetAllLevelWithDetail(CardFaction.Illusion, CardType.Skill, "虚幻技能"));
            initCard.AddRange(GetAllLevelWithDetail(CardFaction.Illusion, CardType.Energy, "虚幻能耗"));
            initCard.AddRange(GetAllLevelWithDetail(CardFaction.Rebirth, CardType.Attack, "轮回攻击"));
            initCard.AddRange(GetAllLevelWithDetail(CardFaction.Rebirth, CardType.Skill, "轮回技能"));
            initCard.AddRange(GetAllLevelWithDetail(CardFaction.Rebirth, CardType.Attack, "轮回能耗"));
            initCard.AddRange(GetAllLevelWithDetail(CardFaction.Reshape, CardType.Attack, "重组攻击"));
            initCard.AddRange(GetAllLevelWithDetail(CardFaction.Reshape, CardType.Skill, "重组技能"));
            initCard.AddRange(GetAllLevelWithDetail(CardFaction.Reshape, CardType.Energy, "重组能耗"));
            ctx.BulkInsertBaseCard(initCard);
        }

        private static IList<BaseCard> GetAllLevelWithDetail(CardFaction faction, CardType type, string description,
            int l = 1, int r = 5)
        {
            IList<BaseCard> res = new List<BaseCard>();
            for (int i = l; i <= r; i++)
            {
                res.Add(new BaseCard
                {
                    CardFaction = faction,
                    CardType = type,
                    CardDescription = description,
                    CardLevel = (uint)i,
                });
            }

            return res;
        }
    }
}

