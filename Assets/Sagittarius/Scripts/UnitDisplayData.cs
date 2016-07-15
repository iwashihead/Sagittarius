using System;
using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Griphone.Sagittarius
{
    /// <summary>
    /// ユニット表示領域に関するデータ.
    /// サジタリウスの出力データになります.
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "UnitDisplayData", menuName = "Sagittarius/UnitDisplayData", order = 0)]
    public class UnitDisplayData : ScriptableObject
    {
        public const string UNIT_DATA_PATH = "Assets/Sagittarius/Resources/ScriptableObject/UnitDisplayData.asset";
        private static UnitDisplayData instance;
        public static UnitDisplayData Instance
        {
            get
            {
#if UNITY_EDITOR
                instance = instance ?? AssetDatabase.LoadAssetAtPath<UnitDisplayData>(UNIT_DATA_PATH);
                if (instance == null) AssetDatabase.CreateAsset(instance, UNIT_DATA_PATH);
#endif
                return instance;
            }
        }

        public List<UnitData> UnitList = new List<UnitData>();

        /// <summary>
        /// 指定のユニットを取得します.
        /// </summary>
        /// <param name="id">ユニットID</param>
        /// <returns></returns>
        public UnitData GetUnitData(string id)
        {
            try
            {
                return UnitList.Find(_ => _.id == id);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            return null;
        }

        /// <summary>
        /// 指定の場面データを取得します.
        /// </summary>
        /// <param name="id">ユニットID</param>
        /// <param name="sceneId">場面ID</param>
        /// <returns></returns>
        public SceneData GetSceneData(string id, int sceneId)
        {
            try
            {
                return GetUnitData(id).sceneList[sceneId];
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            return null;
        }

        /// <summary>
        /// 指定の領域情報を取得します.
        /// </summary>
        /// <param name="id">ユニットID</param>
        /// <param name="elementId">属性ID</param>
        /// <param name="sceneId">場面ID</param>
        public RectData GetRectData(string id, int sceneId, int elementId)
        {
            try
            {
                var rect = GetSceneData(id, sceneId).Get(elementId);

                return rect;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            return null;
        }
    }
}
