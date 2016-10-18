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
        /// RawImage割り当て用のUV情報.
        /// </summary>
        public Rect UVRect = new Rect();

        /// <summary>
        /// 左右反転フラグ.
        /// </summary>
        public bool IsFlip;

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

        public void ApplyUVRect(DrawScene scene)
        {
            var p1 = new Rect(0, 0, rect.width, rect.height);
            var p2 = new Rect(-(rect.x * scale.x), -(rect.y * scale.y), rect.width * scale.x, rect.height * scale.y);
            var p3 = new Rect(0, 0, scene.width, scene.height);
            var p4 = new Vector2(p2.x + p2.width, p2.y + p2.height);
            var p5 = new Vector2((p3.x + p3.width) * scale.x, (p3.y + p3.height) * scale.y);

            //UVRect.x = (p4.x - p5.x) * 0.5f * scale.x / p2.width;
            //UVRect.y = (p4.y - p5.y) * 0.5f * scale.y / p2.height;

            // スケールだけならこのコードでOK！！
            UVRect.x = -(p3.width * 0.5f - p2.width * 0.5f) / p2.width;
            UVRect.y = -(p3.height * 0.5f - p2.height * 0.5f) / p2.height;
            UVRect.width = p3.width / (rect.width * scale.x);
            UVRect.height = p3.height / (rect.height * scale.y);

            //Debug.Log(string.Format("(p4.x - p5.x) * 0.5f / p2.width = ({0} - {1}) * 0.5f / {2}", p4.x, p5.x, p2.width));
            //Debug.Log(string.Format("(p4.y - p5.y) * 0.5f / p2.height = ({0} - {1}) * 0.5f / {2}", p4.y, p5.y, p2.height));

            string s = "";
            s += string.Format("(p4.x - p5.x) * 0.5f / p2.width = ({0} - {1}) * 0.5f / {2}", p4.x, p5.x, p2.width) + "\n";
            s += string.Format("(p4.y - p5.y) * 0.5f / p2.height = ({0} - {1}) * 0.5f / {2}", p4.y, p5.y, p2.height) + "\n";
            s += Log("scaleX", rect.width * scale.x);
            s += Log("scaleY", rect.height * scale.y);
            s += Log("rect", rect);
            s += Log("scale", scale);
            s += Log("scene", new Vector2(scene.width, scene.height));
            s += Log("p1", p1);
            s += Log("p2", p2);
            s += Log("p3", p3);
            s += Log("p4", p4);
            s += Log("p5", p5);
            s += Log("uv", UVRect);
            Debug.LogError(s);
        }

        string Log(string title, object obj)
        {
            var s = string.Format("{0} : {1}", title, obj);
            //Debug.Log(s);
            return s + "\n";
        }
    }
}
