using System.Collections.Generic;
using UnityEngine;
using Settlers.Simulation;

namespace Settlers.Presentation
{
    /// <summary>
    /// Partial: §14.10 dressing around the sector hexes — home castles and
    /// strongholds, the world ground plane, and the road network views.
    /// Sector meshes/materials live in GameController.SectorVisuals.cs.
    /// </summary>
    public partial class GameController
    {
        /// <summary>Each player's home sector is the lowest-id sector they own at
        /// spawn — that sector gets the dominating home castle (§14.10).</summary>
        private HashSet<int> ComputeHomeSectors(int count)
        {
            var seen = new HashSet<int>();
            var homes = new HashSet<int>();
            for (int i = 0; i < count; i++)
            {
                int owner = Graph.GetSector(i).OwnerId;
                if (owner >= 0 && seen.Add(owner)) homes.Add(i);
            }
            return homes;
        }

        // Matches SectorView's ownership palette (Blue/Red/Green/Yellow)
        private static readonly Color[] LandmarkPlayerColors =
        {
            new(0.2f, 0.5f, 0.9f), new(0.9f, 0.2f, 0.2f),
            new(0.2f, 0.8f, 0.3f), new(0.9f, 0.8f, 0.1f),
        };

        /// <summary>Give home sectors a player castle and guarded neutral
        /// sectors a fortified stronghold (§14.10). Built once at spawn.</summary>
        private void AttachLandmark(GameObject go, Sector sector, bool isHome)
        {
            if (isHome && sector.OwnerId >= 0)
            {
                var color = sector.OwnerId < LandmarkPlayerColors.Length
                    ? LandmarkPlayerColors[sector.OwnerId]
                    : Color.white;
                go.AddComponent<SectorLandmarkView>().BuildHomeCastle(_sectorRadius, color);
            }
            else if (sector.IsNeutral && sector.IsFortified)
            {
                go.AddComponent<SectorLandmarkView>().BuildStronghold(_sectorRadius);
            }
        }

        /// <summary>Muted ground plane under the whole map so sectors don't
        /// float as islands over the sky background (§14.10).</summary>
        private void CreateWorldGround(Transform parent)
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "WorldGround";
            ground.transform.SetParent(parent, false);
            ground.transform.localPosition = new Vector3(0f, -0.08f, 0f);
            ground.transform.localScale = new Vector3(60f, 1f, 60f);
            // No collider — clicks must reach the sector hexes only
            Destroy(ground.GetComponent<Collider>());
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"))
            {
                name = "WorldGround",
                color = new Color(0.45f, 0.46f, 0.30f),
            };
            ground.GetComponent<MeshRenderer>().sharedMaterial = mat;
        }

        private List<RoadView> _roadViews;

        private void SpawnRoads()
        {
            _roadViews = new List<RoadView>();
            var root = new GameObject("Roads");
            root.transform.SetParent(transform);
            // Lit so roads receive sun shadows and fog like the terrain
            var roadMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            roadMaterial.name = "RoadDefault";

            var drawn = new HashSet<long>();
            for (int i = 0; i < Graph.SectorCount; i++)
            {
                foreach (int n in Graph.GetNeighbors(i))
                {
                    long key = System.Math.Min(i, n) * 1000L + System.Math.Max(i, n);
                    if (!drawn.Add(key)) continue;

                    var go = new GameObject($"Road_{i}_{n}");
                    go.transform.SetParent(root.transform);
                    var rv = go.AddComponent<RoadView>();
                    rv.Initialize(i, n, GetSectorPosition(i), GetSectorPosition(n), roadMaterial);
                    _roadViews.Add(rv);
                }
            }
        }

        /// <summary>Sync all road visuals with simulation paved state.</summary>
        public void SyncRoads()
        {
            if (_roadViews == null || State?.Logistics == null) return;
            for (int i = 0; i < _roadViews.Count; i++)
                _roadViews[i].SyncFromSimulation(State.Logistics);
        }
    }
}
