using System.Collections.Generic;
using UnityEngine;
using Settlers.Simulation;

namespace Settlers.Presentation
{
    /// <summary>
    /// Manages ClericView instances for active proselytism tasks.
    /// One ClericView per active ProselytismTask (representing the cleric group).
    /// Attached to GameController. Sync once per frame from GameController.Update.
    /// </summary>
    public class ClericManager : MonoBehaviour
    {
        private readonly Dictionary<ProselytismTask, ClericView> _views = new();
        private readonly List<ProselytismTask> _toRemove = new();
        private Transform _root;
        private Material _material;

        public void Initialize(Transform root, Material material)
        {
            _root = root;
            _material = material;
        }

        /// <summary>
        /// Sync cleric views with active proselytism tasks.
        /// Call once per frame from GameController.Update.
        /// </summary>
        public void Sync(IReadOnlyList<ProselytismTask> activeTasks)
        {
            if (_root == null) return;

            // Build active set for O(1) lookup
            var activeSet = new HashSet<ProselytismTask>();
            for (int i = 0; i < activeTasks.Count; i++)
                activeSet.Add(activeTasks[i]);

            // Mark views whose task completed
            _toRemove.Clear();
            foreach (var kvp in _views)
            {
                if (!activeSet.Contains(kvp.Key))
                    _toRemove.Add(kvp.Key);
            }

            for (int i = 0; i < _toRemove.Count; i++)
            {
                if (_views.TryGetValue(_toRemove[i], out var view))
                {
                    if (view != null) Destroy(view.gameObject);
                    _views.Remove(_toRemove[i]);
                }
            }

            // Spawn new views for new tasks; update position for existing ones
            for (int i = 0; i < activeTasks.Count; i++)
            {
                var task = activeTasks[i];
                if (!_views.ContainsKey(task))
                {
                    var view = ClericView.Create(_root, task.SectorId, task.PlayerId, _material);
                    _views[task] = view;
                }
                else
                {
                    _views[task].UpdatePosition(task.Progress);
                }
            }
        }
    }
}
