/* ---------------------------------------
 * Author:          Oleg Pshenin (oleg.pshenin@gmail.com)
 * Project:         Raycast Targets Manager (https://github.com/olegpshenin/RaycastTargetsManager)
 * Date:            27-April-19
 * Studio:          Pillbox Game Studio
 * 
 * This project is released under the MIT license.
 * -------------------------------------*/

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace PB
{
    public class RaycastTargetsManager : EditorWindow
    {
        private static Graphic[] _graphicComponents;
        private static Dictionary<Graphic, Selectable> _connectedSelectablesDictionary = new Dictionary<Graphic, Selectable>();
        private static bool[] _toggleArray;
        private static Transform _root;
        private static Vector2 _scrollPos;


        [MenuItem("GameObject/Manage Raycast Targets", false, -3)]
        private static void ManageRaycastTargets(MenuCommand menuCommand)
        {
            if ((menuCommand.context as GameObject) != null)
            {
                _root = (menuCommand.context as GameObject).transform;
            }
            else if (Selection.gameObjects.Length == 1)
            {
                _root = Selection.gameObjects[0].transform;
            }

            if (_root == null)
            {
                Debug.LogError("Select an object at first");
                return;
            }

            InitArrays();
            RefillToggleArray();
            ShowWindow();
        }

        private static void InitArrays()
        {
            _graphicComponents = _root.GetComponentsInChildren<Graphic>(true);

            _connectedSelectablesDictionary.Clear();
            var allSelectables = _root.GetComponentsInChildren<Selectable>(true);
            foreach (var selectable in allSelectables)
            {
                if (selectable.targetGraphic != null)
                {
                    if (!_connectedSelectablesDictionary.ContainsKey(selectable.targetGraphic))
                    {
                        _connectedSelectablesDictionary.Add(selectable.targetGraphic, selectable);
                    }
                }
            }
        }

        private static void ShowWindow()
        {
            var window = GetWindow<RaycastTargetsManager>();
            window.titleContent = new GUIContent("Raycast Targets Manager");
            window.minSize = new Vector2(514, 356);
            window.Show();
        }

        private static void RefillToggleArray()
        {
            _toggleArray = new bool[_graphicComponents.Length];
            for (int i = 0; i < _toggleArray.Length; i++)
            {
                _toggleArray[i] = _graphicComponents[i].raycastTarget;
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("List of all raycast targets inside " + _root.gameObject.name + ":", EditorStyles.boldLabel);

            ShowComponentsList();
            ShowButtons();
        }

        private float GetToogleWidth()
        {
            return 15f;
        }

        private float GetComponentsWidth()
        {
            return GetScalableWidth() * 0.5f;
        }

        private float GetConnectionTextWidth()
        {
            return 100f;
        }

        private float GetConnectionComponentsWidth()
        {
            return GetScalableWidth() * 0.5f;
        }

        private float GetScalableWidth()
        {
            return Screen.width - GetToogleWidth() - GetConnectionTextWidth() - 50f;
        }

        private void ShowComponentsList()
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Width(Screen.width), GUILayout.Height(Screen.height - 90f));
            GUIStyle style = new GUIStyle();
            style.alignment = TextAnchor.MiddleLeft;

            for (int i = 0; i < _graphicComponents.Length; i++)
            {
                EditorGUILayout.BeginHorizontal(style, GUILayout.ExpandWidth(false));

                EditorGUILayout.ObjectField(_graphicComponents[i], typeof(Graphic), false, GUILayout.Width(GetComponentsWidth()));

                if (_connectedSelectablesDictionary.ContainsKey(_graphicComponents[i]))
                {
                    GUILayout.Label("Connected to:", GUILayout.Width(GetConnectionTextWidth()));
                    EditorGUILayout.ObjectField(_connectedSelectablesDictionary[_graphicComponents[i]], typeof(Selectable), false, GUILayout.Width(GetConnectionComponentsWidth()));
                }
                else
                {
                    GUILayout.Label("No connection", GUILayout.Width(GetConnectionTextWidth()));
                    GUILayout.Label("", GUILayout.Width(GetConnectionComponentsWidth()));
                }

                GUILayout.FlexibleSpace();

                _toggleArray[i] = EditorGUILayout.Toggle(_toggleArray[i], GUILayout.Width(GetToogleWidth()));

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }

        private void ShowButtons()
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Enable All"))
            {
                ChangeStateOfAllRaycastTargets(true);
            }

            if (GUILayout.Button("Apply"))
            {
                ApplyRaycastTargetStates();
                Close();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Disable All"))
            {
                ChangeStateOfAllRaycastTargets(false);
            }

            if (GUILayout.Button("Cancel"))
            {
                Close();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void ChangeStateOfAllRaycastTargets(bool state)
        {
            for (int i = 0; i < _toggleArray.Length; i++)
            {
                _toggleArray[i] = state;
            }
        }

        private void ApplyRaycastTargetStates()
        {
            for (int i = 0; i < _graphicComponents.Length; i++)
            {
                Undo.RecordObjects(_graphicComponents, "Applying raycast states");
                _graphicComponents[i].raycastTarget = _toggleArray[i];
            }
        }
    }
}