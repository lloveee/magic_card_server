using System.ComponentModel.DataAnnotations;
using System.Text;
using SpacetimeDB;
using StdbModule.AuthModule;
using StdbModule.Utils;

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
            var finalJson = ctx.SerializeHeroCardsToBase64(cards);
            ctx.TryUpdateSignature(nameof(ValidateTarget.HeroCard), finalJson);
            ctx.TryInsertValidateInfo(nameof(ValidateTarget.HeroCard), HashUtils.ComputeHash(finalJson));
        }

        [Reducer]
        public static void ReInsertHeroCard(this ReducerContext ctx, List<HeroCard> cards)
        {
            foreach (var c in ctx.Db.hero_card.Iter().ToArray())
            {
                ctx.Db.hero_card.HeroCardId.Delete(c.HeroCardId);
            }
            foreach (var card in cards)
            {
                ctx.Db.hero_card.Insert(card);
            }
            var finalJson = ctx.SerializeHeroCardsToBase64(cards);
            ctx.TryUpdateSignature(nameof(ValidateTarget.HeroCard), finalJson);
            ctx.TryInsertValidateInfo(nameof(ValidateTarget.HeroCard), HashUtils.ComputeHash(finalJson));
        }

        [Reducer]
        public static void TryValidateHeroCard(this ReducerContext ctx, List<HeroCard> cards)
        {
            var finalJson = ctx.SerializeHeroCardsToBase64(cards);
            if (!ctx.Validate(nameof(ValidateTarget.HeroCard), finalJson)) throw new ValidationException("card error");
            if(ctx.TryFindConnection(ctx.Sender, out var connection))
            {
                connection.IsValidated = true;
                ctx.Db.c_connection.Identity.Update(connection);
            }
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