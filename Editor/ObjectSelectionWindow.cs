﻿using System;
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
        private static readonly Color HighlightColor = new(0.2f, 0.4f, 0.8f, 1f);
        private const string NoneLabel = "None";

        public static void Show(string title, IList<Object> objects, Action<Object> onSelected)
        {
            var window = GetWindow<ObjectSelectionWindow>(title);
            window.objectList = objects;
            window.onObjectSelected = onSelected;
            window.selectedObject = null;
            window.highlightedObject = null; // Не выделяем никакой элемент при открытии окна
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

            // Draw None option first (always visible)
            var noneRect = EditorGUILayout.BeginHorizontal(GUILayout.Height(ItemHeight));
            var isNoneHighlighted = highlightedObject == null;

            // Draw highlight border for None option
            if (Event.current.type == EventType.Repaint && isNoneHighlighted)
            {
                DrawHighlightBorder(noneRect);
            }

            // Display None icon (using built-in Unity icon)
            GUILayout.Label(new GUIContent(EditorGUIUtility.FindTexture("GameObject Icon")),
                GUILayout.Width(ItemHeight),
                GUILayout.Height(ItemHeight));

            // Display None button
            if (GUILayout.Button(NoneLabel, EditorStyles.label, GUILayout.Height(ItemHeight)))
            {
                if (isNoneHighlighted)
                {
                    selectedObject = null;
                    onObjectSelected?.Invoke(null);
                    onObjectSelected = null;
                    Close();
                }
                else
                {
                    highlightedObject = null;
                }
            }

            EditorGUILayout.EndHorizontal();

            // Draw other objects
            var filteredList = objectList.Where(obj => string.IsNullOrEmpty(searchString) ||
                                                     obj.name.ToLower().Contains(searchString.ToLower()))
                                       .ToList();

            foreach (var obj in filteredList)
            {
                var isHighlighted = obj == highlightedObject;

                var itemRect = EditorGUILayout.BeginHorizontal(GUILayout.Height(ItemHeight));

                // Draw highlight border
                if (Event.current.type == EventType.Repaint && isHighlighted)
                {
                    DrawHighlightBorder(itemRect);
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
            else if (highlightedObject == null)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Selected: None", EditorStyles.boldLabel);
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