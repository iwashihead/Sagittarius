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

        /// <summary>
        /// Rawイメージ用のUVを計算し、適用します
        /// </summary>
        public void ApplyUVRect(DrawScene scene)
        {
            //┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘
            //
            // サジタリウスからRawImageへの座標変換イメージ
            //
            //┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘
            //
            // サジタリウスはEditor座標系（左上）インターフェース（中心）の２つの座標系を変換し合っています.
            // 加えて、RawImageのuvRectは左下座標系なのでこのような複雑な計算をしています.
            // 
            //  ┌───────────────────────────┐
            //  │                           │
            //  │   r1                      │- - - - - - - - 元画像のRect
            //  │    ●─────────────────┐    │
            //  │    │  r2             │    │
            //  │    │   ●─────────┐   │- - ┼ - - - - - - - - 【r1】: サジタリウスにより編集された画像Rect
            //  │    │   │         │   │    │
            //  │    │   │         │   │    │
            //  │    │   │         │- -│- - ┼ - - - - - - - - 【r2】: サジタリウスのSettingで定義されたシーン別領域Rect
            //  │    │   │         │   │    │ 
            //  │    │   │         │   │    │
            //  │    │   └─────────┘   │    │
            //  │    │                 │    │
            //  │    └─────────────────┘    │
            //  │                           │
            //  │                           │
            //  └───────────────────────────┘

            var r1 = new Rect(-(rect.x * scale.x), -(rect.y * scale.y), rect.width * scale.x, rect.height * scale.y);
            var r2 = new Rect(0, 0, scene.width, scene.height);
            var baseX = rect.width * 0.5f * (1 - scale.x);
            var baseY = rect.height * 0.5f * (1 - scale.y);
            var posX = (rect.x - baseX) / r1.width;
            var posY = (rect.y - baseY) / r1.height;
            var scaleX = -(r2.width * 0.5f - r1.width * 0.5f) / r1.width;
            var scaleY = -(r2.height * 0.5f - r1.height * 0.5f) / r1.height;
            
            UVRect.x = scaleX - posX;
            UVRect.y = scaleY + posY;
            UVRect.width = r2.width / (rect.width * scale.x);
            UVRect.height = r2.height / (rect.height * scale.y);

            // For Debug.
            OutputLog(scene);
        }

        void OutputLog(DrawScene scene)
        {
            string logString = "";
            logString += GetLogString("scaleX", rect.width * scale.x);
            logString += GetLogString("scaleY", rect.height * scale.y);
            logString += GetLogString("rect", rect);
            logString += GetLogString("scale", scale);
            logString += GetLogString("scene", new Vector2(scene.width, scene.height));
            logString += GetLogString("uv", UVRect);
            Debug.LogWarning(logString);
        }

        string GetLogString(string title, object obj)
        {
            var s = string.Format("{0} : {1}", title, obj);
            //Debug.Log(s);
            return s + "\n";
        }
    }
}
