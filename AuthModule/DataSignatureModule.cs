using System.ComponentModel.DataAnnotations;
using SpacetimeDB;
using StdbModule.GamePlayModule.CardData;

namespace StdbModule.AuthModule;

public static partial class DataSignatureModule
{
    [Reducer]
    public static void VerifyData(ReducerContext ctx, string target, string play_load)
    {
        var s = ctx.Db.data_signature.ValidateInfoId.Find(target) ?? throw new Exception("Target not found");
        if (!ctx.Validate(nameof(ValidateTarget.HeroCard), play_load)) throw new ValidationException("card error");
        if (!play_load.Equals(s.PlayLoad)) throw new Exception("Verify Failed");
    }
    public static void TryUpdateSignature(this ReducerContext ctx, string target, string play_load = "")
    {
        var s = ctx.Db.data_signature.ValidateInfoId.Find(target).GetValueOrDefault();
        if (s == default)
        {
            ctx.Db.data_signature.Insert(new DataSignature
            {
                ValidateInfoId = target,
                PlayLoad = play_load
            });
        }
        else
        {
            if (target == nameof(ValidateTarget.HeroCard))
            {
                s.PlayLoad = play_load;
            }
            ctx.Db.data_signature.ValidateInfoId.Update(s);
        }
    }
    
    public static string SerializeHeroCardsToBase64(this ReducerContext ctx, List<HeroCard> heroCards)
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);

        var serializer = new HeroCard.BSATN(); // 注意：这里要创建实例

        foreach (var card in heroCards)
        {
            serializer.Write(writer, card); // 使用实例方法
        }

        return Convert.ToBase64String(ms.ToArray());
    }
}