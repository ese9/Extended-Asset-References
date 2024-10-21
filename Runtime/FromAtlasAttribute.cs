using System;
using System.Diagnostics;
using UnityEngine;

namespace Nine.AssetReferences
{
    [AttributeUsage(AttributeTargets.Field)]
    [Conditional("UNITY_EDITOR")]
    public class FromAtlasAttribute : PropertyAttribute
    {
    }
}