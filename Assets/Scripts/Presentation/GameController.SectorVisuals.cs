using System.Collections.Generic;
using UnityEngine;
using Settlers.Simulation;

namespace Settlers.Presentation
{
    /// <summary>
    /// Partial: sector spawning, hex mesh generation, edge lines, materials.
    /// </summary>
    public partial class GameController
    {
        /// <summary>Cached sector positions computed once at spawn time.</summary>
        private Vector3[] _sectorPositions;

        private void ComputeSectorPositions()
        {
            int count = Graph.SectorCount;
            _sectorPositions = new Vector3[count];

            if (count <= 6)
            {
                float halfSpace = _sectorSpacing * 0.5f;
                if (count > 0) _sectorPositions[0] = new Vector3(-halfSpace, 0f, _sectorSpacing);
                if (count > 1) _sectorPositions[1] = new Vector3(halfSpace, 0f, _sectorSpacing);
                if (count > 2) _sectorPositions[2] = new Vector3(-_sectorSpacing, 0f, 0f);
                if (count > 3) _sectorPositions[3] = new Vector3(_sectorSpacing, 0f, 0f);
                if (count > 4) _sectorPositions[4] = new Vector3(-halfSpace, 0f, -_sectorSpacing);
                if (count > 5) _sectorPositions[5] = new Vector3(halfSpace, 0f, -_sectorSpacing);
            }
            else
            {
                _sectorPositions[0] = Vector3.zero;
                int ring = 1;
                int placed = 1;
                while (placed < count)
                {
                    int ringSize = ring * 6;
                    float ringRadius = ring * _sectorSpacing;
                    for (int i = 0; i < ringSize && placed < count; i++)
                    {
                        float angle = (2f * Mathf.PI * i) / ringSize;
                        _sectorPositions[placed] = new Vector3(
                            Mathf.Cos(angle) * ringRadius, 0f,
                            Mathf.Sin(angle) * ringRadius);
                        placed++;
                    }
                    ring++;
                }
            }
        }

        /// <summary>Get the world position of a sector by ID.</summary>
        public Vector3 GetSectorPosition(int sectorId)
        {
            if (_sectorPositions != null && sectorId >= 0 && sectorId < _sectorPositions.Length)
                return _sectorPositions[sectorId];
            return Vector3.zero;
        }

        /// <summary>Get the SectorView for a given sector ID.</summary>
        public SectorView GetSectorView(int sectorId)
        {
            if (sectorId < 0 || sectorId >= _sectorViews.Length) return null;
            return _sectorViews[sectorId];
        }

        /// <summary>Push current ownership to all SectorViews.</summary>
        public void RefreshAllOwnership()
        {
            for (int i = 0; i < _sectorViews.Length; i++)
            {
                var sector = Graph.GetSector(i);
                _sectorViews[i].UpdateOwnership(sector.OwnerId);
                _sectorViews[i].UpdateBorderColor(sector.OwnerId);
                _sectorViews[i].GetComponent<SectorWallView>()
                    ?.SetVisible(sector.IsPlayerOwned);
            }
        }

        private void SpawnSectors()
        {
            var mapRoot = new GameObject("MapRoot");
            mapRoot.transform.SetParent(transform);
            int count = Graph.SectorCount;
            _sectorViews = new SectorView[count];
            for (int i = 0; i < count; i++)
            {
                var sector = Graph.GetSector(i);
                var go = CreateSectorGameObject(sector, mapRoot.transform);
                var view = go.GetComponent<SectorView>();
                view.Initialize(i, GetSectorPosition(i));
                _sectorViews[i] = view;
            }
        }

