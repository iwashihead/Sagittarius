using UnityEngine;
using UnityEngine.UI;

namespace Griphone.Sagittarius
{
    /// <summary>
    /// サジタリウスの領域データ設定をRAWイメージに反映させるコンポーネント.
    /// </summary>
    [RequireComponent(typeof(RawImage))]
    public class SgtImageFitter : MonoBehaviour
    {
        #region Public Declaration
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

        #region Private Declaration
        [SerializeField]
        private RawImage _rawImage;
        [SerializeField]
        private string _unitId;
        [SerializeField]
        private int _sceneId;
        [SerializeField]
        private int _elementId;

        private RectData _rectData;
        #endregion

        #region Public Method
        /// <summary>
        /// データをビューに反映させます
        /// </summary>
        [ContextMenu("Apply")]
        public void Apply()
        {
            // マスターデータから領域データを取得.
            _rectData = UnitDisplayData.Instance.GetRectData(UnitId, SceneId, ElementId);
            if (_rectData == null)
            {
                Debug.LogError(string.Format("[{0}] rectData not found!! UnitId:{1} SceneId:{2} ElementId:{3}", GetType().Name, UnitId, SceneId, ElementId));
                return;
            }

            var scene = GetSceneData();
            var scale = _rawImage.transform.localScale;
            _rawImage.rectTransform.sizeDelta = new Vector2(scene.width, scene.height);
            _rawImage.uvRect = _rectData.UVRect;
            _rawImage.transform.localScale = new Vector3(scale.x * (_rectData.IsFlip ? -1f : 1f), scale.y, scale.z);
        }
        #endregion

        #region Private Method
        /// <summary>
        /// サジタリウスの設定ファイル(Settings)から表示場面のデータを取得します.
        /// </summary>
        /// <returns>表示場面データ</returns>
        private DrawScene GetSceneData()
        {
            if (SceneId < 0 || SceneId >= Settings.Instance.SceneList.Count)
            {
                Debug.LogError(string.Format("[{0}] rectData not found!! UnitId:{1} SceneId:{2} ElementId:{3}", GetType().Name, UnitId, SceneId, ElementId));
                return null;
            }
            return Settings.Instance.SceneList[SceneId];
        }
        #endregion
    }
}