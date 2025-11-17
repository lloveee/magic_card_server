using System.ComponentModel.DataAnnotations;
using System.Text;
using SpacetimeDB;
using StdbModule.AuthModule;
using StdbModule.Utils;

namespace StdbModule.GamePlayModule.CardData
{
    public static partial class CardModule
    {
        #region Const Value

        public static readonly Dictionary<CardFaction, float> CardFactionWeightDefault = new Dictionary<CardFaction, float>
        {
            [CardFaction.Dream]   = 1f,
            [CardFaction.Illusion] = 1f,
            [CardFaction.Rebirth] = 1f,
            [CardFaction.Reshape] = 1f
        };
        
        public static readonly Dictionary<CardType, float> CardTypeWeightDefault = new Dictionary<CardType, float>
        {
            [CardType.Attack]   = 1f,
            [CardType.Energy] = 1f,
            [CardType.Skill] = 1f,
        };
        
        public static readonly Dictionary<uint, float> CardLevelWeightDefault = new Dictionary<uint, float>
        {
            [1u]   = 0.48f,
            [2u] = 0.28f,
            [3u] = 0.14f,
            [4u] = 0.07f,
            [5u] = 0.03f,
        };

        #endregion
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

        public static bool TryGetCardWeight(this ReducerContext ctx, BaseCard card, uint match_id, out float weight)
        {
            var faction_weight = ctx.Db.card_faction_weight.InstanceId.Find($"{match_id}_{card.CardFaction}").GetValueOrDefault();
            var type_weight = ctx.Db.card_type_weight.InstanceId.Find($"{match_id}_{card.CardType}").GetValueOrDefault();
            var level_weight = ctx.Db.card_level_weight.InstanceId.Find($"{match_id}_{card.CardLevel}").GetValueOrDefault();
            if (faction_weight == default || type_weight == default || level_weight == default)
            {
                weight = 0;
                return false;
            }
            else
            {
                weight = faction_weight.Weight + type_weight.Weight + level_weight.Weight;
                return true;
            }
        }

        public static void TrySetCardWeight(this ReducerContext ctx, uint match_id, CardType type, float weight)
        {
            var a = ctx.Db.card_type_weight.InstanceId.Find($"{match_id}_{type}").GetValueOrDefault();
            if (a == default)
            {
                ctx.Db.card_type_weight.Insert(new CardTypeWeight
                {
                    InstanceId = $"{match_id}_{type}",
                    Weight = weight,
                });
            }
            else
            {
                a.Weight = weight;
                ctx.Db.card_type_weight.InstanceId.Update(a);
            }
        }

        public static void TryRemoveCardWeight(this ReducerContext ctx, uint match_id, CardType type)
        {
            ctx.Db.card_type_weight.InstanceId.Delete($"{match_id}_{type}");
        }
        public static void TrySetCardWeight(this ReducerContext ctx, uint match_id, CardFaction faction, float weight)
        {
            var a = ctx.Db.card_type_weight.InstanceId.Find($"{match_id}_{faction}").GetValueOrDefault();
            if (a == default)
            {
                ctx.Db.card_type_weight.Insert(new CardTypeWeight
                {
                    InstanceId = $"{match_id}_{faction}",
                    Weight = weight,
                });
            }
            else
            {
                a.Weight = weight;
                ctx.Db.card_type_weight.InstanceId.Update(a);
            }
        }

        public static void TryRemoveCardWeight(this ReducerContext ctx, uint match_id, CardFaction faction)
        {
            ctx.Db.card_faction_weight.InstanceId.Delete($"{match_id}_{faction}");
        }
        
        public static void TrySetCardWeight(this ReducerContext ctx, uint match_id, uint level, float weight)
        {
            var a = ctx.Db.card_type_weight.InstanceId.Find($"{match_id}_{level}").GetValueOrDefault();
            if (a == default)
            {
                ctx.Db.card_type_weight.Insert(new CardTypeWeight
                {
                    InstanceId = $"{match_id}_{level}",
                    Weight = weight,
                });
            }
            else
            {
                a.Weight = weight;
                ctx.Db.card_type_weight.InstanceId.Update(a);
            }
        }

        public static void TryRemoveCardWeight(this ReducerContext ctx, uint match_id, uint level)
        {
            ctx.Db.card_level_weight.InstanceId.Delete($"{match_id}_{level}");
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