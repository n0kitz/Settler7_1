using UnityEngine;
using Settlers.Simulation;

namespace Settlers.Presentation
{
    /// <summary>
    /// Visual representation of a worker at a work yard.
    /// A small humanoid figure (straw hat, brown tunic) walks between the work
    /// yard and a nearby point, simulating the worker's production cycle.
    /// The tunic grays out while the yard is idle.
    /// </summary>
    public class WorkerView : MonoBehaviour
    {
        private int _workYardId;
        private Vector3 _homePos;
        private Vector3 _workPos;
        private MeshRenderer _torso;
        private MaterialPropertyBlock _propBlock;
        private Vector3 _lastPos;
        private bool _idleTint = true;

        private static readonly int ColorProp = Shader.PropertyToID("_BaseColor");
        private static readonly Color IDLE_TUNIC = new(0.5f, 0.5f, 0.5f);
        private static readonly Color ACTIVE_TUNIC = new(0.55f, 0.38f, 0.20f);

        public int WorkYardId => _workYardId;

        /// <summary>
        /// Create a worker visual at the given work yard position.
        /// </summary>
        public static WorkerView Create(Transform parent, int workYardId,
            Vector3 workYardWorldPos, Material material)
        {
            var go = new GameObject($"Worker_{workYardId}");
            go.transform.SetParent(parent, false);

            var figure = UnitFigureFactory.CreateFigure(go.transform,
                UnitFigureFactory.Role.Worker, ACTIVE_TUNIC, material);

            var view = go.AddComponent<WorkerView>();
            view._workYardId = workYardId;
            view._homePos = workYardWorldPos + new Vector3(0.8f, 0.02f, 0f);
            view._workPos = workYardWorldPos + new Vector3(-0.8f, 0.02f, 0f);
            view._torso = UnitFigureFactory.GetTorsoRenderer(figure);
            view._propBlock = new MaterialPropertyBlock();
            view.SetTunicColor(IDLE_TUNIC);

            go.transform.position = view._homePos;
            view._lastPos = view._homePos;
            return view;
        }

        /// <summary>
        /// Update the worker visual based on work yard state.
        /// </summary>
        public void UpdateFromWorkYard(WorkYard wy)
        {
            if (wy == null || !wy.IsOperational)
            {
                // Tint only on state CHANGE — Get/SetPropertyBlock every frame
                // for hundreds of workers was the 60-fps killer (Sprint 8a)
                if (!_idleTint) { SetTunicColor(IDLE_TUNIC); _idleTint = true; }
                transform.position = _homePos;
                _lastPos = _homePos;
                return;
            }

            if (_idleTint) { SetTunicColor(ACTIVE_TUNIC); _idleTint = false; }

            // Walk back and forth based on cycle progress
            float cyclePhase = wy.CycleProgress * 2f; // 0→2 over full cycle
            Vector3 pos = cyclePhase <= 1f
                ? Vector3.Lerp(_homePos, _workPos, cyclePhase)
                : Vector3.Lerp(_workPos, _homePos, cyclePhase - 1f);

            // Face the walking direction
            var delta = pos - _lastPos;
            delta.y = 0f;
            if (delta.sqrMagnitude > 0.000001f)
                transform.rotation = Quaternion.LookRotation(delta);
            _lastPos = pos;

            // Walk bob
            pos.y += Mathf.Abs(Mathf.Sin(Time.time * 6f)) * 0.03f;
            transform.position = pos;
        }

        private void SetTunicColor(Color color)
        {
            if (_torso == null) return;
            // Cached material swap instead of MPB — keeps the torso SRP-batched
            _torso.sharedMaterial =
                BuildingViewFactory.GetColorMaterial(_torso.sharedMaterial, color);
        }
    }
}
