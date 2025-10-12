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
            StringBuilder sb = new StringBuilder();
            foreach (var card in cards)
            {
                if (ctx.TryFindHeroCard(card.CardName, out var c_card))
                {
                    sb.Append($"{c_card.HeroCardId}:{c_card.CardName}:{c_card.CardDescription}:{c_card.Stats}\n");
                    c_card.CardDescription = card.CardDescription;
                    c_card.Stats = card.Stats;
                    ctx.Db.hero_card.CardName.Update(c_card);
                }
                else ctx.Db.hero_card.Insert(card);
            }
            
            var finalJson = sb.ToString();
            ctx.TryInsertValidateInfo(nameof(ValidateTarget.HeroCard), HashUtils.ComputeHash(finalJson));
        }

        [Reducer]
        public static void ReInsertHeroCard(this ReducerContext ctx, List<HeroCard> cards)
        {
            foreach (var c in ctx.Db.hero_card.Iter().ToArray())
            {
                ctx.Db.hero_card.HeroCardId.Delete(c.HeroCardId);
            }
            StringBuilder sb = new StringBuilder();
            foreach (var card in cards)
            {
                sb.Append($"{card.HeroCardId}:{card.CardName}:{card.CardDescription}:{card.Stats}\n");
                ctx.Db.hero_card.Insert(card);
            }
            var finalJson = sb.ToString();
            ctx.TryInsertValidateInfo(nameof(ValidateTarget.HeroCard), HashUtils.ComputeHash(finalJson));
        }

        [Reducer]
        public static void TryValidateHeroCard(this ReducerContext ctx, List<HeroCard> cards)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var card in cards)
            {
                sb.Append($"{card.HeroCardId}:{card.CardName}:{card.CardDescription}:{card.Stats}\n");
            }
            var finalJson = sb.ToString();
            if (!ctx.Validate(nameof(ValidateTarget.HeroCard), finalJson)) throw new ValidationException("card error");
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