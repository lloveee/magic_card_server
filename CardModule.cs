using SpacetimeDB;

namespace StdbModule;

public static partial class CardModule
{
    public static void BulkInsertHeroCard(this ReducerContext ctx, IEnumerable<HeroCard> cards)
    {
        foreach (var card in cards)
        {
            ctx.Db.hero_card.Insert(card);
        }
    }

    public static void BulkInsertBaseCard(this ReducerContext ctx, IEnumerable<BaseCard> cards)
    {
        foreach (var card in cards)
        {
            ctx.Db.base_card.Insert(card);
        }
    }
}