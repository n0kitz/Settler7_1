using System.Collections.Generic;
using UnityEngine;
using Settlers.Simulation;

namespace Settlers.Presentation
{
    /// <summary>
    /// Manages WorkerView instances, spawning one per operational work yard
    /// and updating their animation each frame.
    /// </summary>
    public class WorkerManager : MonoBehaviour
    {
        private readonly Dictionary<int, WorkerView> _views = new();
        private Transform _root;
        private Material _material;

        public void Initialize(Transform root, Material material)
        {
            _root = root;
            _material = material;
        }

        /// <summary>
        /// Sync worker views with registered work yards.
        /// Call once per frame from GameController.Update.
        /// </summary>
        public void Sync(IReadOnlyList<WorkYard> allWorkYards)
        {
            if (_root == null) return;

            for (int i = 0; i < allWorkYards.Count; i++)
            {
                var wy = allWorkYards[i];

                if (!_views.ContainsKey(wy.Id))
                {
                    // Spawn worker at work yard position
                    var building = GameController.Instance?.Construction?.GetBuilding(wy.BuildingId);
                    if (building == null) continue;

                    var sectorPos = GameController.Instance.GetSectorPosition(wy.SectorId);
                    var wyWorldPos = sectorPos + new Vector3(wy.LocalX, 0f, wy.LocalZ);
                    var view = WorkerView.Create(_root, wy.Id, wyWorldPos, _material);
                    _views[wy.Id] = view;
                }

                // Update animation
                if (_views.TryGetValue(wy.Id, out var workerView))
                    workerView.UpdateFromWorkYard(wy);
            }
        }
    }
}
