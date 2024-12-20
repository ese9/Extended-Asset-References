﻿using UnityEditor.AddressableAssets;
using UnityEngine;

namespace Nine.AssetReferences.Editor.Utilities
{
    internal static class AddressableUtility
    {
        public static bool IsAssetAddressable(Object asset)
        {
            return IsAssetAddressable(AssetDatabaseUtility.GetAssetGuid(asset));
        }

        public static bool IsAssetAddressable(string guid)
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            var entry = settings.FindAssetEntry(guid);

            return entry != null;
        }

        public static void MarkAsAddressable(Object obj)
        {
            var guid = AssetDatabaseUtility.GetAssetGuid(obj);

            if (IsAssetAddressable(guid))
            {
                Debug.LogWarning($"{obj.name} is already addressable");

                return;
            }

            var settings = AddressableAssetSettingsDefaultObject.Settings;
            settings.CreateOrMoveEntry(guid, settings.DefaultGroup);
        }
    }
}