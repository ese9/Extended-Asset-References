﻿using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using static UnityEditor.AssetDatabase;

namespace Nine.References.Editor.Utilities
{
    public static class AssetDatabaseUtility
    {
        public static string GetAssetGuid(Object obj)
        {
            return AssetPathToGUID(GetAssetPath(obj));
        }

        public static IEnumerable<TObject> FindAssets<TObject>() where TObject : Object
        {
            return AssetDatabase.FindAssets($"t:{typeof(TObject).Name}")
                                .Select(GUIDToAssetPath)
                                .Select(LoadAssetAtPath<TObject>);
        }

        [CanBeNull]
        public static TObject LoadAssetWithGuid<TObject>(string guid) where TObject : Object
        {
            if (string.IsNullOrEmpty(guid))
            {
                return null;
            }

            var path = GUIDToAssetPath(guid);

            return string.IsNullOrEmpty(path) ? null : LoadAssetAtPath<TObject>(path);
        }

        public static void PingObject(string objectName)
        {
            if (string.IsNullOrEmpty(objectName))
            {
                return;
            }

            var foundObject = AssetDatabase.FindAssets(objectName)
                                           .Select(GUIDToAssetPath)
                                           .Select(LoadAssetAtPath<Object>)
                                           .FirstOrDefault();

            if (foundObject)
            {
                EditorGUIUtility.PingObject(foundObject);
            }
        }
    }
}