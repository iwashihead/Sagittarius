using System;
using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Griphone.Sagittarius
{
    public static class SgtEditorUtility
    {
        public static void DrawHorizontal(Action onDraw, params GUILayoutOption[] options)
        {
            EditorGUILayout.BeginHorizontal(options);
            if (onDraw != null) onDraw();
            EditorGUILayout.EndHorizontal();
        }

        public static void DrawHorizontal(Action onDraw, GUIStyle style, params GUILayoutOption[] options)
        {
            EditorGUILayout.BeginHorizontal(style, options);
            if (onDraw != null) onDraw();
            EditorGUILayout.EndHorizontal();
        }

        public static void DrawVertical(Action onDraw, params GUILayoutOption[] options)
        {
            EditorGUILayout.BeginVertical(options);
            if (onDraw != null) onDraw();
            EditorGUILayout.EndVertical();
        }

        public static void DrawVertical(Action onDraw, GUIStyle style, params GUILayoutOption[] options)
        {
            EditorGUILayout.BeginVertical(style, options);
            if (onDraw != null) onDraw();
            EditorGUILayout.EndVertical();
        }
    }
}