using System;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Griphone.Sagittarius
{
    /// <summary>
    /// キャラクター単位のデータ.
    /// </summary>
    [Serializable]
    public class UnitData
    {
        /// <summary>
        /// ID. 設定に正規表現によるルールを設定することができます.
        /// </summary>
        public string id;

        /// <summary>
        /// キャラクター名. 制限なし
        /// </summary>
        public string name;

        /// <summary>
        /// サイズカテゴリーID.
        /// </summary>
        public int sizeId;

        /// <summary>
        /// ロックフラグ.
        /// true:ロック(削除、編集に関する操作禁止)
        /// false:非ロック
        /// </summary>
        public bool isLock;

        /// <summary>
        /// 各属性差分テクスチャのリスト.
        /// </summary>
        public List<TextureInfo> texList;

        /// <summary>
        /// 場面別データリスト.
        /// </summary>
        public List<SceneData> sceneList;


        public UnitData()
        {
            if (texList == null) texList = new List<TextureInfo>();
            if (sceneList == null) sceneList = new List<SceneData>();
        }

        public UnitData(Settings setting)
        {
            if (texList == null) texList = new List<TextureInfo>();

            for (int i = 0; i < setting.ElementList.Count; ++i)
            {
                texList.Add(new TextureInfo()
                {
                    elementId = i,
                    isSelected = false,
                    Texture = null
                });
            }

            if (sceneList == null) sceneList = new List<SceneData>();
            for (int i = 0; i < setting.SceneList.Count; ++i)
            {
                List<int> indexList = new List<int>();
                for (int j = 0; j < setting.ElementList.Count; ++j)
                {
                    indexList.Add(-1);
                }

                sceneList.Add(new SceneData()
                {
                    dataIndex = indexList,
                    rectData = new List<RectData>(new[] {new RectData()})
                });
            }
        }

        /// <summary>
        /// 設定に対するルールのチェックを実行します.
        /// 問題が有る場合エラー文字列を返します.
        /// </summary>
        public string Validate(Settings setting)
        {
            if (string.IsNullOrEmpty(id)) return ErrMsg("IDが空です.");
            if (!string.IsNullOrEmpty(setting.IdRuleRegex))
            {
                // Id名を正規表現でチェック.
                if (id != null && setting != null && Regex.IsMatch(id, setting.IdRuleRegex) == false)
                {
                    return ErrMsg("ID名がプロジェクトルールと異なります.\n" +
                                  "[設定] -> [IDの命名規則]を確認してください\n{0} Regex:{1}", id, setting.IdRuleRegex);
                }
            }

            return string.Empty;
        }

        private string ErrMsg(string msg, params object[] args)
        {
            return string.Format("[" + GetType().Name + "]" + msg, args);
        }
    }
}