using System.Collections.Generic;
using Nine.AssetReferences.Editor.Utilities;
using UnityEngine.AddressableAssets;

namespace Nine.AssetReferences.Editor.Validation
{
    public class AssetReferenceValidator
    {
        public virtual void Validate(AssetReferenceSprite reference,
                                     List<ValidationResult> results)
        {
            if (string.IsNullOrEmpty(reference.AssetGUID))
            {
                results.Add(new (ValidationType.Warning, "Main Asset not selected"));
            }
            else if (!AddressableUtility.IsAssetAddressable(reference.AssetGUID))
            {
                results.Add(new ValidationResult(ValidationType.Error,
                                                 "Main Asset not addressable",
                                                 new AssetNotAddressableFixRequest(reference)));
            }

            if (results.Count == 0)
            {
                results.Add(new ValidationResult(ValidationType.Valid, "Asset Reference Sprite is valid"));
            }
        }
    }
}