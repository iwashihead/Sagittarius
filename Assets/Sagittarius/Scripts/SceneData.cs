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
            int[] refs = new int[rectData.Count];

            int cnt = rectData.Count;
            List<RectData> removeList = new List<RectData>();
            for (int i = 0; i < cnt; ++i)
            {
                if (dataIndex.Contains(i) == false)
                {
                    removeList.Add(rectData[i]);
                }
            }

            foreach (var item in removeList)
            {
                rectData.Remove(item);
            }
        }
    }
}