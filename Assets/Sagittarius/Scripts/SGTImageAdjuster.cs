using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Griphone.Sagittarius
{
    /// <summary>
    /// サジタリウスの領域データ設定をRAWイメージに反映させるコンポーネント.
    /// </summary>
    [RequireComponent(typeof(RawImage))]
    public class SGTImageAdjuster : MonoBehaviour
    {
        #region Property
        /// <summary>
        /// ユニットID
        /// </summary>
        public string UnitId
        {
            get { return _unitId; }
            set { _unitId = value; Apply(); }
        }
        /// <summary>
        /// シーンID（UnityのSceneではなく、再生場面の意味.）
        /// </summary>
        public int SceneId
        {
            get { return _sceneId; }
            set { _sceneId = value; Apply(); }
        }
        /// <summary>
        /// 属性ID
        /// </summary>
        public int ElementId
        {
            get { return _elementId; }
            set { _elementId = value; Apply(); }
        }
        #endregion

        [SerializeField]
        private RawImage _rawImage;
        [SerializeField]
        private string _unitId;
        [SerializeField]
        private int _sceneId;
        [SerializeField]
        private int _elementId;


        public RectTransform P1;
        public RectTransform P2;
        public RectTransform P3;
        public Canvas Canvas;

        private RectData _rectData;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        [ContextMenu("Apply")]
        private void Apply()
        {
            _rectData = UnitDisplayData.Instance.GetRectData(UnitId, SceneId, ElementId);

#if false
            //┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘
            //
            // サジタリウスからRawImageへの座標変換イメージ
            //
            //┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘┘
            // 
            // p1 (0, 0)
            //  ●───────────────────────────┐
            //  │                           │
            //  │   p2                      │- - - - - - - - 元画像のRect
            //  │    ●─────────────────┐    │                 (0,0,tex.width,tex.height) 
            //  │    │  p3             │    │
            //  │    │   ●─────────┐   │- - ┼ - - - - - - - - サジタリウスにより編集された画像Rect
            //  │    │   │         │   │    │                 (x * scale.x, y * scale.y, tex.width * scale.x, tex.height * scale.y)
            //  │    │   │         │   │    │
            //  │    │   │         │- -│- - ┼ - - - - - - - - サジタリウスのSettingで定義されたシーン別領域Rect
            //  │    │   │         │   │    │                 (-scene.width / 2, -scene.height / 2, scene.witdh, scene.height)
            //  │    │   │         │   │    │
            //  │    │   ●─────────┘   │    │
            //  │    │  p5             │    │
            //  │    ●─────────────────┘    │
            //  │   p4                      │
            //  │                           │
            //  └───────────────────────────┘


            Rectangle rect = _rectData.rect;//サジタリウスで編集した領域データ
            Vector2 scale = _rectData.scale;//サジタリウスで編集した倍率データ
            DrawScene scene = GetSceneData();//サジタリウスで選択された表示場面データ

            var p1 = new Rect(0, 0, rect.width, rect.height);
            var p2 = new Rect(-(rect.x * scale.x), -(rect.y * scale.y), rect.width * scale.x, rect.height * scale.y);
            //var p3 = new Rect(-scene.width * 0.5f, -scene.height * 0.5f, scene.width, scene.height);
            var p3 = new Rect(0, 0, scene.width, scene.height);
            var p4 = new Vector2(p2.x + p2.width, p2.y + p2.height);
            var p5 = new Vector2(p3.x + p3.width, p3.y + p3.height);

            var v1 = new Vector2(p2.x - p1.x, p2.y - p1.y);
            var v2 = new Vector2(p3.x - p1.x, p3.y - p1.y);
            var v3 = v2 - v1;

            // 左上原点 → 右下原点に
            //v3.y += rect.height * scale.y;
            //v3 *= -1;

            Rect uv = new Rect();
            uv.x = (p4.x - p5.x) * 0.5f / p2.width;
            uv.y = (p4.y - p5.y) * 0.5f / p2.height;
            Debug.Log(string.Format("(p4.x - p5.x) * 0.5f / p2.width = ({0} - {1}) * 0.5f / {2}", p4.x, p5.x, p2.width));
            Debug.Log(string.Format("(p4.y - p5.y) * 0.5f / p2.height = ({0} - {1}) * 0.5f / {2}", p4.y, p5.y, p2.height));
            //uv.x = v3.x / scene.width;
            //uv.y = v3.y / scene.height;
            //uv.x = (p2.width * 0.5f - p2.x - p3.width * 0.5f) * scale.x / p2.width;
            //uv.y = (p2.height * 0.5f - p2.height + p3.height * 0.5f) * scale.y / p2.height + 1;
            uv.width = p3.width / (rect.width * scale.x);
            uv.height = p3.height / (rect.height * scale.y);


            Log("scaleX", rect.width * scale.x);
            Log("scaleY", rect.height * scale.y);
            Log("rect", rect);
            Log("scale", scale);
            Log("scene", new Vector2(scene.width, scene.height));
            Log("p1", p1);
            Log("p2", p2);
            Log("p3", p3);
            Log("p4", p4);
            Log("p5", p5);
            Log("v1", v1);
            Log("v2", v2);
            Log("v3", v3);
            Log("uv", uv);

            // Imageの表示更新
            var c = new Vector2(Canvas.transform.position.x, Canvas.transform.position.y);
            P1.position = p1.position + c;
            P1.sizeDelta = p1.size;
            P2.position = p2.position + c;
            P2.sizeDelta = p2.size;
            P3.position = p3.position + c;
            P3.sizeDelta = p3.size;
#endif

            var scene = GetSceneData();
            var scale = _rawImage.transform.localScale;
            _rawImage.rectTransform.sizeDelta = new Vector2(scene.width, scene.height);
            _rawImage.uvRect = _rectData.UVRect;
            _rawImage.transform.localScale = new Vector3(scale.x * (_rectData.IsFlip ? -1f : 1f), scale.y, scale.z);
        }

        private DrawScene GetSceneData()
        {
            if (SceneId < 0 || SceneId >= Settings.Instance.SceneList.Count) return null;
            return Settings.Instance.SceneList[SceneId];
        }

        void Log(string title, object obj)
        {
            Debug.Log(string.Format("{0} : {1}", title, obj));
        }
    }
}