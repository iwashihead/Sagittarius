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
        #endregion

        // 編集対象のデータ.
        // このデータはSelectWindowからセットされます.
        public UnitData Current { get; set; }

        private Settings setting { get { return Settings.Instance; } }
        private Vector2 tabScrollPos;
        private int selectedSceneIndex;
        private List<Rect> windwList = new List<Rect>();

        // GUI描画イベント.
        public void OnGUI()
        {
            if (Current == null) return;
            GUI.enabled = !Current.isLock;

            DrawSelectedItemInfo();
            DrawTextureArea();
            DrawDataLinkArea();

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
            EditorGUILayout.BeginVertical(GUI.skin.box);
            DrawTabList();
            DrawDataList();

            EditorGUILayout.EndVertical();
        }

        // シーンタブ一覧を表示します.
        private void DrawTabList()
        {
            tabScrollPos = EditorGUILayout.BeginScrollView(tabScrollPos, true, false, GUILayout.ExpandWidth(true), GUILayout.Height(40));
            EditorGUILayout.BeginHorizontal();
            int index = 0;
            foreach (var scene in setting.SceneList)
            {
                if (GUILayout.Button(scene.name, GUILayout.Width(100)))
                {
                    selectedSceneIndex = index;
                }
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

        private void DrawDataNodes()
        {
            
        }
    }
}