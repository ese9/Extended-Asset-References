using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.U2D;

namespace Nine.AssetReferences.Editor.Validation.Sprites
{
    public class AssetReferenceSpriteValidator : AssetReferenceValidator
    {
        public override void Validate(AssetReferenceSprite reference, List<ValidationResult> results)
        {
            if (reference.editorAsset != null &&
                reference.editorAsset is SpriteAtlas atlas)
            {
                if (!string.IsNullOrEmpty(reference.SubObjectName))
                {
                    var sprite = atlas.GetSprite(reference.SubObjectName);

                    if (!sprite)
                    {
                        results.Add(new ValidationResult(ValidationType.Error,
                                                         "Sprite not found in atlas",
                                                         new FindNewSpriteAtlasFixRequest(reference)));
                    }
                }
                else
                {
                    results.Add(new ValidationResult(ValidationType.Error,
                                                     "Sprite not selected"));
                }
            }

            base.Validate(reference, results);
        }
    }
}