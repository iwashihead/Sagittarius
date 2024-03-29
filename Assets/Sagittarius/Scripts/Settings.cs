﻿using UnityEngine;
using System;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Griphone.Sagittarius
{
    /// <summary>
    /// サジタリウスの設定データ.
    /// Project毎に設定をしてください.
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "Sagittarius", menuName = "Sagittarius/Settings", order = 0)]
    public class Settings : ScriptableObject
    {
        public const string SETTING_DATA_PATH = "Assets/Sagittarius/Resources/ScriptableObject/SagittariusSettings.asset";
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

        /// <summary>
        /// 背景テクスチャリスト.
        /// </summary>
        public List<Texture2D> BgTextureList; 

        /// <summary>
        /// フレームのテクスチャリスト.
        /// </summary>
        public List<Texture2D> FrameTexList; 

        public Settings()
        {
            SizeList = new List<string>();
            ElementList = new List<Element>();
            SceneList = new List<DrawScene>();
            BgTextureList = new List<Texture2D>();
            FrameTexList = new List<Texture2D>();
        }
    }
}
