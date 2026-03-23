using UnityEngine;
using Settlers.Simulation;

namespace Settlers.Presentation
{
    /// <summary>
    /// Visual representation of a placed building.
    /// Uses Unity primitives (cubes + cones) until real prefabs exist.
    /// Reads simulation state — never modifies it directly.
    /// Shape creation delegated to BuildingViewFactory.
    /// </summary>
    public class BuildingView : MonoBehaviour
    {
        [SerializeField] private int _buildingId;
        [SerializeField] private MeshRenderer _baseRenderer;
        [SerializeField] private GameObject _constructionOverlay;
        [SerializeField] private ConstructionView _constructionView;

        private MaterialPropertyBlock _propertyBlock;
        private static readonly int ColorProperty = Shader.PropertyToID("_BaseColor");

        /// <summary>The simulation building ID this view represents.</summary>
        public int BuildingId => _buildingId;

        private void Awake()
        {
            _propertyBlock = new MaterialPropertyBlock();
        }

        /// <summary>Initialize with a building ID and world position.</summary>
        public void Initialize(int buildingId, Vector3 worldPosition)
        {
            _buildingId = buildingId;
            transform.position = worldPosition;
        }

        /// <summary>Update visuals based on current building state.</summary>
        public void UpdateState(BuildingState state, float constructionProgress)
        {
            if (_constructionOverlay != null)
                _constructionOverlay.SetActive(state != BuildingState.Complete);

            if (_constructionView != null)
                _constructionView.SetProgress(constructionProgress);

            // Fade in building as construction progresses
            if (_baseRenderer != null && state != BuildingState.Complete)
            {
                _baseRenderer.GetPropertyBlock(_propertyBlock);
                var color = _propertyBlock.GetColor(ColorProperty);
                color.a = Mathf.Lerp(0.3f, 1f, constructionProgress);
                _propertyBlock.SetColor(ColorProperty, color);
                _baseRenderer.SetPropertyBlock(_propertyBlock);
            }
        }

        /// <summary>Set the building color (based on type or ownership).</summary>
        public void SetColor(Color color)
        {
            if (_baseRenderer == null) return;
            _baseRenderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetColor(ColorProperty, color);
            _baseRenderer.SetPropertyBlock(_propertyBlock);
        }

        /// <summary>
        /// Create a procedural building visual with distinct shapes per type.
        /// Delegates shape creation to BuildingViewFactory.
        /// </summary>
        public static BuildingView CreatePrimitive(Transform parent, int buildingId,
            Vector3 worldPosition, BaseBuildingType type, Material material)
        {
            var go = new GameObject($"Building_{buildingId}");
            go.transform.SetParent(parent, false);
            go.transform.position = worldPosition;

            float height = BuildingViewFactory.GetHeight(type);
            float width = BuildingViewFactory.GetWidth(type);
            Color color = BuildingViewFactory.GetBuildingColor(type);
            Color roofColor = BuildingViewFactory.GetRoofColor(type);

            MeshRenderer baseRenderer = BuildingViewFactory.CreateShape(
                go.transform, type, width, height, material, roofColor);

            // Box collider on root for click detection
            var collider = go.AddComponent<BoxCollider>();
            collider.center = new Vector3(0f, height * 0.5f, 0f);
            collider.size = new Vector3(width * 1.2f, height * 1.2f, width * 1.2f);

            // Construction progress bar
            var constructionView = ConstructionView.Create(go.transform, width);

            // BuildingView component
            var view = go.AddComponent<BuildingView>();
            SetPrivateField(view, "_buildingId", buildingId);
            SetPrivateField(view, "_baseRenderer", baseRenderer);
            SetPrivateField(view, "_constructionView", constructionView);

            view.SetColor(color);
            return view;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);
            field?.SetValue(target, value);
        }
    }
}
