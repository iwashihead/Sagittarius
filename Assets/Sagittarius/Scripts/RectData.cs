using System;
using UnityEngine;

namespace Griphone.Sagittarius
{
    /// <summary>
    /// 画像の領域情報.
    /// </summary>
    [Serializable]
    public class RectData
    {
        /// <summary>
        /// 表示領域
        /// </summary>
        public Rectangle rect = new Rectangle(0,0,0,0);

        /// <summary>
        /// スケール
        /// </summary>
        public Vector2 scale = Vector2.one;

        /// <summary>
        /// 背景画像を表示するかどうか.
        /// </summary>
        public bool enableBgTex;

        /// <summary>
        /// 背景画像のIndex.
        /// </summary>
        public int selectedBgTexIndex;

        /// <summary>
        /// フレーム画像を表示するかどうか.
        /// </summary>
        public bool enableFrameTex;

        /// <summary>
        /// フレーム画像のIndex.
        /// </summary>
        public int selectedFrameTexIndex;

        public RectData() { }
        public RectData(Texture2D tex)
        {
            rect = new Rectangle(0, 0, tex.width, tex.height);
            scale = Vector2.one;
        }
    }
}
