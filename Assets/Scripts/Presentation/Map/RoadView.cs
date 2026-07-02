using UnityEngine;
using Settlers.Simulation;

namespace Settlers.Presentation
{
    /// <summary>
    /// Visual representation of a road (edge) between two sectors.
    /// Renders as a flat quad strip. Updates color when paved.
    /// </summary>
    public class RoadView : MonoBehaviour
    {
        private int _sectorA;
        private int _sectorB;
        private MeshRenderer _renderer;
        private bool _isPaved;

        private static readonly Color UnpavedColor = new Color(0.35f, 0.3f, 0.22f, 0.7f);
        private static readonly Color PavedColor = new Color(0.55f, 0.52f, 0.45f, 1f);
        private const float RoadWidth = 0.6f;
        private const float RoadY = 0.03f;

        /// <summary>Initialize the road between two sector positions.</summary>
        public void Initialize(int sectorA, int sectorB, Vector3 posA, Vector3 posB, Material baseMaterial)
        {
            _sectorA = sectorA;
            _sectorB = sectorB;

            var mesh = CreateRoadMesh(posA, posB);
            var mf = gameObject.AddComponent<MeshFilter>();
            mf.sharedMesh = mesh;

            _renderer = gameObject.AddComponent<MeshRenderer>();
            _renderer.material = new Material(baseMaterial);
            _renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _renderer.receiveShadows = false;

            SetPaved(false);
        }

        /// <summary>Update paved state and color.</summary>
        public void SetPaved(bool paved)
        {
            _isPaved = paved;
            if (_renderer != null)
                _renderer.material.color = paved ? PavedColor : UnpavedColor;
        }

        public bool IsPaved => _isPaved;
        public int SectorA => _sectorA;
        public int SectorB => _sectorB;

        /// <summary>Check simulation state and update visual if paved status changed.</summary>
        public void SyncFromSimulation(LogisticsSystem logistics)
        {
            if (logistics == null) return;
            bool paved = logistics.IsPaved(_sectorA, _sectorB);
            if (paved != _isPaved)
                SetPaved(paved);
        }

        private static Mesh CreateRoadMesh(Vector3 posA, Vector3 posB)
        {
            // Flat quad strip from A to B
            Vector3 a = new Vector3(posA.x, RoadY, posA.z);
            Vector3 b = new Vector3(posB.x, RoadY, posB.z);

            Vector3 dir = (b - a).normalized;
            Vector3 perp = new Vector3(-dir.z, 0f, dir.x) * (RoadWidth * 0.5f);

            // Inset slightly from sector centers so the road doesn't overlap the hex
            Vector3 inset = dir * 1.5f;
            a += inset;
            b -= inset;

            var verts = new Vector3[]
            {
                a - perp, a + perp,
                b - perp, b + perp
            };

            // Clockwise seen from above so the quad faces up (+Y) and isn't backface-culled
            var tris = new int[] { 0, 1, 2, 2, 1, 3 };

            float len = (b - a).magnitude;
            var uvs = new Vector2[]
            {
                new(0f, 0f), new(1f, 0f),
                new(0f, len / RoadWidth), new(1f, len / RoadWidth)
            };

            var mesh = new Mesh
            {
                name = "RoadMesh",
                vertices = verts,
                triangles = tris,
                uv = uvs
            };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
