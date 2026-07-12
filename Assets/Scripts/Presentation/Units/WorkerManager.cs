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
        private readonly HashSet<int> _aliveIds = new();
        private readonly List<int> _staleIds = new();
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

            // Remove views whose yards were unregistered (destroyed buildings,
            // conquests) — without this, long wars accumulate hundreds of
            // zombie worker figures and frame rate collapses.
            _aliveIds.Clear();
            for (int i = 0; i < allWorkYards.Count; i++)
                _aliveIds.Add(allWorkYards[i].Id);
            _staleIds.Clear();
            foreach (var kvp in _views)
                if (!_aliveIds.Contains(kvp.Key)) _staleIds.Add(kvp.Key);
            for (int i = 0; i < _staleIds.Count; i++)
            {
                if (_views[_staleIds[i]] != null)
                    Destroy(_views[_staleIds[i]].gameObject);
                _views.Remove(_staleIds[i]);
            }

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
