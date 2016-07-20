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
        public Rect rect;
        public Vector2 scale;

        public RectData() { }
        public RectData(Texture2D tex)
        {
            rect = new Rect(0, 0, tex.width, tex.height);
            scale = Vector2.one;
        }
    }
}
