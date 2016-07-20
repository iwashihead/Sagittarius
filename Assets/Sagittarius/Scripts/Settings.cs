using UnityEngine;
using System;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Griphone.Sagittarius
{
    [Serializable]
    [CreateAssetMenu(fileName = "Sagittarius", menuName = "Sagittarius/Settings", order = 0)]
    public class Settings : ScriptableObject
    {
        public const string SETTING_DATA_PATH = "Assets/Sagittarius/Data/Settings.asset";
        private static Settings instance;
        public static Settings Instance
        {
            get
            {
#if UNITY_EDITOR
                instance = instance ?? AssetDatabase.LoadAssetAtPath<Settings>(SETTING_DATA_PATH);
                if (instance == null) AssetDatabase.CreateAsset(instance, SETTING_DATA_PATH);
#endif
                return instance;
            }
        }

        /// <summary>
        /// Unit名のルール.
        /// 正規表現でマッチング判定を行います
        /// </summary>
        public string IdRuleRegex;

        /// <summary>
        /// サイズのパターン.
        /// </summary>
        public List<string> SizeList;

        /// <summary>
        /// 属性のパターン.
        /// </summary>
        public List<Element> ElementList;

        /// <summary>
        /// 描画シーンのパターン.
        /// </summary>
        public List<DrawScene> SceneList;

        public Settings()
        {
            SizeList = new List<string>();
            ElementList = new List<Element>();
            SceneList = new List<DrawScene>();
        }
    }
}
