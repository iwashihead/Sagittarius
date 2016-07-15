using System;

namespace Griphone.Sagittarius
{
    /// <summary>
    /// 描画場面の情報.
    /// </summary>
    [Serializable]
    public class DrawScene
    {
        /// <summary>
        /// 場面名称.
        /// </summary>
        public string name;

        /// <summary>
        /// コメント/説明.
        /// </summary>
        public string comment;

        /// <summary>
        /// 表示領域幅[pixel]
        /// </summary>
        public int width;

        /// <summary>
        /// 表示領域高さ[pixel]
        /// </summary>
        public int height;
    }
}