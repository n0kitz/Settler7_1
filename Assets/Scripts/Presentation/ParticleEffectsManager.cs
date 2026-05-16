using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Settlers.Simulation;

namespace Settlers.Presentation
{
    /// <summary>
    /// Procedural particle bursts driven by simulation events.
    /// Spawns small colored spheres that scale down and fade, using a reuse pool.
    /// </summary>
    public class ParticleEffectsManager : MonoBehaviour
    {
        private const int PARTICLES_PER_BURST = 8;
        private const int POOL_SIZE = 48;

        private readonly List<GameObject> _pool = new List<GameObject>();
        private Material _particleMat;

        private static readonly Color COLOR_BUILD   = new Color(0.4f, 0.9f, 0.5f);
        private static readonly Color COLOR_CONQUER = new Color(0.9f, 0.5f, 0.2f);
        private static readonly Color COLOR_VP      = new Color(0.9f, 0.8f, 0.2f);

        private void Awake()
        {
            _particleMat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));

            for (int i = 0; i < POOL_SIZE; i++)
            {
                var p = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                p.transform.SetParent(transform);
                p.transform.localScale = Vector3.one * 0.15f;
                Destroy(p.GetComponent<Collider>());
                p.GetComponent<Renderer>().material = _particleMat;
                p.SetActive(false);
                _pool.Add(p);
            }
        }

        /// <summary>Subscribe to EventBus events when the game state is ready.</summary>
        public void Initialize(EventBus bus, GameController gc)
        {
            bus.Subscribe<BuildingCompletedEvent>(e =>
            {
                var pos = gc.GetSectorPosition(
                    gc.State.Construction.GetBuilding(e.BuildingId)?.SectorId ?? 0);
                Burst(pos, COLOR_BUILD);
            });
            bus.Subscribe<SectorConqueredEvent>(e =>
                Burst(gc.GetSectorPosition(e.SectorId), COLOR_CONQUER));
            bus.Subscribe<VPChangedEvent>(e =>
            {
                if (!e.Gained) return;
                var sectors = gc.State.Graph.GetSectorsOwnedBy(e.PlayerId);
                if (sectors.Count > 0)
                    Burst(gc.GetSectorPosition(sectors[0].Id), COLOR_VP);
            });
        }

        public void Burst(Vector3 origin, Color color)
        {
            int spawned = 0;
            foreach (var p in _pool)
            {
                if (p.activeSelf) continue;
                if (spawned >= PARTICLES_PER_BURST) break;
                var dir = Random.insideUnitSphere.normalized;
                dir.y = Mathf.Abs(dir.y) + 0.2f;
                p.transform.position = origin + Vector3.up * 0.3f;
                p.GetComponent<Renderer>().material.color = color;
                p.SetActive(true);
                StartCoroutine(AnimateParticle(p, dir));
                spawned++;
            }
        }

        private IEnumerator AnimateParticle(GameObject p, Vector3 dir)
        {
            float lifetime = Random.Range(0.5f, 0.9f);
            float speed    = Random.Range(2f, 4f);
            float elapsed  = 0f;
            var   mat      = p.GetComponent<Renderer>().material;
            var   baseCol  = mat.color;
            float startScale = Random.Range(0.12f, 0.2f);

            while (elapsed < lifetime)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / lifetime;
                p.transform.position += dir * (speed * Time.deltaTime * (1f - t));
                float scale = startScale * (1f - t);
                p.transform.localScale = Vector3.one * scale;
                mat.color = new Color(baseCol.r, baseCol.g, baseCol.b, 1f - t);
                yield return null;
            }
            p.SetActive(false);
        }

        public static ParticleEffectsManager Create(Transform root)
        {
            var go = new GameObject("ParticleEffectsManager");
            go.transform.SetParent(root);
            return go.AddComponent<ParticleEffectsManager>();
        }
    }
}
