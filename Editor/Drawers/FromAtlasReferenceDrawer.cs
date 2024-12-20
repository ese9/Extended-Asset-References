﻿using System.Collections.Generic;
using System.Linq;
using Nine.AssetReferences.Editor.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;
using Object = UnityEngine.Object;

namespace Nine.AssetReferences.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(FromAtlasAttribute))]
    public class FromAtlasReferenceDrawer : AssetReferenceSpriteDrawer<SpriteAtlas>
    {
        private Sprite selectedSprite;
        private Sprite[] atlasSprites;

        protected override void Initialize()
        {
            if (HasMainAsset && HasSubAsset)
            {
                selectedSprite = MainAsset.GetSprite(SubAssetName);
            }
        }

        protected override Object GetAssetForPreview()
        {
            return selectedSprite;
        }

        protected override IList<Object> GetSubAssets()
        {
            return atlasSprites.Cast<Object>().ToArray();
        }

        protected override void OnMainAssetChanged(SpriteAtlas newAsset)
        {
            if (newAsset)
            {
                atlasSprites = newAsset.GetAtlasSprites();
            }
        }

        protected override void OnSubAssetChanged(string newSubAsset)
        {
            selectedSprite = !string.IsNullOrEmpty(newSubAsset)
                ? MainAsset.GetSprite(newSubAsset)
                : null;
        }

        protected override void OnPreviewClicked()
        {
            AssetDatabaseUtility.PingObject(SubAssetName);
        }
    }
}