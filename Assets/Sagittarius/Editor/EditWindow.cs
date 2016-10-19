using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace Griphone.Sagittarius
{
    /// <summary>
    /// 画像の領域を編集するウインドウ.
    /// </summary>
    public class EditWindow : DraggableWindow
    {
        #region Static
        /// <summary>
        /// フレーム画像用パス
        /// </summary>
        private const string FrameTexPath = "Assets/Sagittarius/Textures/black_8x8.png";

        /// <summary>
        /// ターゲット領域画像用パス
        /// </summary>
        private const string TargetAreaTexPath = "Assets/Sagittarius/Textures/targetArea.png";

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
        /// <summary>
        /// 編集対象のデータ
        /// このデータはMainWindowからセットされます
        /// </summary>
        public UnitData Current { get; set; }

        /// <summary>
        /// 対象シーン.
        /// このデータはMainWindowからセットされます
        /// </summary>
        public int SelectedSceneIndex { get; set; }

        /// <summary>
        /// 対象データIndex.
        /// このデータはMainWindowからセットされます
        /// </summary>
        public int SelectedRectIndex { get; set; }

        /// <summary>
        /// プレビューモードかどうか.
        /// このデータはMainWindowからセットされます
        /// </summary>
        public bool IsPreviewMode { get; set; }

        /// <summary>
        /// 編集中の領域データ
        /// </summary>
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
                    Debug.LogWarning(string.Format("current:{2} sceneIndex:{0} rectIndex:{1}", SelectedSceneIndex, SelectedRectIndex, Current == null ? "null" : "exist"));
                }
                return null;
            }
        }

        /// <summary>
        /// 選択中のテクスチャ
        /// </summary>
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
        /// <summary>
        /// センターガイドライン表示フラグ
        /// </summary>
        public bool EnableCenterGuide
        {
            get { return EditorPrefs.GetBool("EnableCenterGuide", true); }
            set { EditorPrefs.SetBool("EnableCenterGuide", value); }
        }

        /// <summary>
        /// 1/3ライン表示フラグ
        /// </summary>
        public bool EnableOneThirdLine
        {
            get { return EditorPrefs.GetBool("EnableOneThirdLine", true); }
            set { EditorPrefs.SetBool("EnableOneThirdLine", value); }
        }

        /// <summary>
        /// フレーム表示フラグ
        /// </summary>
        public bool EnableOutlineFrame
        {
            get { return EditorPrefs.GetBool("EnableOutlineFrame", true); }
            set { EditorPrefs.SetBool("EnableOutlineFrame", value); }
        }

        /// <summary>
        /// 画面のズーム量
        /// </summary>
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

        /// <summary>
        /// エディタX位置
        /// </summary>
        public float EditorPosX
        {
            get { return EditorPrefs.GetFloat("EditorPosX", 0f); }
            set { EditorPrefs.SetFloat("EditorPosX", value); }
        }

        /// <summary>
        /// エディタY位置
        /// </summary>
        public float EditorPosY
        {
            get { return EditorPrefs.GetFloat("EditorPosY", 0f); }
            set { EditorPrefs.SetFloat("EditorPosY", value); }
        }
        #endregion

        #endregion

        #region Private Declaration

        private Settings Setting
        {
            get { return Settings.Instance; }
        }

        private bool _pressCtrl;
        private bool[] _pressState = new bool[Enum.GetValues(typeof (KeyCode)).Length];
        private float? _wheelAmount = null;
        private Rectangle _baseRect;
        private Vector2 _editorDragStartPos;
        private Vector2 _editorDragStartMouse;
        private Rect _prevPosition;

        #endregion

        #region Public Method
        /// <summary>
        /// ウインドウを開きます
        /// </summary>
        public static void Open()
        {
            instance = Instance;
        }

        /// <summary>
        /// 初期化
        /// </summary>
        public virtual void Initialize()
        {
            if (CurrentRect.rect == null)
            {
                CurrentRect.rect = new Rectangle(0, 0, 0, 0);
            }
            if (SelectedTextures != null && SelectedTextures.Count > 0)
            {
                CurrentRect.rect.width = SelectedTextures[0] == null ? Setting.SceneList[SelectedSceneIndex].width : SelectedTextures[0].width;
                CurrentRect.rect.height = SelectedTextures[0] == null ? Setting.SceneList[SelectedSceneIndex].height : SelectedTextures[0].height;
            }

            // Drag情報クリア
            ClearDrag();

            // ベース領域
            _baseRect = new Rectangle(-10000, -10000, 100000, 100000);
            RegisterDrag(10, _baseRect, Pivot.TopLeft, null, null, null, "Base",
                OnBaseDragStart, OnBaseDrag, OnBaseDragEnd);

            // ユニットのテクスチャ
            RegisterDrag(9, CurrentRect.rect, Pivot.Center,
                () => Mathf.Abs(CurrentRect.scale.x),
                () => EditorZoomAmount,
                () => new Vector2(EditorPosX, EditorPosY),
                "UnitTex",
                OnUnitDragStart, OnUnitDrag, OnUnitDragEnd);
        }
        #endregion

        #region Private Method

        #region UnityEvent
        /// <summary>
        /// 破棄イベント
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (instance) EditorApplication.update -= instance.Update;
            instance = null;
        }

        /// <summary>
        /// 更新
        /// </summary>
        protected virtual void Update()
        {
            if (_pressCtrl)
            {
                if (_wheelAmount.HasValue)
                {
                    // ユニットのテクスチャスケールの更新
                    var scale = Mathf.Clamp(CurrentRect.scale.x - _wheelAmount.Value * 0.01f, 0.05f, 5f);
                    SetCurrentScale(scale);
                }

                if (_pressState[(int) KeyCode.Plus] || _pressState[(int) KeyCode.KeypadPlus])
                {
                    EditorZoomAmount *= 1.25f;
                }
                if (_pressState[(int) KeyCode.Minus] || _pressState[(int) KeyCode.KeypadMinus])
                {
                    EditorZoomAmount *= 0.75f;
                }
            }

            if (_pressState[(int) KeyCode.None])
            {
                _pressCtrl = false;
            }

            // reset
            _wheelAmount = null;
            for (int i = 0; i < _pressState.Length; ++i)
            {
                //if (pressState[i]) Debug.Log("  " + (KeyCode) i);
                _pressState[i] = false;
            }
        }

        /// <summary>
        /// GUI描画イベント
        /// </summary>
        protected override void OnGUI()
        {
            GUI.enabled = !IsPreviewMode;

            DrawBgTexture();
            DrawTargetRect();
            DrawUnitImage();
            DrawOverlayFrame();
            DrawOutlineFrame();
            DrawOneThirdLine();
            DrawCenterGuide();
            DrawMenu();
            DrawTextureMenu();
            DrawDebug();

            GUI.enabled = true;

            base.OnGUI();
        }

        /// <summary>
        /// GUI入力イベント
        /// </summary>
        protected override void OnGuiEvent(Event e)
        {
            if (e != null)
            {
                if (e.type == EventType.scrollWheel)
                {
                    _wheelAmount = e.delta.y;
                }

                if (e.type == EventType.keyDown)
                {
                    _pressState[(int) e.keyCode] = true;
                    if (e.keyCode == KeyCode.LeftControl || e.keyCode == KeyCode.RightControl)
                    {
                        _pressCtrl = true;
                    }
                }
            }

            base.OnGuiEvent(e);
        }
        #endregion

        #region Base Drag
        void OnBaseDragStart(Vector2 mousePos, DragObject o)
        {
            _editorDragStartMouse = mousePos;
            _editorDragStartPos = new Vector2(EditorPosX, EditorPosY);
        }

        void OnBaseDrag(Vector2 mousePos, DragObject o)
        {
            var move = mousePos - _editorDragStartMouse;
            EditorPosX = _editorDragStartPos.x + move.x / EditorZoomAmount;
            EditorPosY = _editorDragStartPos.y + move.y / EditorZoomAmount;
            Repaint();
        }

        void OnBaseDragEnd(Vector2 mousePos, DragObject o)
        {
            var move = mousePos - _editorDragStartMouse;
            EditorPosX = _editorDragStartPos.x + move.x / EditorZoomAmount;
            EditorPosY = _editorDragStartPos.y + move.y / EditorZoomAmount;
            Repaint();
        }
        #endregion

        #region Unit Drag

        void OnUnitDragStart(Vector2 mousePos, DragObject o)
        {
        }

        void OnUnitDrag(Vector2 mousePos, DragObject o)
        {
            CurrentRect.rect.position = mousePos + DragObject.Current.dragOffset;
            Repaint();
        }

        void OnUnitDragEnd(Vector2 mousePos, DragObject o)
        {
            CurrentRect.rect.position = mousePos + DragObject.Current.dragOffset;
            Repaint();
        }
        #endregion

        #region UnitScale Drag
        // TODO
        void OnLeftTopDragStart(Vector2 mousePos, DragObject o)
        {
        }

        void OnLeftTopDrag(Vector2 mousePos, DragObject o)
        {
        }

        void OnLeftTopDragEnd(Vector2 mousePos, DragObject o)
        {
        }

        // TODO
        void OnRightTopDragStart(Vector2 mousePos, DragObject o)
        {
        }

        void OnRightTopDrag(Vector2 mousePos, DragObject o)
        {
        }

        void OnRightTopDragEnd(Vector2 mousePos, DragObject o)
        {
        }

        // TODO
        void OnRightBottomDragStart(Vector2 mousePos, DragObject o)
        {
        }

        void OnRightBottomDrag(Vector2 mousePos, DragObject o)
        {
        }

        void OnRightBottomDragEnd(Vector2 mousePos, DragObject o)
        {
        }

        // TODO
        void OnLeftBottomDragStart(Vector2 mousePos, DragObject o)
        {
        }

        void OnLeftBottomDrag(Vector2 mousePos, DragObject o)
        {
        }

        void OnLeftBottomDragEnd(Vector2 mousePos, DragObject o)
        {
        }
        #endregion

        #region Menu
        private void DrawMenu()
        {
            EditorGUILayout.BeginHorizontal(GUI.skin.box);

            if (GUILayout.Button("左右反転", GUILayout.MaxWidth(150))) OnClickFlip();
            if (GUILayout.Button("センターガイド ON/OFF", GUILayout.MaxWidth(150))) OnClickCenterGuide();
            if (GUILayout.Button("1/3ライン ON/OFF", GUILayout.MaxWidth(150))) OnClickOneThirdLine();
            if (GUILayout.Button("フレーム ON/OFF", GUILayout.MaxWidth(150))) OnClickFrame();


            if (GUILayout.Button("位置リセット", GUILayout.MaxWidth(150)))
            {
                EditorPosX = EditorPosY = 0f;
            }
            if (GUILayout.Button("Zoomリセット", GUILayout.MaxWidth(150)))
            {
                EditorZoomAmount = 1f;
            }
            if (GUILayout.Button("Rectリセット", GUILayout.MaxWidth(150)))
            {
                CurrentRect.scale = Vector2.one;
                CurrentRect.rect.x = 0f;
                CurrentRect.rect.y = 0f;
            }

            var col = GUI.backgroundColor;
            GUI.backgroundColor = Color.magenta;
            if (GUILayout.Button("確定", GUILayout.MaxWidth(150))) OnClickConfirm();
            GUI.backgroundColor = col;
            GUI.Label(new Rect(position.width - 300, position.height - 45, 100, 20), "UnitScale");
            var scale = GUI.HorizontalSlider(new Rect(position.width - 200, position.height - 45, 200, 20), CurrentRect.scale.x, 0.01f, 2f);
            if (CurrentRect.scale.x != scale)
            {
                SetCurrentScale(scale);
            }
            GUI.Label(new Rect(position.width - 300, position.height - 20, 100, 20), "Zoom");
            EditorZoomAmount = GUI.HorizontalSlider(new Rect(position.width - 200, position.height - 20, 200, 20), EditorZoomAmount, 0.3f, 5f);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawTextureMenu()
        {
            EditorGUILayout.BeginHorizontal(GUI.skin.box);
            {
                var texNames = Setting.BgTextureList.ConvertAll(_ => _ == null ? " " : _.name);
                CurrentRect.selectedBgTexIndex = EditorGUILayout.Popup(CurrentRect.selectedBgTexIndex, texNames.ToArray(), GUILayout.MaxWidth(200));
            }
            {
                var texNames = Setting.FrameTexList.ConvertAll(_ => _ == null ? " " : _.name);
                CurrentRect.selectedFrameTexIndex = EditorGUILayout.Popup(CurrentRect.selectedFrameTexIndex, texNames.ToArray(), GUILayout.MaxWidth(200));
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 左右反転ボタンを押した時の挙動.
        /// </summary>
        private void OnClickFlip()
        {
            CurrentRect.IsFlip = !CurrentRect.IsFlip;
        }

        /// <summary>
        /// センターガイド切替ボタンを押した時の挙動.
        /// </summary>
        private void OnClickCenterGuide()
        {
            EnableCenterGuide = !EnableCenterGuide;
        }

        /// <summary>
        /// 1/3ライン切替ボタンを押した時の挙動.
        /// </summary>
        private void OnClickOneThirdLine()
        {
            EnableOneThirdLine = !EnableOneThirdLine;
        }

        /// <summary>
        /// 外枠の表示切替ボタンを押した時の挙動.
        /// </summary>
        private void OnClickFrame()
        {
            EnableOutlineFrame = !EnableOutlineFrame;
        }

        /// <summary>
        /// 確定ボタンを押した時の挙動.
        /// </summary>
        private void OnClickConfirm()
        {
            if (EditorUtility.DisplayDialog("確認", "編集データを保存します\nよろしいですか？", "OK", "Cancel"))
            {
                var drawScene = Setting.SceneList[SelectedSceneIndex];
                CurrentRect.ApplyUVRect(drawScene);
                AssetDatabase.SaveAssets();
                ShowNotification(new GUIContent("保存が完了しました"));
            }
        }
        #endregion

        #region Draw Method

        /// <summary>
        /// 外枠の表示
        /// </summary>
        private void DrawOutlineFrame()
        {
            var drawScene = Setting.SceneList[SelectedSceneIndex];
            if (!EnableOutlineFrame || drawScene == null) return;

            var x = EditorPosX * EditorZoomAmount;
            var y = EditorPosY * EditorZoomAmount;
            var w = position.width;
            var h = position.height;
            //GUI.DrawTexture(new Rect(x, y, w, h), FrameTex);

            GUI.DrawTexture(new Rect(x, y, w / 2f - drawScene.width / 2f * EditorZoomAmount, h), FrameTex);
            GUI.DrawTexture(new Rect(x + w / 2f + drawScene.width / 2f * EditorZoomAmount, y, w / 2f - drawScene.width / 2f * EditorZoomAmount, h), FrameTex);
            GUI.DrawTexture(new Rect(x + w / 2f - drawScene.width / 2f * EditorZoomAmount, y, drawScene.width * EditorZoomAmount, (h - drawScene.height * EditorZoomAmount) / 2f), FrameTex);
            GUI.DrawTexture(new Rect(x + w / 2f - drawScene.width / 2f * EditorZoomAmount, y + (h - drawScene.height * EditorZoomAmount) / 2f + drawScene.height * EditorZoomAmount, drawScene.width * EditorZoomAmount, (h - drawScene.height * EditorZoomAmount) / 2f), FrameTex);
        }

        /// <summary>
        /// 背景テクスチャの表示
        /// </summary>
        private void DrawBgTexture()
        {
            if (Setting.BgTextureList.Count > CurrentRect.selectedBgTexIndex)
            {
                var tex = Setting.BgTextureList[CurrentRect.selectedBgTexIndex];
                if (tex != null)
                {
                    var x = position.width / 2f - (tex.width / 2f - EditorPosX) * EditorZoomAmount;
                    var y = position.height / 2f - (tex.height / 2f - EditorPosY) * EditorZoomAmount;
                    var w = tex.width * EditorZoomAmount;
                    var h = tex.height * EditorZoomAmount;
                    GUI.DrawTexture(new Rect(x, y, w, h), tex);
                }
            }
        }

        /// <summary>
        /// 選択中シーンの領域サイズを描画
        /// </summary>
        private void DrawTargetRect()
        {
            var drawScene = Setting.SceneList[SelectedSceneIndex];
            if (drawScene != null)
            {
                var x = position.width / 2f - (drawScene.width / 2f - EditorPosX) * EditorZoomAmount;
                var y = position.height / 2f - (drawScene.height / 2f - EditorPosY) * EditorZoomAmount;
                var w = drawScene.width * EditorZoomAmount;
                var h = drawScene.height * EditorZoomAmount;
                GUI.DrawTexture(new Rect(x, y, w, h), TargetAreaTex);
            }
        }

        /// <summary>
        /// ユニットイラストを表示
        /// </summary>
        private void DrawUnitImage()
        {
            var selectedElementIds = Current.sceneList[SelectedSceneIndex].GetSelectedElementIdList(SelectedRectIndex);
            var texs = selectedElementIds.ConvertAll(_ => Current.texList[_]);

            float scaleX = CurrentRect.scale.x;
            float rectX = CurrentRect.rect.x;

            if (CurrentRect.IsFlip)
            {
                scaleX *= -1;
                if (scaleX > 0)
                {
                    rectX -= CurrentRect.rect.width * Mathf.Abs(scaleX);
                }
                else if (scaleX < 0)
                {
                    rectX += CurrentRect.rect.width * Mathf.Abs(scaleX);
                }
            }

            foreach (var info in texs)
            {
                if (info.Texture == null) continue;
                var x = (position.width / 2f - CurrentRect.rect.width / 2f * EditorZoomAmount) + (rectX + EditorPosX) * EditorZoomAmount;
                var y = (position.height / 2f - CurrentRect.rect.height / 2f * EditorZoomAmount) + (CurrentRect.rect.y + EditorPosY) * EditorZoomAmount;
                var w = info.Texture.width * scaleX * EditorZoomAmount;
                var h = info.Texture.height * CurrentRect.scale.y * EditorZoomAmount;
                //Debug.Log("DrawTexture : " + new Rect(x, y, w, h) + " scale : " + CurrentRect.scale + " zoom : " + EditorZoomAmount);

                GUI.DrawTexture(new Rect(x, y, w, h), info.Texture);
            }
        }

        /// <summary>
        /// オーバーレイフレームの表示
        /// </summary>
        private void DrawOverlayFrame()
        {
            if (Setting.FrameTexList.Count > CurrentRect.selectedFrameTexIndex)
            {
                var tex = Setting.FrameTexList[CurrentRect.selectedFrameTexIndex];
                if (tex != null)
                {
                    var x = position.width / 2f - (tex.width / 2f - EditorPosX) * EditorZoomAmount;
                    var y = position.height / 2f - (tex.height / 2f - EditorPosY) * EditorZoomAmount;
                    var w = tex.width * EditorZoomAmount;
                    var h = tex.height * EditorZoomAmount;
                    GUI.DrawTexture(new Rect(x, y, w, h), tex);
                }
            }
        }

        /// <summary>
        /// 1/3ラインの表示
        /// </summary>
        private void DrawOneThirdLine()
        {
            var drawScene = Setting.SceneList[SelectedSceneIndex];
            if (!EnableOneThirdLine || drawScene == null) return;

            var lineColor = new Color(0f, 0f, 1f, 0.5f);
            var centerX = position.width / 2f - (drawScene.width / 2f - EditorPosX) * EditorZoomAmount;
            var centerY = position.height / 2f - (drawScene.height / 2f - EditorPosY) * EditorZoomAmount;

            // 縦1/3
            var p1 = new Vector2(centerX + drawScene.width * 0.3333f * EditorZoomAmount, 0);
            var p2 = new Vector2(centerX + drawScene.width * 0.3333f * EditorZoomAmount, position.height);
            Drawing.DrawLine(p1, p2, lineColor, 1, false);
            // 縦2/3
            p1 = new Vector2(centerX + drawScene.width * 0.6667f * EditorZoomAmount, 0);
            p2 = new Vector2(centerX + drawScene.width * 0.6667f * EditorZoomAmount, position.height);
            Drawing.DrawLine(p1, p2, lineColor, 1, false);
            // 横1/3
            p1 = new Vector2(-position.width, centerY + drawScene.height * 0.3333f * EditorZoomAmount);
            p2 = new Vector2(position.width, centerY + drawScene.height * 0.3333f * EditorZoomAmount);
            Drawing.DrawLine(p1, p2, lineColor, 1, false);
            // 横2/3
            p1 = new Vector2(-position.width, centerY + drawScene.height * 0.6667f * EditorZoomAmount);
            p2 = new Vector2(position.width, centerY + drawScene.height * 0.6667f * EditorZoomAmount);
            Drawing.DrawLine(p1, p2, lineColor, 1, false);
        }

        /// <summary>
        /// 1/2ラインの表示
        /// </summary>
        private void DrawCenterGuide()
        {
            var drawScene = Setting.SceneList[SelectedSceneIndex];
            if (!EnableCenterGuide || drawScene == null) return;

            var lineColor = new Color(1f, 0f, 0f, 0.5f);
            var centerX = position.width / 2f - (drawScene.width / 2f - EditorPosX) * EditorZoomAmount;
            var centerY = position.height / 2f - (drawScene.height / 2f - EditorPosY) * EditorZoomAmount;

            // 縦1/2
            var p1 = new Vector2(centerX + drawScene.width * 0.5f * EditorZoomAmount, 0);
            var p2 = new Vector2(centerX + drawScene.width * 0.5f * EditorZoomAmount, position.height);
            Drawing.DrawLine(p1, p2, lineColor, 1, false);
            // 横1/2
            p1 = new Vector2(-position.width, centerY + drawScene.height * 0.5f * EditorZoomAmount);
            p2 = new Vector2(position.width, centerY + drawScene.height * 0.5f * EditorZoomAmount);
            Drawing.DrawLine(p1, p2, lineColor, 1, false);
        }

        /// <summary>
        /// デバッグ変数の描画
        /// </summary>
        [Conditional("DEBUG")]
        private void DrawDebug()
        {
            // デバッグ用の数値を表示する
            EditorGUILayout.LabelField("X: " + EditorPosX);
            EditorGUILayout.LabelField("Y: " + EditorPosY);
            EditorGUILayout.LabelField("Zoom: " + EditorZoomAmount);
            EditorGUILayout.LabelField("Rect: " + CurrentRect.rect);
            EditorGUILayout.LabelField("Scale: " + CurrentRect.scale);
        }
        #endregion

        private void SetCurrentScale(float scale)
        {
            // ユニットのテクスチャスケールの更新
            var prevScale = CurrentRect.scale.x;
            CurrentRect.scale.x = scale;
            CurrentRect.scale.y = scale;

            // ずれた位置を修正する
            var prevW = CurrentRect.rect.width * prevScale;
            var prevH = CurrentRect.rect.height * prevScale;
            var afterW = CurrentRect.rect.width * scale;
            var afterH = CurrentRect.rect.height * scale;
            var diffW = afterW - prevW;
            var diffH = afterH - prevH;

            CurrentRect.rect.x -= diffW / 2f;
            CurrentRect.rect.y -= diffH / 2f;
        }

        #endregion
    }
}