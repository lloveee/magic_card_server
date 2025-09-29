using SpacetimeDB;

namespace StdbModule.GamePlayModule.CardData
{
    public static partial class CardModule
    {
        [Reducer]
        public static void BulkInsertOrUpdateHeroCard(this ReducerContext ctx, List<HeroCard> cards)
        {
            foreach (var card in cards)
            {
                if (ctx.TryFindHeroCard(card.CardName, out var c_card))
                {
                    c_card.CardDescription = card.CardDescription;
                    c_card.Stats = card.Stats;
                    ctx.Db.hero_card.CardName.Update(c_card);
                }
                else ctx.Db.hero_card.Insert(card);
            }
            //TODO: Calculate Hash Code
        }

        private static bool TryFindHeroCard(this ReducerContext ctx, string cardName, out HeroCard card)
        {
            card = ctx.Db.hero_card.CardName.Find(cardName).GetValueOrDefault();
            return card != default;
        }

        public static void BulkInsertBaseCard(this ReducerContext ctx, IEnumerable<BaseCard> cards)
        {
            foreach (var card in cards)
            {
                ctx.Db.base_card.Insert(card);
            }
        }
    }
}