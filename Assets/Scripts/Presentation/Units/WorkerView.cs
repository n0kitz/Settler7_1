using UnityEngine;
using Settlers.Simulation;

namespace Settlers.Presentation
{
    /// <summary>
    /// Visual representation of a worker at a work yard.
    /// Shows a small capsule walking between the work yard and a nearby point,
    /// simulating the worker's production cycle.
    /// </summary>
    public class WorkerView : MonoBehaviour
    {
        private int _workYardId;
        private Vector3 _homePos;
        private Vector3 _workPos;
        private MeshRenderer _renderer;
        private MaterialPropertyBlock _propBlock;

        private static readonly int ColorProp = Shader.PropertyToID("_BaseColor");
        private static readonly Color IDLE_COLOR = new Color(0.5f, 0.5f, 0.5f);
        private static readonly Color ACTIVE_COLOR = new Color(0.2f, 0.7f, 0.3f);

        public int WorkYardId => _workYardId;

        /// <summary>
        /// Create a worker visual at the given work yard position.
        /// </summary>
        public static WorkerView Create(Transform parent, int workYardId,
            Vector3 workYardWorldPos, Material material)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = $"Worker_{workYardId}";
            go.transform.SetParent(parent, false);
            go.transform.localScale = new Vector3(0.3f, 0.5f, 0.3f);

            // Remove default collider (workers aren't clickable)
            var collider = go.GetComponent<Collider>();
            if (collider != null) Object.Destroy(collider);

            var view = go.AddComponent<WorkerView>();
            view._workYardId = workYardId;
            view._homePos = workYardWorldPos + new Vector3(0.8f, 0.25f, 0f);
            view._workPos = workYardWorldPos + new Vector3(-0.8f, 0.25f, 0f);
            view._renderer = go.GetComponent<MeshRenderer>();
            if (material != null)
                view._renderer.sharedMaterial = material;
            view._propBlock = new MaterialPropertyBlock();
            view.SetColor(IDLE_COLOR);

            go.transform.position = view._homePos;
            return view;
        }

        /// <summary>
        /// Update the worker visual based on work yard state.
        /// </summary>
        public void UpdateFromWorkYard(WorkYard wy)
        {
            if (wy == null || !wy.IsOperational)
            {
                SetColor(IDLE_COLOR);
                transform.position = _homePos;
                return;
            }

            SetColor(ACTIVE_COLOR);

            // Animate back and forth based on cycle progress
            float cyclePhase = wy.CycleProgress * 2f; // 0→2 over full cycle
            if (cyclePhase <= 1f)
            {
                // Walking to work position
                transform.position = Vector3.Lerp(_homePos, _workPos, cyclePhase);
            }
            else
            {
                // Walking back
                transform.position = Vector3.Lerp(_workPos, _homePos, cyclePhase - 1f);
            }
        }

        private void SetColor(Color color)
        {
            if (_renderer == null) return;
            _renderer.GetPropertyBlock(_propBlock);
            _propBlock.SetColor(ColorProp, color);
            _renderer.SetPropertyBlock(_propBlock);
        }
    }
}
