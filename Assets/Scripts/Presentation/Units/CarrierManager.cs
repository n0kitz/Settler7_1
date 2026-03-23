using System.Collections.Generic;
using UnityEngine;
using Settlers.Simulation;

namespace Settlers.Presentation
{
    /// <summary>
    /// Manages CarrierView instances, spawning new ones for active tasks
    /// and destroying completed ones. Attached to GameController.
    /// </summary>
    public class CarrierManager : MonoBehaviour
    {
        private readonly Dictionary<CarrierTask, CarrierView> _views = new();
        private readonly List<CarrierTask> _toRemove = new();
        private Transform _root;
        private Material _material;

        public void Initialize(Transform root, Material material)
        {
            _root = root;
            _material = material;
        }

        /// <summary>
        /// Sync carrier views with active logistics tasks.
        /// Call once per frame from GameController.Update.
        /// </summary>
        public void Sync(IReadOnlyList<CarrierTask> activeTasks)
        {
            if (_root == null) return;

            // Build set of active tasks for O(1) lookup
            var activeSet = new HashSet<CarrierTask>();
            for (int i = 0; i < activeTasks.Count; i++)
                activeSet.Add(activeTasks[i]);

            // Mark existing views that no longer have a matching task
            _toRemove.Clear();
            foreach (var kvp in _views)
            {
                if (!activeSet.Contains(kvp.Key))
                    _toRemove.Add(kvp.Key);
            }

            // Remove completed
            for (int i = 0; i < _toRemove.Count; i++)
            {
                if (_views.TryGetValue(_toRemove[i], out var view))
                {
                    if (view != null)
                        Destroy(view.gameObject);
                    _views.Remove(_toRemove[i]);
                }
            }

            // Spawn new views for tasks that don't have one
            for (int i = 0; i < activeTasks.Count; i++)
            {
                var task = activeTasks[i];
                if (!_views.ContainsKey(task))
                {
                    var view = CarrierView.Create(_root, task, _material);
                    _views[task] = view;
                }
            }
        }
    }
}
