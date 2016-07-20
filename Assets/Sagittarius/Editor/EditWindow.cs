using System;
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
        private const string FrameTexPath = "Assets/Sagittarius/Textures/black_8x8.png";
        private const string TargetAreaTexPath = "Assets/Sagittarius/Textures/targetArea.png";

        #region Static
        private static EditWindow instance;
        public static EditWindow Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = GetWindow<EditWindow>(true);
                    EditorApplication.update = instance.Update;
                }
                return instance;
            }
        }

        private static Texture2D frameTex;
        private static Texture2D FrameTex
        {
            get
            {
                return frameTex ??
                       (frameTex = AssetDatabase.LoadAssetAtPath<Texture2D>(FrameTexPath));
            }
        }

        private static Texture2D targetAreaTex;
        private static Texture2D TargetAreaTex
        {
            get
            {
                return targetAreaTex ??
                       (targetAreaTex = AssetDatabase.LoadAssetAtPath<Texture2D>(TargetAreaTexPath));
            }
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
        // プレビューモードかどうか.
        // このデータはMainWindowからセットされます.
        public bool IsPreviewMode { get; set; }

        // 編集中の領域データ.
        public RectData CurrentRect
        {
            get
            {
                try
                {
                    return Current.sceneList[SelectedSceneIndex].rectData[SelectedRectIndex];
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e);
                }
                return null;
            }
        }
        // センターガイドライン表示フラグ
        public bool EnableCenterGuide
        {
            get { return EditorPrefs.GetBool("EnableCenterGuide", true); }
            set { EditorPrefs.SetBool("EnableCenterGuide", value); }
        }
        // 1/3ライン表示フラグ
        public bool EnableOneThirdLine
        {
            get { return EditorPrefs.GetBool("EnableOneThirdLine", true); }
            set { EditorPrefs.SetBool("EnableOneThirdLine", value); }
        }
        // フレーム表示フラグ
        public bool EnableOutlineFrame
        {
            get { return EditorPrefs.GetBool("EnableOutlineFrame", true); }
            set { EditorPrefs.SetBool("EnableOutlineFrame", value); }
        }
        // 画面のズーム量
        public float EditorZoomAmount
        {
            get { return EditorPrefs.GetFloat("EditorZoomAmount", 1f); }
            set
            {
                EditorPrefs.SetFloat("EditorZoomAmount", value);
                Repaint();
            }
        }

        private Settings setting
        {
            get { return Settings.Instance; }
        }

        private Vector2 scrollPos;
        private Vector2 centerPos;
        private bool pressCtrl;
        private float? wheelAmount = null;

        #endregion

        // ウインドウを開くメニューコマンド.
        [MenuItem("Window/Sagittarius/EditWindow Open")]
        public static void Open()
        {
            instance = Instance;
        }

        // GUI更新.
        private void GUIUpdate()
        {
            Event e = Event.current;
            do
            {
                if (e != null)
                {
                    if (e.type == EventType.scrollWheel)
                    {
                        wheelAmount = e.delta.y;
                    }

                    if (!pressCtrl && e.type == EventType.keyDown)
                    {
                        if (e.keyCode == KeyCode.LeftControl || e.keyCode == KeyCode.RightControl)
                        {
                            pressCtrl = true;
                        }
                    }
                }
            } while (Event.PopEvent(e));
        }

        // 更新.
        private void Update()
        {
            if (pressCtrl && wheelAmount.HasValue)
            {
                EditorZoomAmount += wheelAmount.Value * 0.01f;
            }

            // reset
            pressCtrl = false;
            wheelAmount = null;
        }

        // GUI描画イベント.
        public void OnGUI()
        {
            GUI.enabled = !IsPreviewMode;
            
            DrawOutlineFrame();
            DrawTargetRect();
            DrawUnitImage();
            DrawOneThirdLine();
            DrawCenterGuide();

            DrawMenu();

            GUI.enabled = true;

            GUIUpdate();
        }

        #region Menu
        private void DrawMenu()
        {
            EditorGUILayout.BeginHorizontal(GUI.skin.box);
            if (GUILayout.Button("左右反転", GUILayout.MaxWidth(150))) OnClickFlip();
            if (GUILayout.Button("センターガイド ON/OFF", GUILayout.MaxWidth(150))) OnClickCenterGuide();
            if (GUILayout.Button("1/3ライン ON/OFF", GUILayout.MaxWidth(150))) OnClickOneThirdLine();
            if (GUILayout.Button("フレーム ON/OFF", GUILayout.MaxWidth(150))) OnClickFrame();
            var col = GUI.backgroundColor;
            GUI.backgroundColor = Color.magenta;
            if (GUILayout.Button("確定", GUILayout.MaxWidth(150))) OnClickConfirm();
            GUI.backgroundColor = col;
            EditorGUILayout.EndHorizontal();
        }

        // 左右反転ボタンを押した時の挙動.
        private void OnClickFlip()
        {
            CurrentRect.scale.x *= -1;
        }

        // センターガイド切替ボタンを押した時の挙動.
        private void OnClickCenterGuide()
        {
            EnableCenterGuide = !EnableCenterGuide;
        }

        // 1/3ライン切替ボタンを押した時の挙動.
        private void OnClickOneThirdLine()
        {
            EnableOneThirdLine = !EnableOneThirdLine;
        }

        // 外枠の表示切替ボタンを押した時の挙動.
        private void OnClickFrame()
        {
            EnableOutlineFrame = !EnableOutlineFrame;
        }

        // 確定ボタンを押した時の挙動.
        private void OnClickConfirm()
        {
            if (EditorUtility.DisplayDialog("確認", "編集データを保存します\nよろしいですか？", "OK", "Cancel"))
            {
                AssetDatabase.SaveAssets();
            }
        }
        #endregion

        // 外枠の表示
        private void DrawOutlineFrame()
        {
            if (!EnableOutlineFrame) return;
            var x = 0;
            var y = 0;
            var w = position.width;
            var h = position.height;
            GUI.DrawTexture(new Rect(x, y, w, h), FrameTex);
        }

        // 選択中シーンの領域サイズを描画
        private void DrawTargetRect()
        {
            var drawScene = setting.SceneList[SelectedSceneIndex];
            var x = position.width / 2 - drawScene.width / 2 * EditorZoomAmount;
            var y = position.height / 2 - drawScene.height / 2 * EditorZoomAmount;
            var w = drawScene.width * EditorZoomAmount;
            var h = drawScene.height * EditorZoomAmount;
            GUI.DrawTexture(new Rect(x, y, w, h), TargetAreaTex);
        }

        // ユニットイラストを表示
        private void DrawUnitImage()
        {
            var drawScene = setting.SceneList[SelectedSceneIndex];
            var selectedElementIds = Current.sceneList[SelectedSceneIndex].GetSelectedElementIdList(SelectedRectIndex);
            var texs = selectedElementIds.ConvertAll(_ => Current.texList[_]);

            foreach (var info in texs)
            {
                var x = (position.width / 2 - drawScene.width / 2 + CurrentRect.rect.x) * EditorZoomAmount;
                var y = (position.height / 2 - drawScene.height / 2 + CurrentRect.rect.y) * EditorZoomAmount;
                var w = info.Texture.width * CurrentRect.scale.x * EditorZoomAmount;
                var h = info.Texture.height * CurrentRect.scale.y * EditorZoomAmount;
                //Debug.Log("DrawTexture : " + new Rect(x, y, w, h) + " scale : " + CurrentRect.scale + " zoom : " + EditorZoomAmount);
                //GUI.DrawTexture(new Rect(x, y, w, h), info.Texture);
                GUI.Box(new Rect(x, y, w, h), info.Texture);
            }
        }

        private void DrawOneThirdLine()
        {
            if (!EnableOneThirdLine) return;
            // TODO
        }

        private void DrawCenterGuide()
        {
            if (!EnableCenterGuide) return;
            // TODO
        }

        
    }
}