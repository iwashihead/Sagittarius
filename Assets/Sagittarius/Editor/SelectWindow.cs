using UnityEngine;
using UnityEditor;

namespace Griphone.Sagittarius
{
    /// <summary>
    /// 編集をするイラストを選択するウインドウ
    /// </summary>
    public class SelectWindow : EditorWindow
    {
        #region Static
        private static SelectWindow instance;
        public static SelectWindow Instance
        {
            get { return instance ?? (instance = GetWindow<SelectWindow>()); }
        }
        #endregion

        #region Private Declaration

        private Settings setting
        {
            get { return Settings.Instance; }
        }

        private Vector2 scrollPos;
        private int selectedIndex;
        private UnitDisplayData dataList
        {
            get { return UnitDisplayData.Instance; }
        }
        private UnitData addData;

        #endregion

        #region Public Method
        /// <summary>
        /// ウインドウを開くメニューコマンド
        /// </summary>
        [MenuItem("Window/Sagittarius/Open")]
        public static void Open()
        {
            GetWindow<SelectWindow>("Sagittarius.Select");
        }

        /// <summary>
        /// GUI描画イベント
        /// </summary>
        public void OnGUI()
        {
            DrawAddItemView();
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, true);
            DrawItemList();
            DrawSaveButton();
            EditorGUILayout.EndScrollView();
        }

        #endregion

        #region Private Method
        /// <summary>
        /// 要素の追加を表示します
        /// </summary>
        private void DrawAddItemView()
        {
            if (addData == null)
            {
                addData = new UnitData(setting);
            }

            EditorGUILayout.BeginVertical(GUI.skin.box);
            addData.id = EditorGUILayout.TextField("Id:", addData.id);
            addData.name = EditorGUILayout.TextField("名前:", addData.name);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("サイズ:");
            addData.sizeId = EditorGUILayout.Popup(addData.sizeId, setting.SizeList.ToArray());
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            var col = GUI.backgroundColor;
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("新規作成", GUILayout.Width(100), GUILayout.Height(25)))
            {
                var result = addData.Validate(setting);
                if (!string.IsNullOrEmpty(result))
                {
                    // 異常.
                    EditorUtility.DisplayDialog("警告", result, "OK");
                    return;
                }
                if (dataList.UnitList.Exists(_ => _.id == addData.id))
                {
                    // 既に要素が存在する
                    EditorUtility.DisplayDialog("警告", "同一IDが存在するため、新規追加をキャンセルします.", "OK");
                    return;
                }

                // 新規追加実行
                dataList.UnitList.Add(addData);
                addData = new UnitData(setting);
            }
            GUI.backgroundColor = col;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 要素一覧を表示します
        /// </summary>
        private void DrawItemList()
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical(GUI.skin.box);

            for (int i = 0; i < dataList.UnitList.Count; i++)
            {
                var data = dataList.UnitList[i];
                if (data == null)
                {
                    //TODO Warning表示.
                    continue;
                }

                EditorGUILayout.BeginHorizontal();

                {
                    // 選択ボタン ユニットIDと名前を表示.
                    var title = string.Format("{0} {1}", data.id, data.name);
                    var col = GUI.backgroundColor;
                    GUI.backgroundColor = selectedIndex == i ? Color.cyan : col;
                    if (GUILayout.Button(title))
                    {
                        OnClickSelect(i, data);
                    }
                    GUI.backgroundColor = col;
                }

                // ロック/アンロックボタン.
                if (GUILayout.Button(data.isLock ? "Unlock" : "Lock", GUILayout.Width(60)))
                {
                    OnClickLock(i, data);
                }

                {
                    // 削除ボタン.
                    GUI.enabled = !data.isLock;
                    var col = GUI.backgroundColor;
                    GUI.backgroundColor = Color.red;
                    if (GUILayout.Button("Delete", GUILayout.Width(80)))
                    {
                        OnClickDelete(i, data);
                    }
                    GUI.enabled = true;
                    GUI.backgroundColor = col;
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// セーブボタンを表示します
        /// </summary>
        private void DrawSaveButton()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            if (GUILayout.Button("保存"))
            {
                AssetDatabase.SaveAssets();
                ShowNotification(new GUIContent("保存が完了しました"));
            }
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// ユニット1件のボタンを押して選択した時の挙動.
        /// </summary>
        private void OnClickSelect(int index, UnitData data)
        {
            selectedIndex = index;
            //if (GUI.changed)
            {
                MainWindow.Instance.Current = data;
                MainWindow.Instance.Repaint();
            }
        }

        /// <summary>
        /// ユニット1件のロックボタンを押した時の挙動.
        /// </summary>
        private void OnClickLock(int index, UnitData data)
        {
            data.isLock = !data.isLock;
            //if (GUI.changed)
            {
                MainWindow.Instance.Current = dataList.UnitList[selectedIndex];
                MainWindow.Instance.Repaint();
            }
        }

        /// <summary>
        /// ユニット1件を削除した時の挙動.
        /// </summary>
        private void OnClickDelete(int index, UnitData data)
        {
            var result = EditorUtility.DisplayDialog(
                title: "確認",
                message: string.Format("ユニット:{0}を削除します。\nよろしいですか？", data.id + " " + data.name),
                ok: "OK",
                cancel: "Cancel");

            if (result)
            {
                // 削除実行.
                dataList.UnitList.Remove(data);
                if (selectedIndex == index)
                {
                    selectedIndex = -1;
                }
            }
        }

        #endregion
    }
}