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
        public Rectangle rect = new Rectangle(0,0,0,0);
        public Vector2 scale = Vector2.one;

        public RectData() { }
        public RectData(Texture2D tex)
        {
            rect = new Rectangle(0, 0, tex.width, tex.height);
            scale = Vector2.one;
        }
    }
}
