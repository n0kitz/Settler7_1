using UnityEngine;
using Settlers.Simulation;

namespace Settlers.Presentation
{
    /// <summary>
    /// Visual representation of a carrier transporting goods between storehouses.
    /// A small humanoid figure (blue tunic, cap) walks the route facing its travel
    /// direction, carrying a crate tinted by the resource type.
    /// </summary>
    public class CarrierView : MonoBehaviour
    {
        private static readonly Color CARRIER_TUNIC = new(0.25f, 0.42f, 0.70f);

        private CarrierTask _task;
        private Vector3 _startPos;
        private Vector3 _endPos;

        /// <summary>The carrier task this view tracks (null if recycled).</summary>
        public CarrierTask Task => _task;

        /// <summary>
        /// Create a carrier visual for a given task.
        /// </summary>
        public static CarrierView Create(Transform parent, CarrierTask task, Material material)
        {
            var go = new GameObject($"Carrier_{task.FromSectorId}to{task.ToSectorId}");
            go.transform.SetParent(parent, false);

            var figure = UnitFigureFactory.CreateFigure(go.transform,
                UnitFigureFactory.Role.Carrier, CARRIER_TUNIC, material);

            var hands = UnitFigureFactory.GetHandsAnchor(figure);
            var crate = BuildingViewFactory.CreatePrim(hands, "Crate",
                PrimitiveType.Cube, new Vector3(0.18f, 0.14f, 0.14f),
                Vector3.zero, material);
            BuildingViewFactory.SetColor(crate, GetResourceColor(task.ResourceType));

            var view = go.AddComponent<CarrierView>();
            view._task = task;

            var gc = GameController.Instance;
            if (gc != null)
            {
                view._startPos = gc.GetSectorPosition(task.FromSectorId) + Vector3.up * 0.02f;
                view._endPos = gc.GetSectorPosition(task.ToSectorId) + Vector3.up * 0.02f;
            }

            go.transform.position = view._startPos;

            // Face the travel direction once — routes are straight lines.
            var dir = view._endPos - view._startPos;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.0001f)
                go.transform.rotation = Quaternion.LookRotation(dir);

            return view;
        }

        private void Update()
        {
            if (_task == null) return;

            // Interpolate between start and end based on task progress
            var pos = Vector3.Lerp(_startPos, _endPos, _task.Progress);

            // Walk bob
            pos.y += Mathf.Abs(Mathf.Sin(Time.time * 6f)) * 0.04f;
            transform.position = pos;
        }

        /// <summary>Mark this carrier view as completed (ready for cleanup).</summary>
        public void MarkCompleted()
        {
            _task = null;
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
