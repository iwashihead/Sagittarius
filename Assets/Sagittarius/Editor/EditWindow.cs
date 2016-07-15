using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Griphone.Sagittarius
{
    /// <summary>
    /// 画像の領域を編集するウインドウ.
    /// </summary>
    public class EditWindow : EditorWindow
    {
        #region Static
        private static EditWindow instance;
        public static EditWindow Instance
        {
            get { return instance ?? (instance = GetWindow<EditWindow>(true)); }
        }
        #endregion

        #region Private Declaration

        // 編集対象のデータ.
        // このデータはMainWindowからセットされます.
        public UnitData Current { get; set; }
        // 対象シーン.
        // このデータはMainWindowからセットされます.
        public int SelectedSceneIndex { get; set; }
        // 対象データIndex.
        // このデータはMainWindowからセットされます.
        public int SelectedRectIndex { get; set; }

        private Settings setting
        {
            get { return Settings.Instance; }
        }

        private Vector2 scrollPos;
        private float zomeScale;
        private Vector2 centerPos;

        #endregion

        // ウインドウを開くメニューコマンド.
        [MenuItem("Window/Sagittarius/EditWindow Open")]
        public static void Open()
        {
            instance = Instance;
        }

        // GUI描画イベント.
        public void OnGUI()
        {
        }
    }
}