﻿using System;
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
    public class EditWindow : DraggableWindow
    {
        private const string FrameTexPath = "Assets/Sagittarius/Textures/black_8x8.png";
        private const string TargetAreaTexPath = "Assets/Sagittarius/Textures/targetArea.png";
        private const int spSize = 20;// scale point size.

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

        #region Public Declaration
        // 編集対象のデータ.
        // このデータはMainWindowからセットされます
        public UnitData Current { get; set; }
        // 対象シーン.
        // このデータはMainWindowからセットされます
        public int SelectedSceneIndex { get; set; }
        // 対象データIndex.
        // このデータはMainWindowからセットされます
        public int SelectedRectIndex { get; set; }
        // プレビューモードかどうか.
        // このデータはMainWindowからセットされます
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
                catch
                {
                    Debug.LogWarning(string.Format("current:{2} sceneIndex:{0} rectIndex:{1}", SelectedSceneIndex, SelectedRectIndex, Current==null ? "null" : "exist"));
                }
                return null;
            }
        }
        // 選択中のテクスチャ.
        public List<Texture2D> SelectedTextures
        {
            get
            {
                try
                {
                    var selectedElementIds = Current.sceneList[SelectedSceneIndex].GetSelectedElementIdList(SelectedRectIndex);
                    return selectedElementIds.ConvertAll(_ => Current.texList[_].Texture);
                }
                catch
                {
                    return null;
                }
            }
        }

        #region EditorPrefs
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
                var clamped = Mathf.Clamp(value, 0.3f, 5f);
                EditorPrefs.SetFloat("EditorZoomAmount", clamped);
                Repaint();
            }
        }
        // 画面X位置
        public float EditorPosX
        {
            get { return EditorPrefs.GetFloat("EditorPosX", 0f); }
            set { EditorPrefs.SetFloat("EditorPosX", value); }
        }
        // 画面Y位置
        public float EditorPosY
        {
            get { return EditorPrefs.GetFloat("EditorPosY", 0f); }
            set { EditorPrefs.SetFloat("EditorPosY", value); }
        }
        #endregion
        #endregion

        #region Private Declaration

        private Settings setting
        {
            get { return Settings.Instance; }
        }

        private bool pressCtrl;
        private bool[] pressState = new bool[Enum.GetValues(typeof (KeyCode)).Length];
        private bool isDraggingUnitTex;
        private float? wheelAmount = null;
        private Vector2 mousePos;
        private Rectangle baseRect;
        private Rectangle leftTopRect;
        private Rectangle rightTopRect;
        private Rectangle rightBottomRect;
        private Rectangle leftBottomRect;
        private Vector2 editorDragStartPos;
        private Vector2 editorDragStartMouse;

        #endregion

        // ウインドウを開くメニューコマンド.
        [MenuItem("Window/Sagittarius/EditWindow Open")]
        public static void Open()
        {
            instance = Instance;
        }

        protected virtual void OnDestroy()
        {
            if (instance) EditorApplication.update -= instance.Update;
            instance = null;
        }

        // 初期化.
        public virtual void Initialize()
        {
            if (CurrentRect.rect == null)
            {
                CurrentRect.rect = new Rectangle(0, 0, 0, 0);
            }
            if (SelectedTextures != null && SelectedTextures.Count > 0)
            {
                if (CurrentRect.rect.width.Equals(0))
                    CurrentRect.rect.width = SelectedTextures[0].width;
                if (CurrentRect.rect.height.Equals(0))
                    CurrentRect.rect.height = SelectedTextures[0].height;
            }

            // ベース領域
//            baseRect = new Rectangle(-10000, -10000, 100000, 100000);
//            RegisterDrag(1, baseRect, null, null,"Base",
//                OnBaseDragStart, OnBaseDrag, OnBaseDragEnd);

            // ユニットのテクスチャ
//            RegisterDrag(0, CurrentRect.rect,
//                () => CurrentRect.scale.x,
//                () => EditorZoomAmount, "UnitTex",
//                OnUnitDragStart, OnUnitDrag, OnUnitDragEnd);
        }

        #region Base Drag
        void OnBaseDragStart(Vector2 mousePos, DragObject o)
        {
            editorDragStartMouse = mousePos;
            editorDragStartPos = new Vector2(EditorPosX, EditorPosY);
        }

        void OnBaseDrag(Vector2 mousePos, DragObject o)
        {
            var move = mousePos - editorDragStartMouse;
            EditorPosX = editorDragStartPos.x + move.x / EditorZoomAmount;
            EditorPosY = editorDragStartPos.y + move.y / EditorZoomAmount;
            Repaint();
        }

        void OnBaseDragEnd(Vector2 mousePos, DragObject o)
        {
            var move = mousePos - editorDragStartMouse;
            EditorPosX = editorDragStartPos.x + move.x / EditorZoomAmount;
            EditorPosY = editorDragStartPos.y + move.y / EditorZoomAmount;
            Repaint();
        }
        #endregion

        #region Unit Drag

        void OnUnitDragStart(Vector2 mousePos, DragObject o)
        {
            isDraggingUnitTex = true;
        }

        void OnUnitDrag(Vector2 mousePos, DragObject o)
        {
            CurrentRect.rect.position = mousePos + DragObject.Current.dragOffset;
            Repaint();
        }

        void OnUnitDragEnd(Vector2 mousePos, DragObject o)
        {
            isDraggingUnitTex = false;
            CurrentRect.rect.position = mousePos + DragObject.Current.dragOffset;
            Repaint();
        }
        #endregion

        #region UnitScale Drag
        // TODO
        void OnLeftTopDragStart(Vector2 mousePos, DragObject o) { }
        void OnLeftTopDrag(Vector2 mousePos, DragObject o) { }
        void OnLeftTopDragEnd(Vector2 mousePos, DragObject o) { }


        void OnRightTopDragStart(Vector2 mousePos, DragObject o) { }
        void OnRightTopDrag(Vector2 mousePos, DragObject o) { }
        void OnRightTopDragEnd(Vector2 mousePos, DragObject o) { }


        void OnRightBottomDragStart(Vector2 mousePos, DragObject o) { }
        void OnRightBottomDrag(Vector2 mousePos, DragObject o) { }
        void OnRightBottomDragEnd(Vector2 mousePos, DragObject o) { }


        void OnLeftBottomDragStart(Vector2 mousePos, DragObject o) { }
        void OnLeftBottomDrag(Vector2 mousePos, DragObject o) { }
        void OnLeftBottomDragEnd(Vector2 mousePos, DragObject o) { }
        #endregion


        // GUI描画イベント.
        protected override void OnGUI()
        {
            GUI.enabled = !IsPreviewMode;
            
            DrawOutlineFrame();
            DrawTargetRect();
            DrawUnitImage();
            DrawOneThirdLine();
            DrawCenterGuide();

            DrawMenu();

            GUI.enabled = true;

            base.OnGUI();
        }

        // GUI入力イベント.
        protected override void OnGuiEvent(Event e)
        {
            if (e != null)
            {
                mousePos = e.mousePosition;
                if (e.type == EventType.scrollWheel)
                {
                    wheelAmount = e.delta.y;
                }

                if (e.type == EventType.keyDown)
                {
                    pressState[(int) e.keyCode] = true;
                    if (e.keyCode == KeyCode.LeftControl || e.keyCode == KeyCode.RightControl)
                    {
                        pressCtrl = true;
                    }
                }
            }

            base.OnGuiEvent(e);
        }

        // 更新.
        private void Update()
        {
            if (pressCtrl)
            {
                if (wheelAmount.HasValue)
                {
                    // ユニットのテクスチャスケールの更新
                    var prevScale = CurrentRect.scale.x;
                    var scale = Mathf.Clamp(CurrentRect.scale.x - wheelAmount.Value*0.01f, 0.05f, 5f);
                    CurrentRect.scale.x = scale;
                    CurrentRect.scale.y = scale;

                    // ずれた位置を修正する
                    var prevW = CurrentRect.rect.width*prevScale;
                    var prevH = CurrentRect.rect.height*prevScale;
                    var afterW = CurrentRect.rect.width*scale;
                    var afterH = CurrentRect.rect.height*scale;
                    var diffW = afterW - prevW;
                    var diffH = afterH - prevH;

                    CurrentRect.rect.x -= diffW/2;
                    CurrentRect.rect.y -= diffH/2;
                }

                if (pressState[(int) KeyCode.Plus] || pressState[(int) KeyCode.KeypadPlus])
                {
                    EditorZoomAmount *= 1.25f;
                }
                if (pressState[(int) KeyCode.Minus] || pressState[(int) KeyCode.KeypadMinus])
                {
                    EditorZoomAmount *= 0.75f;
                }
            }

            if (pressState[(int)KeyCode.None])
            {
                pressCtrl = false;
            }

            // reset
            wheelAmount = null;
            for (int i = 0; i < pressState.Length; ++i)
            {
                //if (pressState[i]) Debug.Log("  " + (KeyCode) i);
                pressState[i] = false;
            }
        }

        #region Menu
        private void DrawMenu()
        {
            EditorGUILayout.BeginHorizontal(GUI.skin.box);

            if (GUILayout.Button("左右反転", GUILayout.MaxWidth(150))) OnClickFlip();
            if (GUILayout.Button("センターガイド ON/OFF", GUILayout.MaxWidth(150))) OnClickCenterGuide();
            if (GUILayout.Button("1/3ライン ON/OFF", GUILayout.MaxWidth(150))) OnClickOneThirdLine();
            if (GUILayout.Button("フレーム ON/OFF", GUILayout.MaxWidth(150))) OnClickFrame();
            

            if (GUILayout.Button("位置リセット", GUILayout.MaxWidth(150))) { EditorPosX = EditorPosY = 0f; }
            if (GUILayout.Button("Zoomリセット", GUILayout.MaxWidth(150))) { EditorZoomAmount = 1f; }
            if (GUILayout.Button("Rectリセット", GUILayout.MaxWidth(150)))
            {
                CurrentRect.scale = Vector2.one;
                CurrentRect.rect = new Rectangle(0, 0, SelectedTextures[0].width, SelectedTextures[0].height);
            }

            var col = GUI.backgroundColor;
            GUI.backgroundColor = Color.magenta;
            if (GUILayout.Button("確定", GUILayout.MaxWidth(150))) OnClickConfirm();
            GUI.backgroundColor = col;
            EditorZoomAmount = GUI.HorizontalSlider(new Rect(position.width - 200, position.height - 20, 200, 20), EditorZoomAmount, 0.3f, 5f);
            EditorGUILayout.EndHorizontal();
        }

        // 左右反転ボタンを押した時の挙動.
        private void OnClickFlip()
        {
            CurrentRect.scale.x *= -1;

            var selectedElementIds = Current.sceneList[SelectedSceneIndex].GetSelectedElementIdList(SelectedRectIndex);
            var texs = selectedElementIds.ConvertAll(_ => Current.texList[_]);

            if (CurrentRect.scale.x > 0)
            {
                CurrentRect.rect.x -= texs.Find(_ => _.Texture != null).Texture.width;
            }
            else if (CurrentRect.scale.x < 0)
            {
                CurrentRect.rect.x += texs.Find(_ => _.Texture != null).Texture.width;
            }
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
                var index = UnitDisplayData.Instance.UnitList.IndexOf(Current);
                UnitDisplayData.Instance.UnitList[index].sceneList[SelectedSceneIndex].rectData[SelectedRectIndex] = CurrentRect;

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
            if (drawScene != null)
            {
                var x = position.width/2 - drawScene.width/2*EditorZoomAmount;
                var y = position.height/2 - drawScene.height/2*EditorZoomAmount;
                var w = drawScene.width*EditorZoomAmount;
                var h = drawScene.height*EditorZoomAmount;
                GUI.DrawTexture(new Rect(x, y, w, h), TargetAreaTex);
            }
        }

        // ユニットイラストを表示
        private void DrawUnitImage()
        {
            var drawScene = setting.SceneList[SelectedSceneIndex];
            var selectedElementIds = Current.sceneList[SelectedSceneIndex].GetSelectedElementIdList(SelectedRectIndex);
            var texs = selectedElementIds.ConvertAll(_ => Current.texList[_]);

            foreach (var info in texs)
            {
                var x = (position.width / 2 - CurrentRect.rect.width / 2 * EditorZoomAmount) + (CurrentRect.rect.x + EditorPosX) * EditorZoomAmount;
                var y = (position.height / 2 - CurrentRect.rect.width / 2 * EditorZoomAmount) + (CurrentRect.rect.y + EditorPosY) * EditorZoomAmount;
                var w = info.Texture.width * CurrentRect.scale.x * EditorZoomAmount;
                var h = info.Texture.height * CurrentRect.scale.y * EditorZoomAmount;
                //Debug.Log("DrawTexture : " + new Rect(x, y, w, h) + " scale : " + CurrentRect.scale + " zoom : " + EditorZoomAmount);
                

                if (isDraggingUnitTex) GUI.Box(new Rect(x, y, w, h), info.Texture);
                else GUI.DrawTexture(new Rect(x, y, w, h), info.Texture);
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