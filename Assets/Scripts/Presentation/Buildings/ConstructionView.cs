using UnityEngine;

namespace Settlers.Presentation
{
    /// <summary>
    /// Displays a construction progress bar above a building.
    /// Uses a simple quad that scales horizontally with progress.
    /// </summary>
    public class ConstructionView : MonoBehaviour
    {
        [SerializeField] private Transform _fillBar;
        [SerializeField] private GameObject _barRoot;

        private float _maxWidth;

        /// <summary>Set the progress (0.0 to 1.0). Hides bar when complete.</summary>
        public void SetProgress(float progress)
        {
            progress = Mathf.Clamp01(progress);

            if (progress >= 1f)
            {
                if (_barRoot != null)
                    _barRoot.SetActive(false);
                return;
            }

            if (_barRoot != null)
                _barRoot.SetActive(true);

            if (_fillBar != null)
            {
                var scale = _fillBar.localScale;
                scale.x = _maxWidth * progress;
                _fillBar.localScale = scale;

                // Keep fill anchored to left edge
                var pos = _fillBar.localPosition;
                pos.x = (_maxWidth * progress - _maxWidth) * 0.5f;
                _fillBar.localPosition = pos;
            }
        }

        /// <summary>
        /// Create a progress bar above a building.
        /// </summary>
        public static ConstructionView Create(Transform parent, float buildingWidth)
        {
            float barWidth = buildingWidth * 1.2f;
            float barHeight = 0.15f;
            float barY = GetBarHeight(parent);

            // Bar root
            var barRoot = new GameObject("ConstructionBar");
            barRoot.transform.SetParent(parent, false);
            barRoot.transform.localPosition = new Vector3(0f, barY, 0f);

            // Background (dark)
            var bgObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
            bgObj.name = "BarBackground";
            bgObj.transform.SetParent(barRoot.transform, false);
            bgObj.transform.localScale = new Vector3(barWidth, barHeight, 1f);
            bgObj.transform.localRotation = Quaternion.identity;

            var bgRenderer = bgObj.GetComponent<MeshRenderer>();
            var bgMat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            bgMat.color = new Color(0.15f, 0.15f, 0.15f, 0.8f);
            bgMat.SetFloat("_Surface", 1f);
            bgMat.renderQueue = 3100;
            bgRenderer.sharedMaterial = bgMat;
            Object.Destroy(bgObj.GetComponent<Collider>());

            // Fill bar (green)
            var fillObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
            fillObj.name = "BarFill";
            fillObj.transform.SetParent(barRoot.transform, false);
            fillObj.transform.localScale = new Vector3(0f, barHeight * 0.8f, 1f);
            fillObj.transform.localPosition = new Vector3(-barWidth * 0.5f, 0f, -0.01f);

            var fillRenderer = fillObj.GetComponent<MeshRenderer>();
            var fillMat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            fillMat.color = new Color(0.2f, 0.8f, 0.2f, 0.9f);
            fillMat.SetFloat("_Surface", 1f);
            fillMat.renderQueue = 3101;
            fillRenderer.sharedMaterial = fillMat;
            Object.Destroy(fillObj.GetComponent<Collider>());

            // Billboard component to face camera
            barRoot.AddComponent<BillboardBar>();

            // ConstructionView component
            var view = barRoot.AddComponent<ConstructionView>();
            view._fillBar = fillObj.transform;
            view._barRoot = barRoot;
            view._maxWidth = barWidth;

            return view;
        }

        private static float GetBarHeight(Transform building)
        {
            // Place bar above the tallest child
            float maxY = 2f;
            foreach (Transform child in building)
            {
                float top = child.localPosition.y + child.localScale.y * 0.5f;
                if (top > maxY) maxY = top;
            }
            return maxY + 0.5f;
        }
    }

    /// <summary>
    /// Makes a transform always face the camera (billboard effect).
    /// </summary>
    public class BillboardBar : MonoBehaviour
    {
        private Camera _camera;

        private void Start()
        {
            _camera = Camera.main;
        }

        private void LateUpdate()
        {
            if (_camera == null) return;
            transform.rotation = Quaternion.LookRotation(
                transform.position - _camera.transform.position);
        }
    }
}