        private GameObject CreateSectorGameObject(Sector sector, Transform parent)
        {
            var go = new GameObject($"Sector_{sector.Id}_{sector.Name}");
            go.transform.SetParent(parent);

            var terrainChild = CreateHexMesh(go.transform, "Terrain", _sectorRadius);
            var meshRenderer = terrainChild.GetComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = _sectorMaterial;

            var highlightChild = CreateHexMesh(go.transform, "SelectionHighlight", _sectorRadius * 1.05f);
            highlightChild.transform.localPosition = new Vector3(0f, 0.01f, 0f);
            var hlRenderer = highlightChild.GetComponent<MeshRenderer>();
            hlRenderer.sharedMaterial = CreateHighlightMaterial();
            highlightChild.SetActive(false);

            var borderLr = go.AddComponent<LineRenderer>();
            ConfigureBorderRenderer(borderLr);
            var borderPoints = GenerateHexBorderPoints(_sectorRadius, _borderHeight);

            var view = go.AddComponent<SectorView>();
            SetPrivateField(view, "_terrainRenderer", meshRenderer);
            SetPrivateField(view, "_borderRenderer", borderLr);
            SetPrivateField(view, "_selectionHighlight", highlightChild);
            view.SetBorderPoints(borderPoints);

            // Stone wall ring (§14.10) — shown only while the sector is owned
            var wall = go.AddComponent<SectorWallView>();
            wall.Build(_sectorRadius);

            var collider = go.AddComponent<MeshCollider>();
            collider.sharedMesh = terrainChild.GetComponent<MeshFilter>().sharedMesh;
            return go;
        }

        private GameObject CreateHexMesh(Transform parent, string name, float radius)
        {
            var child = new GameObject(name);
            child.transform.SetParent(parent, false);
            child.AddComponent<MeshFilter>().sharedMesh = GenerateHexMesh(radius);
            child.AddComponent<MeshRenderer>();
            return child;
        }

        private Mesh GenerateHexMesh(float radius)
        {
            const int S = 6;
            var verts = new Vector3[S + 1];
            var tris = new int[S * 3];
            var norms = new Vector3[S + 1];
            verts[0] = Vector3.zero; norms[0] = Vector3.up;
            for (int i = 0; i < S; i++)
            {
                float a = Mathf.Deg2Rad * (60f * i - 30f);
                verts[i + 1] = new Vector3(Mathf.Cos(a) * radius, 0f, Mathf.Sin(a) * radius);
                norms[i + 1] = Vector3.up;
                tris[i * 3] = 0; tris[i * 3 + 1] = i + 1; tris[i * 3 + 2] = (i + 1) % S + 1;
            }
            var m = new Mesh { name = $"Hex_{radius:F1}", vertices = verts, triangles = tris, normals = norms };
            m.RecalculateBounds();
            return m;
        }

        private Vector3[] GenerateHexBorderPoints(float radius, float height)
        {
            var pts = new Vector3[6];
            for (int i = 0; i < 6; i++)
            {
                float a = Mathf.Deg2Rad * (60f * i - 30f);
                pts[i] = new Vector3(Mathf.Cos(a) * radius, height, Mathf.Sin(a) * radius);
            }
            return pts;
        }

        private void ConfigureBorderRenderer(LineRenderer lr)
        {
            lr.useWorldSpace = false;
            lr.startWidth = _borderWidth; lr.endWidth = _borderWidth;
            lr.loop = false;
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.receiveShadows = false;
            lr.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        }

        private List<RoadView> _roadViews;

        private void SpawnRoads()
        {
            _roadViews = new List<RoadView>();
            var root = new GameObject("Roads");
            root.transform.SetParent(transform);
            var roadMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
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

        private void EnsureMaterial()
        {
            if (_sectorMaterial != null) return;
            _sectorMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            _sectorMaterial.name = "SectorDefault";
        }

        private void EnsureBuildingMaterial()
        {
            _buildingMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            _buildingMaterial.name = "BuildingDefault";
        }

        private Material CreateHighlightMaterial()
        {
            var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            mat.name = "SectorHighlight";
            mat.color = new Color(1f, 1f, 1f, 0.3f);
            mat.SetFloat("_Surface", 1f); mat.SetFloat("_Blend", 0f);
            mat.renderQueue = 3000;
            return mat;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(target, value);
        }
    }
}
