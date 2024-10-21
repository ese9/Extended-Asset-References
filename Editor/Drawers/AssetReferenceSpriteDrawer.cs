using System;
using System.Collections.Generic;
using System.Linq;
using Nine.AssetReferences.Editor.Utilities;
using Nine.AssetReferences.Editor.Validation;
using Nine.AssetReferences.Editor.Validation.Sprites;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

namespace Nine.AssetReferences.Editor.Drawers
{
    // TODO: add Undo
    // TODO: add multiselection handle
    public class AssetReferenceSpriteDrawer<T> : PropertyDrawer where T : Object
    {
        private bool isInitialized;
        private string cachedSubAssetName;
        private SerializedObject serializedObject;
        private SerializedProperty mainAssetProperty;
        private SerializedProperty subAssetProperty;
        private AssetReferenceSpriteValidator validator;
        private List<ValidationResult> validationResults;

        private GUIStyle subAssetLabelStyle;
        private GUIStyle validLabelStyle;
        private GUIStyle warningLabelStyle;
        private GUIStyle errorLabelStyle;

        private const float ButtonWidth = 48;
        private static readonly float PropertyHeight = EditorGUIUtility.singleLineHeight * 3;
        private static readonly float SingleLineHeight = EditorGUIUtility.singleLineHeight;

        protected T MainAsset { get; private set; }
        protected string MainAssetValue => mainAssetProperty?.stringValue;
        protected string SubAssetValue => subAssetProperty?.stringValue;
        protected bool HasMainAsset => !string.IsNullOrEmpty(MainAssetValue);
        protected bool HasSubAsset => !string.IsNullOrEmpty(SubAssetValue);

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

            var mainAssetRect = controlRect.SplitVertical(0, 3)
                                           .SetHeight(SingleLineHeight);

            var subAssetRect = controlRect.SplitVertical(1, 3)
                                          .SetYMin(mainAssetRect.yMax + 1)
                                          .SetHeight(SingleLineHeight);

            var infoRect = controlRect.SplitVertical(2, 3)
                                      .SetYMin(subAssetRect.yMax)
                                      .SetHeight(SingleLineHeight);

            DrawPreview(previewRect, GetAssetForPreview());
            DrawMainAsset(mainAssetRect);

            if (HasMainAsset)
            {
                DrawSubAsset(subAssetRect);
            }

            ValidateSpriteReference(property);
            DrawValidationResults(infoRect);

            EditorGUI.EndProperty();
        }

