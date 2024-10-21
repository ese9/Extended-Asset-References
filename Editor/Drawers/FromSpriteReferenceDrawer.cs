using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Nine.AssetReferences.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(FromSpriteAttribute))]
    public class FromSpriteReferenceDrawer : AssetReferenceSpriteDrawer<Texture2D>
    {
        private Object[] sprites;
        private Sprite subSprite;
        private bool isMultipleSprite;

        protected override Object GetAssetForPreview()
        {
            return isMultipleSprite ? subSprite ? subSprite : sprites[0] : MainAsset;
        }

        protected override void OnMainAssetChanged(Texture2D newAsset)
        {
            var path = AssetDatabase.GUIDToAssetPath(MainAssetValue);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            isMultipleSprite = importer.spriteImportMode == SpriteImportMode.Multiple;

            var all = AssetDatabase.LoadAllAssetsAtPath(path);
            // skip main asset
            sprites = new Object[all.Length - 1];

            for (var i = 1; i < all.Length; i++)
            {
                sprites[i - 1] = all[i];
            }
        }

        protected override void OnSubAssetChanged(string newSubAsset)
        {
            subSprite = sprites.FirstOrDefault(x => x.name == newSubAsset) as Sprite;
        }

        protected override IList<Object> GetSubAssets()
        {
            return sprites;
        }

        protected override void OnPreviewClicked()
        {
            if (HasMainAsset)
            {
                EditorGUIUtility.PingObject(MainAsset);
            }
        }
    }
}