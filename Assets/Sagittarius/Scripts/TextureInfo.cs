using System;
using UnityEditor;
#if UNITY_EDITOR
using UnityEngine;
#endif

namespace Griphone.Sagittarius
{
    /// <summary>
    /// 画像情報.
    /// </summary>
    [Serializable]
    public class TextureInfo
    {
        [SerializeField]
        private Texture2D tex;
        public string path;
        public int elementId;
        public bool isSelected;

        public Texture2D Texture
        {
            get { return tex; }
            set
            {
                tex = value;
                if (tex != null)
                {
#if UNITY_EDITOR
                    path = AssetDatabase.GetAssetPath(tex);
#endif
                }
                else
                {
                    path = null;
                }
            }
        }
    }
}
