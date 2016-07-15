using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Griphone.Sagittarius
{
    /// <summary>
    /// 表示シーン毎のデータ.
    /// </summary>
    [Serializable]
    public class SceneData
    {
        /// <summary>
        /// どの属性がどのRectDataを向いているか.
        /// 配列のIndex = 属性Index
        /// </summary>
        public List<int> dataIndex;

        /// <summary>
        /// 領域情報.
        /// </summary>
        public List<RectData> rectData;

        /// <summary>
        /// 指定の属性の領域データを取得します.
        /// </summary>
        /// <param name="elementId">属性ID</param>
        public RectData Get(int elementId)
        {
            try
            {
                return rectData[dataIndex[elementId]];
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            return null;
        }

        /// <summary>
        /// 参照の無いデータを削除します.
        /// </summary>
        public void Clean()
        {
            // RectDataクラス参照のリスト.
            var refList = new List<RectData>(new RectData[dataIndex.Count]);
            for (int i = 0; i < dataIndex.Count; ++i)
            {
                if (dataIndex[i] >= 0)
                {
                    refList[i] = rectData[dataIndex[i]];
                }
                else
                {
                    refList[i] = null;
                }
            }

            var removeList = new List<RectData>();
            for (int i = 0; i < rectData.Count; ++i)
            {
                if (dataIndex.Contains(i) == false)
                {
                    // 参照されていないRectDataを削除対象に登録.
                    removeList.Add(rectData[i]);
                }
            }

            foreach (var item in removeList)
            {
                // 実体データの削除.
                rectData.Remove(item);
                // 削除データの参照はnullに.
                refList.ForEach(_ =>
                {
                    if (_ == item) _ = null;
                });
            }

            // Index情報を更新する.
            for (int i = 0; i < refList.Count; i++)
            {
                dataIndex[i] = refList == null ? -1 : rectData.IndexOf(refList[i]);
            }
        }
    }
}