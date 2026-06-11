using UnityEngine;
using System.Collections.Generic;
using Settlers.Simulation;

namespace Settlers.Presentation
{
    /// <summary>
    /// Spawns and pools FloatingTextItem instances above world positions.
    /// Subscribes to simulation events and shows contextual labels.
    /// </summary>
    public class FloatingTextManager : MonoBehaviour
    {
        private const int POOL_SIZE = 16;
        private readonly List<FloatingTextItem> _pool = new List<FloatingTextItem>();

        private static readonly Color COLOR_VP      = new Color(0.9f, 0.8f, 0.2f);
        private static readonly Color COLOR_CONQUER = new Color(0.9f, 0.4f, 0.2f);
        private static readonly Color COLOR_BUILD   = new Color(0.4f, 0.9f, 0.5f);
        private static readonly Color COLOR_COIN    = new Color(0.7f, 0.9f, 0.3f);

        private void Awake()
        {
            for (int i = 0; i < POOL_SIZE; i++)
            {
                var go = new GameObject($"FloatText_{i}");
                go.transform.SetParent(transform);
                var item = go.AddComponent<FloatingTextItem>();
                go.SetActive(false);
                _pool.Add(item);
            }
        }

        /// <summary>Subscribe to EventBus events after the game state is ready.</summary>
        public void Initialize(EventBus bus, GameController gc)
        {
            bus.Subscribe<BuildingCompletedEvent>(e =>
            {
                var pos = gc.GetSectorPosition(gc.State.Construction.GetBuilding(e.BuildingId)?.SectorId ?? 0);
                Spawn(pos, "Built!", COLOR_BUILD);
            });
            bus.Subscribe<SectorConqueredEvent>(e =>
            {
                var pos = gc.GetSectorPosition(e.SectorId);
                string text = e.NewOwnerId == 0 ? "Conquered!" : "Lost!";
                Spawn(pos, text, COLOR_CONQUER);
            });
            bus.Subscribe<VPChangedEvent>(e =>
            {
                if (!e.Gained) return;
                // Show near a player-owned sector
                var sectors = gc.State.Graph.GetSectorsOwnedBy(e.PlayerId);
                if (sectors.Count > 0)
                    Spawn(gc.GetSectorPosition(sectors[0]), "+1 VP", COLOR_VP);
            });
        }

        public void Spawn(Vector3 worldPos, string text, Color color)
        {
            var item = GetFree();
            if (item != null)
                item.Play(worldPos, text, color);
        }

        private FloatingTextItem GetFree()
        {
            foreach (var item in _pool)
                if (!item.gameObject.activeSelf) return item;
            return null; // pool exhausted (silently skip)
        }

        public static FloatingTextManager Create(Transform root)
        {
            var go = new GameObject("FloatingTextManager");
            go.transform.SetParent(root);
            return go.AddComponent<FloatingTextManager>();
        }
    }
}
