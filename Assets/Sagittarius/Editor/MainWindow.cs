using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Griphone.Sagittarius
{
    /// <summary>
    /// サジタリウスのデータの操作を行うメインウインドウ.
    /// </summary>
    public class MainWindow : EditorWindow
    {
        #region static
        private static MainWindow _instance;
        public static MainWindow Instance
        {
            get { return _instance ?? (_instance = GetWindow<MainWindow>("Sagittarius.Data")); }
        }

        /// <summary>
        /// ノード領域サイズ.
        /// </summary>
        private static Rect NodeViewSize
        {
            get
            {
                return new Rect(
                    Instance.position.x - ConstrainX,
                    Instance.position.y - ConstrainY,
                    Instance.position.width - ConstrainWidth,
                    Instance.position.height - ConstrainHeight);
            }
        }

        private const float ConstrainX = 120;
        private const float ConstrainY = 180;
        private const float ConstrainWidth = 20;
        private const float ConstrainHeight = 20;
        private const float NodeStartY = 200;
        private const float ElementStartY = 181;
        private const float ElementHeight = 41.31f;

        /// <summary>
        /// ノードウインドウ
        /// </summary>
        private class NodeWindow
        {
            public Rect rect;
        }

        /// <summary>
        /// データノードウインドウ
        /// </summary>
        private class DataNodeWindow : NodeWindow
        {
            public int selectedDataIndex;
        }
        #endregion

        #region Public Declaration
        /// <summary>
        /// 編集対象のデータ.
        /// このデータはSelectWindowからセットされます.
        /// </summary>
        public UnitData Current;
        #endregion

        #region Private Declaration
        private Settings Setting { get { return Settings.Instance; } }
        private Vector2 _tabScrollPos;
        private int _selectedSceneIndex;
        private List<DataNodeWindow> _windowList = new List<DataNodeWindow>();
        #endregion

        #region Unity Event

        /// <summary>
        /// 破棄イベント.
        /// </summary>
        public void OnDestroy()
        {
            _instance = null;
        }

        /// <summary>
        /// GUI描画イベント.
        /// </summary>
        public void OnGUI()
        {
            if (Current == null) return;
            GUI.enabled = !Current.isLock;

            DrawSelectedItemInfo();
            DrawTextureArea();

            EditorGUILayout.BeginVertical(GUI.skin.box);
            DrawDataLinkArea();
            DrawDataNodes();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndVertical();

            GUI.enabled = true;
        }

        #endregion

        #region Draw Method
        /// <summary>
        /// 選択中のアイテムを表示する領域.
        /// </summary>
        private void DrawSelectedItemInfo()
        {
            SgtEditorUtility.VerticalBlock(() =>
            {
                Current.name = EditorGUILayout.TextField("ID : " + Current.id, Current.name);
                SgtEditorUtility.HorizontalBlock(() =>
                {
                    EditorGUILayout.LabelField("サイズ : ");
                    Current.sizeId = EditorGUILayout.Popup(Current.sizeId, Setting.SizeList.ToArray());
                });
                if (GUI.changed) SelectWindow.Instance.Repaint();
            }, GUI.skin.box);
        }

        /// <summary>
        /// 属性毎テクスチャの編集領域を表示します.
        /// </summary>
        private void DrawTextureArea()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);

            if (Current.texList == null)
                Current.texList = new List<TextureInfo>();

            for (int i = 0; i < Setting.ElementList.Count; i += 2)
            {
                EditorGUILayout.BeginHorizontal();
                // 左側
                DrawTextureItem(i);
                // 右側
                if (i + 1 < Setting.ElementList.Count)
                {
                    DrawTextureItem(i + 1);
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("保存", GUILayout.Height(25), GUILayout.MaxWidth(100)))
            {
                AssetDatabase.SaveAssets();
                ShowNotification(new GUIContent("保存が完了しました"));
            }
            if (GUILayout.Button("読み込み", GUILayout.Height(25), GUILayout.MaxWidth(100)))
            {
                OnClickLoadTexture();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// テクスチャ設定の1件表示.
        /// </summary>
        private void DrawTextureItem(int index)
        {
            if (Current.texList.Count <= index)
            {
                Current.texList.Add(new TextureInfo()
                {
                    elementId = index,
                    isSelected = false,
                    Texture = null
                });
            }

            // 選択フラグ.
            Current.texList[index].isSelected = EditorGUILayout.Toggle(Current.texList[index].isSelected, GUILayout.Width(15));

            // 属性アイコン.
            EditorGUILayout.LabelField(Setting.ElementList[index].name, new GUIStyle()
            {
                fontSize = 16,
                fontStyle = FontStyle.Normal,
                normal = new GUIStyleState() { textColor = Setting.ElementList[index].color }
            }, GUILayout.Width(18));

            // テクスチャ参照.
            Current.texList[index].Texture = EditorGUILayout.ObjectField(Current.texList[index].Texture, typeof(Texture2D), false) as Texture2D;

            EditorGUILayout.Space();
        }

        /// <summary>
        /// 属性毎のデータとの紐付きを表示します.
        /// </summary>
        private void DrawDataLinkArea()
        {
            DrawTabList();
            DrawDataList();
        }

        /// <summary>
        /// シーンタブ一覧を表示します.
        /// </summary>
        private void DrawTabList()
        {
            _tabScrollPos = EditorGUILayout.BeginScrollView(_tabScrollPos, true, false, GUILayout.ExpandWidth(true), GUILayout.Height(40));
            EditorGUILayout.BeginHorizontal();
            int index = 0;
            foreach (var scene in Setting.SceneList)
            {
                var col = GUI.backgroundColor;
                GUI.backgroundColor = _selectedSceneIndex == index ? Color.cyan : col;
                if (GUILayout.Button(scene.name, GUILayout.Width(100)))
                {
                    _selectedSceneIndex = index;
                }
                GUI.backgroundColor = col;
                index++;
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// 属性アイコンを表示します.
        /// </summary>
        private void DrawDataList()
        {
            EditorGUILayout.LabelField(Setting.SceneList[_selectedSceneIndex].name, EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical();
            for (int i = 0; i < Setting.ElementList.Count; ++i)
            {
                EditorGUILayout.BeginHorizontal();

                // テクスチャ有無でトグル表示.
                // TODO : 見栄えが悪いので、できればラベルではなくアイコンにしたい
                bool existTex = Current.texList[i].Texture != null;
                EditorGUILayout.LabelField("●",
                    new GUIStyle()
                    {
                        fontSize = 20,
                        fontStyle = FontStyle.Normal,
                        normal = new GUIStyleState() { textColor = existTex ? Color.green : Color.gray }
                    }, GUILayout.Width(12));

                var col = GUI.backgroundColor;
                GUI.backgroundColor = Setting.ElementList[i].color;
                if (GUILayout.Button(Setting.ElementList[i].name, GUILayout.Width(80), GUILayout.Height(25)))
                {
                    OnClickElementIcon(i);
                }
                GUI.backgroundColor = col;
                GUILayout.FlexibleSpace();

                EditorGUILayout.EndHorizontal();
                GUILayout.Space(15);
            }
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 属性とデータの紐付きをノードで表示します.
        /// </summary>
        private void DrawDataNodes()
        {
            if (_windowList == null)
            {
                _windowList = new List<DataNodeWindow>();
            }

            var scene = Current.sceneList[_selectedSceneIndex];

            // Windowの初期化.
            for (int i = 0; i < scene.rectData.Count; ++i)
            {
                if (_windowList.Count <= i)
                {
                    _windowList.Add(new DataNodeWindow()
                    {
                        rect = new Rect(200, NodeStartY + 60 * i, 280, 50),
                        selectedDataIndex = -1
                    });
                }
            }

            // Nodeラインの表示.
            for (int i = 0; i < scene.dataIndex.Count; ++i)
            {
                if (scene.dataIndex[i] < 0) continue;
                var elementRect = new Rect(0, ElementStartY + ElementHeight * i, 100, 50);
                var window = _windowList[scene.dataIndex[i]];
                Drawing.CurveFromTo(elementRect, window.rect, Setting.ElementList[i].color, new Color(0.2f, 0.2f, 0.2f, 1f));
            }

            // Nodeウインドウの表示.
            BeginWindows();
            for (int i = 0; i < scene.rectData.Count; ++i)
            {
                // Windowの描画.
                _windowList[i].rect = GUI.Window(i, _windowList[i].rect, DrawNodeWindow, "領域データ " + i);
                // Windowの移動位置制限.
                _windowList[i].rect = SgtEditorUtility.ConstrainRect(_windowList[i].rect, NodeViewSize, this);
            }
            EndWindows();
        }

        /// <summary>
        /// ノード形式のウインドウを表示します.
        /// </summary>
        private void DrawNodeWindow(int windowId)
        {
            var e = GUI.enabled;
            GUI.enabled = !Current.isLock;

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("編集")) { OnClickEditButton(windowId); }
            EditorGUILayout.Space();
            if (GUILayout.Button("プレビュー")) { OnClickPreviewButton(windowId); }

            var col = GUI.backgroundColor;
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("消去")) { OnClickDeleteButton(windowId); }
            GUI.backgroundColor = col;

            {
                var enable = GUI.enabled;
                if (_windowList.Count > windowId)
                {
                    GUI.enabled = _windowList[windowId].selectedDataIndex >= 0 && !Current.isLock;
                }
                if (GUILayout.Button("同期"))
                {
                    OnClickSyncButton(windowId);
                }
                GUI.enabled = enable;
            }

            var popupList = new List<string>(new []{ "" });
            for (int i = 0; i < Current.sceneList[_selectedSceneIndex].rectData.Count; ++i)
            {
                popupList.Add(i.ToString());
            }

            if (_windowList.Count > windowId)
            {
                _windowList[windowId].selectedDataIndex = EditorGUILayout.Popup(
                    _windowList[windowId].selectedDataIndex,
                    popupList.ToArray());
            }

            EditorGUILayout.EndHorizontal();
            GUI.DragWindow();

            GUI.enabled = e;
        }

        #endregion

        #region Button Callback
        /// <summary>
        /// テクスチャ読み込みボタンを押した時の挙動.
        /// </summary>
        private void OnClickLoadTexture()
        {
            //TODO IDのレギュレーションに合ったテクスチャを読み込む処理
        }

        /// <summary>
        /// 属性アイコンを押した時の挙動.
        /// </summary>
        private void OnClickElementIcon(int elementId)
        {
            var scene = Current.sceneList[_selectedSceneIndex];

            if (scene.rectData == null) scene.rectData = new List<RectData>();
            if (scene.dataIndex == null) scene.dataIndex = new List<int>();

            if (scene.dataIndex[elementId] < 0)
            {
                if (scene.rectData.Count < elementId)
                {
                    scene.rectData.Add(new RectData());
                }
                if (scene.dataIndex.Count < elementId)
                {
                    scene.dataIndex.Add(-1);
                }

                // 未指定の場合、新しいデータを作成し、そのデータを参照する.
                scene.rectData.Add(new RectData());
                scene.dataIndex[elementId] = scene.rectData.Count - 1;
                scene.Clean();
            }
            else
            {
                // 指定されている場合、ダイアログで確認した上でデータ参照を解除
                var result = EditorUtility.DisplayDialog("確認", "データのリンクを解除します.\nよろしいですか？", "OK", "Cancel");
                if (result)
                {
                    scene.dataIndex[elementId] = -1;
                    scene.Clean();
                }
            }
        }

        /// <summary>
        /// 編集ボタンを押した時の挙動.
        /// </summary>
        private void OnClickEditButton(int windowId)
        {
            EditWindow.Instance.Current = Current;
            EditWindow.Instance.SelectedSceneIndex = _selectedSceneIndex;
            EditWindow.Instance.SelectedRectIndex = windowId;
            EditWindow.Instance.IsPreviewMode = false;
            EditWindow.Instance.Initialize();
        }

        /// <summary>
        /// プレビューボタンを押した時の挙動.
        /// </summary>
        private void OnClickPreviewButton(int windowId)
        {
            EditWindow.Instance.Current = Current;
            EditWindow.Instance.SelectedSceneIndex = _selectedSceneIndex;
            EditWindow.Instance.SelectedRectIndex = windowId;
            EditWindow.Instance.IsPreviewMode = true;
            EditWindow.Instance.Initialize();
        }

        /// <summary>
        /// 削除ボタンを押した時の挙動.
        /// </summary>
        private void OnClickDeleteButton(int windowId)
        {
            var result = EditorUtility.DisplayDialog("確認", "領域データ " + windowId + " を削除します。\nよろしいですか？", "OK", "Cancel");
            if (result)
            {
                var scene = Current.sceneList[_selectedSceneIndex];
                scene.rectData[windowId] = null;
                for (int i = 0; i < scene.dataIndex.Count; i++)
                {
                    if (scene.dataIndex[i] == windowId)
                        scene.dataIndex[i] = -1;
                }
                _windowList.RemoveAt(windowId);
                scene.Clean();
            }
        }

        /// <summary>
        /// 同期ボタンを押した時の挙動.
        /// </summary>
        private void OnClickSyncButton(int windowId)
        {
            var scene = Current.sceneList[_selectedSceneIndex];
            var syncTargetRectIndex = _windowList[windowId].selectedDataIndex - 1;

            var result = EditorUtility.DisplayDialog(
                "確認",
                string.Format("データ {0} を データ {1} に同期します。\nよろしいですか？", windowId, syncTargetRectIndex),
                "OK", "Cancel");

            if (result)
            {
                for (int i = 0; i < scene.dataIndex.Count; ++i)
                {
                    if (scene.dataIndex[i] == windowId)
                    {
                        scene.dataIndex[i] = syncTargetRectIndex;
                    }
                }
                scene.Clean();
            }
            foreach (var nodeWindow in _windowList)
            {
                nodeWindow.selectedDataIndex = -1;
            }
        }

        #endregion
    }
}