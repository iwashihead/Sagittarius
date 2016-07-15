using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Griphone.Sagittarius
{
    public class MainWindow : EditorWindow
    {
        #region static
        private static MainWindow instance;
        public static MainWindow Instance
        {
            get { return instance ?? (instance = GetWindow<MainWindow>()); }
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
                    Instance.position.width - ConstrainW,
                    Instance.position.height - ConstrainH);
            }
        }

        private const float ConstrainX = 120;
        private const float ConstrainY = 180;
        private const float ConstrainW = 20;
        private const float ConstrainH = 20;
        private const float NodeStartY = 200;
        private const float ElementStartY = 256 - 75;
        private const float ElementHeight = 41.31f;

        private class NodeWindow
        {
            public Rect rect;
        }

        private class DataNodeWindow : NodeWindow
        {
            public int selectedDataIndex;
        }
        #endregion

        // 編集対象のデータ.
        // このデータはSelectWindowからセットされます.
        public UnitData Current { get; set; }

        private Settings setting { get { return Settings.Instance; } }
        private Vector2 tabScrollPos;
        private int selectedSceneIndex;
        private List<DataNodeWindow> windowList = new List<DataNodeWindow>();

        // GUI描画イベント.
        public void OnGUI()
        {
            if (Current == null) return;
            GUI.enabled = !Current.isLock;

            DrawSelectedItemInfo();
            DrawTextureArea();

//            ConstrainX = EditorGUILayout.FloatField("ConstrainX", ConstrainX);
//            ConstrainY = EditorGUILayout.FloatField("ConstrainY", ConstrainY);
//            ConstrainW = EditorGUILayout.FloatField("ConstrainW", ConstrainW);
//            ConstrainH = EditorGUILayout.FloatField("ConstrainH", ConstrainH);

            EditorGUILayout.BeginVertical(GUI.skin.box);
            DrawDataLinkArea();
            DrawDataNodes();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndVertical();

            GUI.enabled = true;
        }

        // 選択中のアイテムを表示する領域.
        private void DrawSelectedItemInfo()
        {
            SgtEditorUtility.DrawVertical(() =>
            {
                Current.name = EditorGUILayout.TextField("ID : " + Current.id, Current.name);
                SgtEditorUtility.DrawHorizontal(() =>
                {
                    EditorGUILayout.LabelField("サイズ : ");
                    Current.sizeId = EditorGUILayout.Popup(Current.sizeId, setting.SizeList.ToArray());
                });
                if (GUI.changed) SelectWindow.Instance.Repaint();
            }, GUI.skin.box);
        }

        // 属性毎テクスチャの編集領域を表示します.
        private void DrawTextureArea()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);

            if (Current.texList == null)
                Current.texList = new List<TextureInfo>();

            for (int i = 0; i < setting.ElementList.Count; i+=2)
            {
                EditorGUILayout.BeginHorizontal();
                // 左側
                DrawTextureItem(i, Current.texList);
                // 右側
                if (i + 1 < setting.ElementList.Count)
                {
                    DrawTextureItem(i + 1, Current.texList);
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("読み込み", GUILayout.Height(25), GUILayout.MaxWidth(100)))
            {
                OnClickLoadTexture();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        // テクスチャ設定の1件表示.
        private void DrawTextureItem(int index, List<TextureInfo> texList)
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
            EditorGUILayout.LabelField(setting.ElementList[index].name, new GUIStyle()
            {
                fontSize = 16,
                fontStyle = FontStyle.Normal,
                normal = new GUIStyleState() {textColor = setting.ElementList[index].color}
            }, GUILayout.Width(18));

            // テクスチャ参照.
            Current.texList[index].Texture = EditorGUILayout.ObjectField(Current.texList[index].Texture, typeof (Texture2D), false) as Texture2D;

            EditorGUILayout.Space();
        }

        // テクスチャ読み込みボタンを押した時の挙動.
        private void OnClickLoadTexture()
        {
            //TODO IDのレギュレーションに合ったテクスチャを読み込む処理?
        }

        // 属性毎のデータとの紐付きを表示します.
        private void DrawDataLinkArea()
        {
            DrawTabList();
            DrawDataList();
        }

        // シーンタブ一覧を表示します.
        private void DrawTabList()
        {
            tabScrollPos = EditorGUILayout.BeginScrollView(tabScrollPos, true, false, GUILayout.ExpandWidth(true), GUILayout.Height(40));
            EditorGUILayout.BeginHorizontal();
            int index = 0;
            foreach (var scene in setting.SceneList)
            {
                var col = GUI.backgroundColor;
                GUI.backgroundColor = selectedSceneIndex == index ? Color.cyan : col;
                if (GUILayout.Button(scene.name, GUILayout.Width(100)))
                {
                    selectedSceneIndex = index;
                }
                GUI.backgroundColor = col;
                index++;
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndScrollView();
        }

        // 属性アイコンを表示します.
        private void DrawDataList()
        {
            EditorGUILayout.LabelField(setting.SceneList[selectedSceneIndex].name, EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical();
            for (int i = 0; i < setting.ElementList.Count; ++i)
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
                GUI.backgroundColor = setting.ElementList[i].color;
                if (GUILayout.Button(setting.ElementList[i].name, GUILayout.Width(80), GUILayout.Height(25)))
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

        // 属性アイコンを押した時の挙動.
        private void OnClickElementIcon(int elementId)
        {
            var scene = Current.sceneList[selectedSceneIndex];

            if (scene.rectData == null) scene.rectData = new List<RectData>();
            if (scene.dataIndex == null) scene.dataIndex = new List<int>();
            if (scene.rectData.Count < elementId)
            {
                scene.rectData.Add(new RectData());
            }
            if (scene.dataIndex.Count < elementId)
            {
                scene.dataIndex.Add(-1);
            }

            if (scene.dataIndex[elementId] < 0)
            {
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

        // 属性とデータの紐付きをノードで表示します.
        private void DrawDataNodes()
        {
            if (windowList == null)
            {
                windowList = new List<DataNodeWindow>();
            }

            var scene = Current.sceneList[selectedSceneIndex];

            // Windowの初期化.
            for (int i = 0; i < scene.rectData.Count; ++i)
            {
                if (windowList.Count <= i)
                {
                    Debug.Log("Add Window");
                    windowList.Add(new DataNodeWindow()
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
                var window = windowList[scene.dataIndex[i]];
                CurveFronTo(elementRect, window.rect, setting.ElementList[i].color, new Color(0.2f, 0.2f, 0.2f, 1f));
            }

            // Nodeウインドウの表示.
            BeginWindows();
            for (int i = 0; i < scene.rectData.Count; ++i)
            {
                // Windowの描画.
                windowList[i].rect = GUI.Window(i, windowList[i].rect, DrawNodeWindow, "領域データ " + i);
                // Windowの移動位置制限.
                windowList[i].rect = ConstrainRect(windowList[i].rect, NodeViewSize);
            }
            EndWindows();
        }

        // ノード形式のウインドウを表示します.
        private void DrawNodeWindow(int id)
        {
            var e = GUI.enabled;
            GUI.enabled = !Current.isLock;

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("編集")) { OnClickEditButton(); }
            EditorGUILayout.Space();
            if (GUILayout.Button("プレビュー")) { OnClickPreviewButton(); }

            var col = GUI.backgroundColor;
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("消去")) { OnClickDeleteButton(id); }
            GUI.backgroundColor = col;

            {
                var enable = GUI.enabled;
                GUI.enabled = windowList[id].selectedDataIndex >= 0 && e;
                if (GUILayout.Button("同期"))
                {
                    OnClickSyncButton();
                }
                GUI.enabled = enable;
            }

            var popupList = new List<string>(new []{ "" });
            for (int i = 0; i < Current.sceneList[selectedSceneIndex].rectData.Count; ++i)
            {
                popupList.Add(i.ToString());
            }
            windowList[id].selectedDataIndex = EditorGUILayout.Popup(
                windowList[id].selectedDataIndex,
                popupList.ToArray());

            EditorGUILayout.EndHorizontal();
            GUI.DragWindow();

            GUI.enabled = e;
        }

        // 編集ボタンを押した時の挙動.
        private void OnClickEditButton()
        {
            // TODO
        }

        // プレビューボタンを押した時の挙動.
        private void OnClickPreviewButton()
        {
            // TODO
        }

        // 削除ボタンを押した時の挙動.
        private void OnClickDeleteButton(int windowId)
        {
            var result = EditorUtility.DisplayDialog("確認", "領域データ " + windowId + " を削除します。\nよろしいですか？", "OK", "Cancel");
            if (result)
            {
                var scene = Current.sceneList[selectedSceneIndex];
                scene.rectData[windowId] = null;
                for (int i = 0; i < scene.dataIndex.Count; i++)
                {
                    if (scene.dataIndex[i] == windowId)
                        scene.dataIndex[i] = -1;
                }
                windowList.RemoveAt(windowId);
                scene.Clean();
            }
        }

        // 同期ボタンを押した時の挙動.
        private void OnClickSyncButton()
        {
            // TODO
        }

        // Window間にベジェ曲線でラインを引きます.
        void CurveFronTo(Rect wr, Rect wr2, Color color, Color shadow)
        {
            Drawing.bezierLine(
                new Vector2(wr.x + wr.width, wr.y + 1 + wr.height / 2),
                new Vector2(wr.x + wr.width + Mathf.Abs(wr2.x - (wr.x + wr.width)) / 2, wr.y + 3 + wr.height / 2),
                new Vector2(wr2.x, wr2.y + 1 + wr2.height / 2),
                new Vector2(wr2.x - Mathf.Abs(wr2.x - (wr.x + wr.width)) / 2, wr2.y + 3 + wr2.height / 2), shadow, 3, true, 20);
            Drawing.bezierLine(
                new Vector2(wr.x + wr.width, wr.y + wr.height / 2),
                new Vector2(wr.x + wr.width + Mathf.Abs(wr2.x - (wr.x + wr.width)) / 2, wr.y + wr.height / 2),
                new Vector2(wr2.x, wr2.y + wr2.height / 2),
                new Vector2(wr2.x - Mathf.Abs(wr2.x - (wr.x + wr.width)) / 2, wr2.y + wr2.height / 2), color, 2, true, 20);
        }

        // Windowの領域に制限をかけます
        Rect ConstrainRect(Rect window, Rect constraintsSize)
        {
            window.x = Mathf.Clamp(window.x, position.x - constraintsSize.x, constraintsSize.width - window.width);
            window.y = Mathf.Clamp(window.y, position.y - constraintsSize.y, constraintsSize.height - window.height);
            return window;
        }
    }
}