        private void ValidateSpriteReference(SerializedProperty property)
        {
            var targetObject = property.serializedObject.targetObject;
            var targetObjectClassType = targetObject.GetType();
            var field = targetObjectClassType.GetField(property.propertyPath);

            if (field != null)
            {
                var value = field.GetValue(targetObject);
                validationResults.Clear();
                validator.Validate((AssetReferenceSprite)value, validationResults);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return isInitialized ? PropertyHeight + 2 : SingleLineHeight;
        }

        private bool TryInitialize(SerializedProperty property)
        {
            if (isInitialized)
            {
                return true;
            }

            mainAssetProperty = property.FindPropertyRelative("m_AssetGUID");
            subAssetProperty = property.FindPropertyRelative("m_SubObjectName");

            if (mainAssetProperty == null || subAssetProperty == null)
            {
                return isInitialized = false;
            }

            serializedObject = property.serializedObject;
            validator = new AssetReferenceSpriteValidator();
            validationResults = new List<ValidationResult>();
            subAssetLabelStyle = new GUIStyle(EditorStyles.label) { fontStyle = FontStyle.Italic };
            validLabelStyle = new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = Color.green } };
            warningLabelStyle = new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = Color.yellow } };
            errorLabelStyle = new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = Color.red } };

            Reinitialize();

            return isInitialized = true;
        }

        private void Reinitialize()
        {
            if (HasMainAsset)
            {
                SetMainAsset(AssetDatabaseUtility.LoadAssetWithGuid<T>(MainAssetValue));
            }

            Initialize();
        }

        protected virtual void Initialize()
        {
        }

        protected virtual Object GetAssetForPreview()
        {
            return null;
        }

        protected virtual IList<Object> GetSubAssets()
        {
            return Array.Empty<Object>();
        }

        protected virtual void OnMainAssetChanged(T newAsset)
        {
        }

        protected virtual void OnSubAssetChanged(string newSubAsset)
        {
        }

        protected virtual void OnPreviewClicked()
        {
        }

        private void DrawMainAsset(Rect rect)
        {
            var newAsset = (T)EditorGUI.ObjectField(rect, MainAsset, typeof(T), false);

            if (MainAsset != newAsset)
            {
                SetMainAsset(newAsset);
                SetSubAsset(string.Empty);
            }
        }

        private void DrawSubAsset(Rect rect)
        {
            var selectXMin = rect.xMax - ButtonWidth;

            var spriteLabelRect = rect.Split(0, 2)
                                      .HorizontalPadding(4, 0)
                                      .SetWidth(selectXMin - rect.xMin - SingleLineHeight);

            var spriteSelectRect = rect.Split(1, 2)
                                       .SetXMin(spriteLabelRect.xMax + SingleLineHeight - 4)
                                       .SetWidth(ButtonWidth);

            EditorGUI.LabelField(spriteLabelRect,
                                 HasSubAsset ? SubAssetValue : "Not selected",
                                 subAssetLabelStyle);

            if (GUI.Button(spriteSelectRect, "Select"))
            {
                ObjectSelectionWindow.Show("Select sub asset",
                                           GetSubAssets(),
                                           s => { cachedSubAssetName = s?.name ?? string.Empty; });
            }

            // it doesn't set properly if changing stringValue from callback directly
            if (cachedSubAssetName != null && cachedSubAssetName != SubAssetValue)
            {
                SetSubAsset(cachedSubAssetName);
                cachedSubAssetName = null;
            }
        }

        private void DrawValidationResults(Rect rect)
        {
            var result = validationResults.OrderByDescending(x => x.Type).First();
            DrawValidationResult(rect, result);
        }

        private void DrawValidationResult(Rect rect, ValidationResult result)
        {
            var (icon, textStyle) = GetMessageStyle(result.Type);

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

            EditorGUI.LabelField(labelRect, result.Message, textStyle);

            if (result.Request != null && GUI.Button(buttonRect, "Fix"))
            {
                if (result.Request.Fix())
                {
                    serializedObject.Update();
                    Reinitialize();
                    EditorUtility.SetDirty(serializedObject.targetObject);
                }
            }
        }

        protected void SetMainAsset(T newAsset)
        {
            var newGuid = string.Empty;

            if (newAsset)
            {
                newGuid = AssetDatabaseUtility.GetAssetGuid(newAsset);
            }

            mainAssetProperty.stringValue = newGuid;
            MainAsset = newAsset;
            OnMainAssetChanged(MainAsset);
        }

        protected void SetSubAsset(string subAsset)
        {
            subAssetProperty.stringValue = subAsset;
            OnSubAssetChanged(subAsset);
        }

        private void DrawPreview(Rect rect, Object asset)
        {
            var id = GUIUtility.GetControlID(FocusType.Passive, rect);
            var currentEvent = Event.current;

            if (currentEvent.type == EventType.Repaint)
            {
                EditorStyles.objectFieldThumb.Draw(rect, GUIContent.none, id);
            }

            if (asset)
            {
                var texture = AssetPreview.GetAssetPreview(asset);
                EditorGUI.DrawTextureTransparent(rect.Padding(2), texture, ScaleMode.ScaleAndCrop, 1f);

                if (currentEvent.type == EventType.MouseDown &&
                    rect.Contains(currentEvent.mousePosition))
                {
                    OnPreviewClicked();
                    currentEvent.Use();
                }
            }
        }

        private (string icon, GUIStyle text) GetMessageStyle(ValidationType type)
        {
            return type switch
                   {
                       ValidationType.Valid => (UnityEditorIcons.GreenLight, validLabelStyle),
                       ValidationType.Warning => (UnityEditorIcons.OrangeLight, warningLabelStyle),
                       ValidationType.Error => (UnityEditorIcons.RedLight, errorLabelStyle),
                       _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
                   };
        }
    }
}