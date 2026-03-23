using UnityEngine;
using Settlers.Simulation;

namespace Settlers.Presentation
{
    /// <summary>
    /// Visual representation of a carrier transporting goods between storehouses.
    /// Shows a small cube moving along the route based on CarrierTask progress.
    /// </summary>
    public class CarrierView : MonoBehaviour
    {
        private CarrierTask _task;
        private Vector3 _startPos;
        private Vector3 _endPos;
        private MeshRenderer _renderer;
        private MaterialPropertyBlock _propBlock;

        private static readonly int ColorProp = Shader.PropertyToID("_BaseColor");

        /// <summary>The carrier task this view tracks (null if recycled).</summary>
        public CarrierTask Task => _task;

        /// <summary>
        /// Create a carrier visual for a given task.
        /// </summary>
        public static CarrierView Create(Transform parent, CarrierTask task, Material material)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = $"Carrier_{task.FromSectorId}to{task.ToSectorId}";
            go.transform.SetParent(parent, false);
            go.transform.localScale = new Vector3(0.25f, 0.35f, 0.25f);

            // Remove default collider
            var collider = go.GetComponent<Collider>();
            if (collider != null) Object.Destroy(collider);

            var view = go.AddComponent<CarrierView>();
            view._task = task;
            view._renderer = go.GetComponent<MeshRenderer>();
            if (material != null)
                view._renderer.sharedMaterial = material;
            view._propBlock = new MaterialPropertyBlock();

            // Get positions from GameController
            var gc = GameController.Instance;
            if (gc != null)
            {
                view._startPos = gc.GetSectorPosition(task.FromSectorId) + Vector3.up * 0.3f;
                view._endPos = gc.GetSectorPosition(task.ToSectorId) + Vector3.up * 0.3f;
            }

            // Color based on resource type
            view.SetColor(GetResourceColor(task.ResourceType));
            go.transform.position = view._startPos;

            return view;
        }

        private void Update()
        {
            if (_task == null) return;

            // Interpolate between start and end based on task progress
            transform.position = Vector3.Lerp(_startPos, _endPos, _task.Progress);

            // Small bob animation
            float bob = Mathf.Sin(Time.time * 4f) * 0.05f;
            transform.position += Vector3.up * bob;
        }

        /// <summary>Mark this carrier view as completed (ready for cleanup).</summary>
        public void MarkCompleted()
        {
            _task = null;
        }

        private void SetColor(Color color)
        {
            if (_renderer == null) return;
            _renderer.GetPropertyBlock(_propBlock);
            _propBlock.SetColor(ColorProp, color);
            _renderer.SetPropertyBlock(_propBlock);
        }

        private static Color GetResourceColor(ResourceType type)
        {
            return type switch
            {
                ResourceType.Wood => new Color(0.55f, 0.35f, 0.15f),
                ResourceType.Planks => new Color(0.75f, 0.55f, 0.25f),
                ResourceType.Stone => new Color(0.6f, 0.6f, 0.6f),
                ResourceType.IronOre => new Color(0.4f, 0.3f, 0.3f),
                ResourceType.IronBars => new Color(0.7f, 0.7f, 0.75f),
                ResourceType.GoldOre => new Color(0.8f, 0.65f, 0.2f),
                ResourceType.Coal => new Color(0.2f, 0.2f, 0.2f),
                ResourceType.Grain => new Color(0.9f, 0.8f, 0.3f),
                ResourceType.Bread => new Color(0.85f, 0.65f, 0.3f),
                ResourceType.Fish => new Color(0.3f, 0.5f, 0.8f),
                ResourceType.Sausages => new Color(0.7f, 0.3f, 0.3f),
                ResourceType.Coins => new Color(0.9f, 0.85f, 0.2f),
                ResourceType.Tools => new Color(0.5f, 0.5f, 0.55f),
                ResourceType.Weapons => new Color(0.45f, 0.45f, 0.5f),
                _ => new Color(0.6f, 0.6f, 0.6f)
            };
        }
    }
}
