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
    // TODO: draw multiselection
    public class AssetReferenceSpriteDrawer<T> : PropertyDrawer where T : Object
    {
        private bool isInitialized;
        private string cachedSubAssetName;
        private SerializedObject serializedObject;
        private SerializedProperty mainAssetProperty;
        private SerializedProperty subAssetProperty;
        private AssetReferenceSprite reference;
        private AssetReferenceSpriteValidator validator;
        private List<ValidationResult> validationResults;

        private GUIStyle subAssetLabelStyle;
        private GUIStyle validLabelStyle;
        private GUIStyle warningLabelStyle;
        private GUIStyle errorLabelStyle;

        private const float ButtonWidth = 48;

        private float SingleLineHeight => EditorGUIUtility.singleLineHeight;
        private float PreviewSize => EditorGUIUtility.singleLineHeight * 3;

        protected T MainAsset { get; private set; }
        protected string SubAsset { get; private set; }
        protected string MainAssetGuid => mainAssetProperty?.stringValue;
        protected string SubAssetName => subAssetProperty?.stringValue;
        protected bool HasMainAsset => !string.IsNullOrEmpty(MainAssetGuid);
        protected bool HasSubAsset => !string.IsNullOrEmpty(SubAssetName);

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!TryInitialize(property))
            {
                // ReSharper disable once Unity.PropertyDrawerOnGUIBase
                base.OnGUI(position, property, label);

                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position,
                                             GUIUtility.GetControlID(FocusType.Passive),
                                             label);

            var previewRect = position.Split(0, 2)
                                      .SetSize(PreviewSize, PreviewSize);

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

            DrawValidationResults(infoRect);
            CheckUndo();

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return PreviewSize + 2;
        }

        private bool TryInitialize(SerializedProperty property)
        {
            mainAssetProperty = property.FindPropertyRelative("m_AssetGUID");
            subAssetProperty = property.FindPropertyRelative("m_SubObjectName");

            if (mainAssetProperty == null || subAssetProperty == null)
            {
                return isInitialized = false;
            }

            if (isInitialized)
            {
                return true;
            }

            validator = new AssetReferenceSpriteValidator();
            validationResults = new List<ValidationResult>();
            subAssetLabelStyle = new GUIStyle(EditorStyles.label) { fontStyle = FontStyle.Italic };
            validLabelStyle = new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = Color.green } };
            warningLabelStyle = new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = Color.yellow } };
            errorLabelStyle = new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = Color.red } };

            serializedObject = property.serializedObject;
            var targetObject = serializedObject.targetObject;
            var targetObjectClassType = targetObject.GetType();
            var field = targetObjectClassType.GetField(property.propertyPath);
            var value = field.GetValue(targetObject);
            reference = (AssetReferenceSprite)value;

            Reinitialize();

            SubAsset = SubAssetName;

            return isInitialized = true;
        }

        private void Reinitialize()
        {
            if (HasMainAsset)
            {
                SetMainAsset(AssetDatabaseUtility.LoadAssetWithGuid<T>(MainAssetGuid));
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

            ChangeMainAsset(newAsset);
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
                                 HasSubAsset ? SubAssetName : "Not selected",
                                 subAssetLabelStyle);

            if (GUI.Button(spriteSelectRect, "Select"))
            {
                ObjectSelectionWindow.Show("Select sub asset",
                                           GetSubAssets(),
                                           s => { cachedSubAssetName = s?.name ?? string.Empty; });
            }

            // it doesn't set properly if changing stringValue from callback directly
            if (cachedSubAssetName != null && cachedSubAssetName != SubAssetName)
            {
                SetSubAsset(cachedSubAssetName);
                cachedSubAssetName = null;
            }
        }

        private void CheckUndo()
        {
            var mainAssetChanged = AssetDatabaseUtility.GetAssetGuid(MainAsset) != MainAssetGuid;

            if (mainAssetChanged)
            {
                SetMainAsset(AssetDatabaseUtility.LoadAssetWithGuid<T>(MainAssetGuid));
                SetSubAsset(SubAssetName);
            }

            var subAssetChanged = SubAsset != SubAssetName;

            if (subAssetChanged)
            {
                SetSubAsset(SubAssetName);
            }
        }

        private void DrawValidationResults(Rect rect)
        {
            validationResults.Clear();
            validator.Validate(reference, validationResults);
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

        private void ChangeMainAsset(T newAsset)
        {
            if (MainAsset != newAsset)
            {
                SetMainAsset(newAsset);
                SetSubAsset(string.Empty);
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
            SubAsset = subAsset;
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