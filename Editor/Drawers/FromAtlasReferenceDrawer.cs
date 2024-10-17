using System;
using System.Linq;
using Nine.References.Editor.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;
using Object = UnityEngine.Object;

// TODO: add Undo

namespace Nine.References.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(FromAtlasAttribute))]
    public class FromAtlasReferenceDrawer : PropertyDrawer
    {
        private enum MessageType
        {
            Normal = 0, Warning = 1, Error = 2
        }

        private const float ButtonWidth = 48;

        private static readonly float PropertyHeight = EditorGUIUtility.singleLineHeight * 3;
        private static readonly float SingleLineHeight = EditorGUIUtility.singleLineHeight;

        private SerializedProperty atlasProperty;
        private SerializedProperty spriteNameProperty;

        private SpriteAtlas selectedAtlas;
        private Sprite selectedSprite;
        private Sprite[] atlasSprites;

        private GUIStyle spriteLabelStyle;
        private GUIStyle validLabelStyle;
        private GUIStyle warningLabelStyle;
        private GUIStyle errorLabelStyle;

        private bool isInitialized;
        private string cachedSpriteName;

        private string SelectedAtlasGuid => atlasProperty?.stringValue;
        private string SelectedSpriteName => spriteNameProperty?.stringValue;

        private bool HasAtlas => !string.IsNullOrEmpty(SelectedAtlasGuid);
        private bool HasSprite => !string.IsNullOrEmpty(SelectedSpriteName);

        private bool SpriteHasLinkError => HasSprite && !selectedSprite;
        private bool AtlasIsNotAddressable => !AddressableUtility.IsAssetAddressable(selectedAtlas);

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return isInitialized ? PropertyHeight + 2 : SingleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!TryInitialize(property))
            {
                // ReSharper disable once Unity.PropertyDrawerOnGUIBase
                base.OnGUI(position, property, label);

                return;
            }

            EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            var previewRect = position.Split(0, 2)
                                      .SetSize(PropertyHeight, PropertyHeight);

            var controlRect = position.Split(1, 2)
                                      .SetXMin(previewRect.xMax + 1);

            var atlasRect = controlRect.SplitVertical(0, 3)
                                       .SetHeight(SingleLineHeight);

            var newAtlas = (SpriteAtlas)EditorGUI.ObjectField(atlasRect, selectedAtlas, typeof(SpriteAtlas), false);

            if (selectedAtlas != newAtlas)
            {
                SetAtlas(newAtlas);
            }

            var spritePopupRect = controlRect.SplitVertical(1, 3)
                                             .SetYMin(atlasRect.yMax + 1)
                                             .SetHeight(SingleLineHeight);

            if (HasAtlas)
            {
                DrawSprite(spritePopupRect);
            }

            DrawSpritePreview(previewRect, selectedSprite);

            var infoRect = controlRect.SplitVertical(2, 3)
                                      .SetYMin(spritePopupRect.yMax)
                                      .SetHeight(SingleLineHeight);

            DrawInfo(infoRect);

            EditorGUI.EndProperty();
        }

        private bool TryInitialize(SerializedProperty property)
        {
            if (isInitialized)
            {
                return true;
            }

            atlasProperty = property.FindPropertyRelative("m_AssetGUID");
            spriteNameProperty = property.FindPropertyRelative("m_SubObjectName");

            if (atlasProperty == null || spriteNameProperty == null)
            {
                return isInitialized = false;
            }

            spriteLabelStyle = new GUIStyle(EditorStyles.label) { fontStyle = FontStyle.Italic };
            validLabelStyle = new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = Color.green } };
            warningLabelStyle = new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = Color.yellow } };
            errorLabelStyle = new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = Color.red } };

            if (!string.IsNullOrEmpty(SelectedAtlasGuid))
            {
                var atlas = AssetDatabaseUtility.LoadAssetWithGuid<SpriteAtlas>(SelectedAtlasGuid);

                SetAtlas(atlas);
            }

            return isInitialized = true;
        }

        private void DrawSprite(Rect rect)
        {
            var selectXMin = rect.xMax - ButtonWidth;

            var spriteLabelRect = rect.Split(0, 2)
                                      .HorizontalPadding(4, 0)
                                      .SetWidth(selectXMin - rect.xMin - SingleLineHeight);

            var spriteSelectRect = rect.Split(1, 2)
                                       .SetXMin(spriteLabelRect.xMax + SingleLineHeight - 4)
                                       .SetWidth(ButtonWidth);

            EditorGUI.LabelField(spriteLabelRect,
                                 HasSprite ? SelectedSpriteName : "None",
                                 spriteLabelStyle);

            if (GUI.Button(spriteSelectRect, "Select"))
            {
                ObjectSelectionWindow.Show(atlasSprites.Cast<Object>().ToArray(),
                                           s =>
                                           {
                                               selectedSprite = s as Sprite;
                                               cachedSpriteName = selectedSprite?.name ?? string.Empty;
                                           });
            }

            // it doesn't set properly if changing stringValue from callback directly
            if (cachedSpriteName != null && cachedSpriteName != SelectedSpriteName)
            {
                spriteNameProperty.stringValue = cachedSpriteName;
                cachedSpriteName = null;
            }
        }

        private void DrawInfo(Rect rect)
        {
            if (!HasAtlas)
            {
                DrawInfo(rect, "Atlas not selected", MessageType.Warning);
            }
            else if (SpriteHasLinkError)
            {
                DrawInfo(rect,
                         "Sprite not found in atlas",
                         MessageType.Error,
                         () =>
                         {
                             if (SpriteAtlasUtility.TryFindSpriteAtlasBySpriteName(SelectedSpriteName,
                                                                                   out var foundAtlas,
                                                                                   out var foundSprite))
                             {
                                 SetAtlas(foundAtlas);
                                 selectedSprite = foundSprite;
                             }
                         });
            }
            else if (AtlasIsNotAddressable)
            {
                DrawInfo(rect,
                         "Sprite Atlas is not addressable",
                         MessageType.Warning,
                         () => AddressableUtility.MarkAsAddressable(selectedAtlas));
            }
            else if (!HasSprite)
            {
                DrawInfo(rect, "Sprite not selected", MessageType.Warning);
            }
            else
            {
                DrawInfo(rect, "Asset Reference Sprite is valid", MessageType.Normal);
            }
        }

        private void DrawInfo(Rect rect, string message, MessageType type, Action callback = null)
        {
            var (icon, textStyle) = GetMessageStyle(type);

            var iconRect = rect.Split(0, 3)
                               .SetWidth(SingleLineHeight);

            var validContent = EditorGUIUtility.IconContent(icon);
            EditorGUI.LabelField(iconRect, validContent);

            var labelRect = rect.Split(1, 3)
                                .SetXMin(iconRect.xMax)
                                .SetWidth(rect.width - SingleLineHeight - ButtonWidth);

            var buttonRect = rect.Split(2, 3)
                                 .SetXMin(labelRect.xMax)
                                 .SetWidth(ButtonWidth);

            EditorGUI.LabelField(labelRect, message, textStyle);

            if (callback != null && GUI.Button(buttonRect, "Fix"))
            {
                callback.Invoke();
            }
        }

        private void DrawSpritePreview(Rect rect, Sprite sprite)
        {
            var id = GUIUtility.GetControlID(FocusType.Passive, rect);
            var currentEvent = Event.current;

            if (currentEvent.type == EventType.Repaint)
            {
                EditorStyles.objectFieldThumb.Draw(rect, GUIContent.none, id);
            }

            if (sprite)
            {
                var texture = AssetPreview.GetAssetPreview(sprite);
                EditorGUI.DrawTextureTransparent(rect.Padding(2), texture, ScaleMode.ScaleAndCrop);

                if (currentEvent.type == EventType.MouseDown &&
                    rect.Contains(currentEvent.mousePosition))
                {
                    AssetDatabaseUtility.PingObject(SelectedSpriteName);
                    currentEvent.Use();
                }
            }
        }

        private void SetAtlas(SpriteAtlas newAtlas)
        {
            var newGuid = string.Empty;

            if (newAtlas)
            {
                newGuid = AssetDatabaseUtility.GetAssetGuid(newAtlas);
                atlasSprites = newAtlas.GetAtlasSprites();

                if (!string.IsNullOrEmpty(SelectedSpriteName))
                {
                    selectedSprite = newAtlas.GetSprite(SelectedSpriteName);
                }
            }
            else
            {
                selectedSprite = null;
            }

            atlasProperty.stringValue = newGuid;
            selectedAtlas = newAtlas;

            if (!selectedSprite)
            {
                spriteNameProperty.stringValue = string.Empty;
            }
        }

        private (string icon, GUIStyle text) GetMessageStyle(MessageType type)
        {
            return type switch
                   {
                       MessageType.Normal => (UnityEditorIcons.GreenLight, validLabelStyle),
                       MessageType.Warning => (UnityEditorIcons.OrangeLight, warningLabelStyle),
                       MessageType.Error => (UnityEditorIcons.RedLight, errorLabelStyle),
                       _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
                   };
        }
    }
}