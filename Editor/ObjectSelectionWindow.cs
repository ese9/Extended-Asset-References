using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Nine.AssetReferences.Editor
{
    public class ObjectSelectionWindow : EditorWindow
    {
        private IList<Object> objectList;
        private Vector2 scrollPosition;
        private string searchString = "";
        private Object selectedObject;
        private Object highlightedObject;
        private Action<Object> onObjectSelected;

        private const float ItemHeight = 30f;
        private const float BorderWidth = 2f;
        private static readonly Color HighlightColor = new (0.2f, 0.4f, 0.8f, 1f);

        public static void Show(IList<Object> objects, Action<Object> onSelected)
        {
            var window = GetWindow<ObjectSelectionWindow>("Custom Object Selector");
            window.objectList = objects;
            window.onObjectSelected = onSelected;
            window.selectedObject = null;
            window.highlightedObject = null;
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();

            // Search field
            GUI.SetNextControlName("SearchField");
            var newSearchString = EditorGUILayout.TextField("Search", searchString);

            if (newSearchString != searchString)
            {
                searchString = newSearchString;
                GUI.FocusControl("SearchField");
            }

            // Scrollable area
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            var filteredList = objectList.Where(obj => string.IsNullOrEmpty(searchString) ||
                                                       obj.name.ToLower().Contains(searchString.ToLower()))
                                         .ToList();

            foreach (var obj in filteredList)
            {
                var isHighlighted = obj == highlightedObject;

                var rect = EditorGUILayout.BeginHorizontal(GUILayout.Height(ItemHeight));

                // Draw highlight border
                if (Event.current.type == EventType.Repaint && isHighlighted)
                {
                    DrawHighlightBorder(rect);
                }

                // Display object icon
                var icon = GetObjectIcon(obj);

                GUILayout.Label(new GUIContent(icon),
                                GUILayout.Width(ItemHeight),
                                GUILayout.Height(ItemHeight));

                // Display object name as a selectable button
                if (GUILayout.Button(obj.name, EditorStyles.label, GUILayout.Height(ItemHeight)))
                {
                    if (highlightedObject == obj)
                    {
                        selectedObject = obj;
                        onObjectSelected?.Invoke(selectedObject);
                        onObjectSelected = null;
                        Close();
                    }
                    else
                    {
                        highlightedObject = obj;
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();

            // Display instructions
            EditorGUILayout.HelpBox("Click once to highlight, click again to select.", MessageType.Info);

            // Display selected object
            if (highlightedObject != null)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Selected: " + highlightedObject.name, EditorStyles.boldLabel);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawHighlightBorder(Rect rect)
        {
            EditorGUI.DrawRect(new Rect(rect.x + BorderWidth,
                                        rect.y,
                                        rect.width - BorderWidth * 2,
                                        BorderWidth),
                               HighlightColor); // Top

            EditorGUI.DrawRect(new Rect(rect.x + BorderWidth,
                                        rect.yMax - BorderWidth,
                                        rect.width - BorderWidth * 2,
                                        BorderWidth),
                               HighlightColor); // Bottom

            EditorGUI.DrawRect(new Rect(rect.x,
                                        rect.y + BorderWidth,
                                        BorderWidth,
                                        rect.height - 2 * BorderWidth),
                               HighlightColor); // Left

            EditorGUI.DrawRect(new Rect(rect.xMax - BorderWidth,
                                        rect.y + BorderWidth,
                                        BorderWidth,
                                        rect.height - 2 * BorderWidth),
                               HighlightColor); // Right
        }

        private Texture2D GetObjectIcon(Object obj)
        {
            if (obj is Sprite sprite)
            {
                return AssetPreview.GetAssetPreview(sprite);
            }

            return AssetPreview.GetMiniThumbnail(obj);
        }
    }
}