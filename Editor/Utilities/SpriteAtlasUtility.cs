using UnityEngine;
using UnityEngine.U2D;

namespace Nine.AssetReferences.Editor.Utilities
{
    public static class SpriteAtlasUtility
    {
        public static bool TryFindSpriteAtlasBySpriteName(string spriteName,
                                                          out SpriteAtlas spriteAtlas,
                                                          out Sprite sprite)
        {
            foreach (var atlas in AssetDatabaseUtility.FindAssets<SpriteAtlas>())
            {
                sprite = atlas.GetSprite(spriteName);

                if (sprite)
                {
                    spriteAtlas = atlas;

                    return true;
                }
            }

            spriteAtlas = null;
            sprite = null;

            return false;
        }

        // GetSprites api returns copy of sprites included in atlas with (Clone) names
        public static Sprite[] GetAtlasSprites(this SpriteAtlas spriteAtlas)
        {
            var arr = new Sprite[spriteAtlas.spriteCount];
            spriteAtlas.GetSprites(arr);

            foreach (var sprite in arr)
            {
                sprite.name = sprite.name.Replace("(Clone)", "");
            }

            return arr;
        }
    }
}