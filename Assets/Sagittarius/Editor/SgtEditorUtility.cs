using System;
using UnityEngine;
using UnityEditor;

namespace Griphone.Sagittarius
{
    /// <summary>
    /// サジタリウス用エディタユーティリティー
    /// </summary>
    public static class SgtEditorUtility
    {
        /// <summary>
        /// 横レイアウトブロックを開始します
        /// </summary>
        public static void HorizontalBlock(Action onDraw, params GUILayoutOption[] options)
        {
            EditorGUILayout.BeginHorizontal(options);
            if (onDraw != null) onDraw();
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 横レイアウトブロックを開始します
        /// </summary>
        public static void HorizontalBlock(Action onDraw, GUIStyle style, params GUILayoutOption[] options)
        {
            EditorGUILayout.BeginHorizontal(style, options);
            if (onDraw != null) onDraw();
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 縦レイアウトブロックを開始します
        /// </summary>
        public static void VerticalBlock(Action onDraw, params GUILayoutOption[] options)
        {
            EditorGUILayout.BeginVertical(options);
            if (onDraw != null) onDraw();
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 縦レイアウトブロックを開始します
        /// </summary>
        public static void VerticalBlock(Action onDraw, GUIStyle style, params GUILayoutOption[] options)
        {
            EditorGUILayout.BeginVertical(style, options);
            if (onDraw != null) onDraw();
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// ウインドウ内領域に制限をかけます.
        /// </summary>
        /// <param name="window">ウインドウ領域</param>
        /// <param name="constraintsSize">制限領域</param>
        /// <param name="editorWindow">エディタウインドウ</param>
        /// <returns>制限されたウインドウ領域を返します</returns>
        public static Rect ConstrainRect(Rect window, Rect constraintsSize, EditorWindow editorWindow)
        {
            window.x = Mathf.Clamp(window.x, editorWindow.position.x - constraintsSize.x, constraintsSize.width - window.width);
            window.y = Mathf.Clamp(window.y, editorWindow.position.y - constraintsSize.y, constraintsSize.height - window.height);
            return window;
        }
    }
}