using Nine.AssetReferences.Editor.Utilities;
using UnityEngine.AddressableAssets;

namespace Nine.AssetReferences.Editor.Validation.Sprites
{
    public class FindNewSpriteAtlasFixRequest : IFixRequest
    {
        private readonly AssetReferenceSprite reference;

        public FindNewSpriteAtlasFixRequest(AssetReferenceSprite reference)
        {
            this.reference = reference;
        }

        public bool Fix()
        {
            if (string.IsNullOrEmpty(reference.SubObjectName))
            {
                return false;
            }

            if (SpriteAtlasUtility.TryFindSpriteAtlasBySpriteName(reference.SubObjectName, out var spriteAtlas))
            {
                var subName = reference.SubObjectName;
                // SetEditorAsset resets sub object name
                reference.SetEditorAsset(spriteAtlas);
                reference.SubObjectName = subName;

                return true;
            }

            return false;
        }
    }
}