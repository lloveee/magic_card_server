using SpacetimeDB;
using StdbModule.Utils;

namespace StdbModule.AuthModule
{
    //TODO Add Manifest Type To Calculate Hash
    public static partial class ValidateModule
    {
        public static bool Validate(this ReducerContext ctx, string target, string json)
        {
            if (!ctx.TryFindValidateInfo(target, out var validateInfo)) return false;
            return HashUtils.VerifyHash(json, validateInfo.HashCode);
        }
        
        public static bool TryFindValidateInfo(this ReducerContext ctx,string validateTarget, out ValidateInfo validateInfo)
        {
            validateInfo = ctx.Db.validate_info.ValidateInfoId.Find(validateTarget).GetValueOrDefault();
            return validateInfo != default;
        }
        
        public static void TryInsertValidateInfo(this ReducerContext ctx, string target, string hashCode)
        {
            var exist = ctx.Db.validate_info.ValidateInfoId.Find(target).GetValueOrDefault();
            if (exist == default)
            {
                ctx.Db.validate_info.Insert(new ValidateInfo
                {
                    ValidateInfoId = target,
                    HashCode = hashCode
                });
            }
            else
            {
                exist.HashCode = hashCode;
                ctx.Db.validate_info.ValidateInfoId.Update(exist);
            }
        }
    }
}

