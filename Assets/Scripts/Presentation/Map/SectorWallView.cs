using UnityEngine;

namespace Settlers.Presentation
{
    /// <summary>
    /// Physical stone wall around an owned sector (§14.10) — wall segments
    /// with corner towers along the hex perimeter instead of an abstract
    /// line. Hidden for neutral/unowned sectors.
    /// </summary>
    public class SectorWallView : MonoBehaviour
    {
        private GameObject _root;
        private static Material _wallMaterial;

        // Light weathered stone, like the original's rough field walls
        private static readonly Color STONE = new(0.70f, 0.68f, 0.62f);

        /// <summary>Build the wall ring once (starts hidden).</summary>
        public void Build(float radius)
        {
            if (_root != null) return;
            _root = new GameObject("Walls");
            _root.transform.SetParent(transform, false);

            EnsureMaterial();

            float wallHeight = radius * 0.07f;
            float stoneLen   = radius * 0.11f;

            // Deterministic jitter per sector so walls don't change on reload
            var rng = new System.Random(
                (int)(transform.position.x * 73f) ^ (int)(transform.position.z * 131f));

            var corners = new Vector3[6];
            for (int i = 0; i < 6; i++)
            {
                float a = Mathf.Deg2Rad * (60f * i - 30f);
                corners[i] = new Vector3(Mathf.Cos(a) * radius, 0f, Mathf.Sin(a) * radius);
            }

            for (int i = 0; i < 6; i++)
            {
                Vector3 a = corners[i];
                Vector3 b = corners[(i + 1) % 6];
                BuildStoneRow(i, a, b, wallHeight, stoneLen, rng);
            }

            _root.SetActive(false);
        }

        /// <summary>Row of irregular stone blocks along one edge — reads as a
        /// hand-stacked field wall rather than a uniform barrier.</summary>
        private void BuildStoneRow(int edge, Vector3 a, Vector3 b,
            float height, float stoneLen, System.Random rng)
        {
            Vector3 dir = (b - a).normalized;
            Vector3 perp = new Vector3(-dir.z, 0f, dir.x);
            float length = (b - a).magnitude * 0.9f;
            int count = Mathf.Max(3, Mathf.RoundToInt(length / stoneLen));

            for (int s = 0; s < count; s++)
            {
                float t = (s + 0.5f) / count;
                Vector3 basePos = Vector3.Lerp(a, b, 0.05f + t * 0.9f);

                float jitterSide = ((float)rng.NextDouble() - 0.5f) * height * 0.5f;
                float h = height * (0.7f + (float)rng.NextDouble() * 0.55f);
                float w = height * (0.6f + (float)rng.NextDouble() * 0.5f);
                float l = stoneLen * (0.8f + (float)rng.NextDouble() * 0.4f);

                var block = CreateBlock($"Stone_{edge}_{s}",
                    basePos + perp * jitterSide + new Vector3(0f, h * 0.5f, 0f),
                    new Vector3(w, h, l));
                block.transform.localRotation =
                    Quaternion.LookRotation(dir) *
                    Quaternion.Euler(0f, ((float)rng.NextDouble() - 0.5f) * 14f, 0f);
            }
        }

        /// <summary>Show walls for player-owned sectors, hide otherwise.</summary>
        public void SetVisible(bool visible)
        {
            if (_root != null && _root.activeSelf != visible)
                _root.SetActive(visible);
        }

        private GameObject CreateBlock(string name, Vector3 localPos, Vector3 scale)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(_root.transform, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = scale;
            // No collider — sector clicks go to the hex mesh
            Destroy(go.GetComponent<Collider>());
            var renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = _wallMaterial;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            return go;
        }

        private static void EnsureMaterial()
        {
            if (_wallMaterial != null) return;
            _wallMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            _wallMaterial.name = "SectorWallStone";
            _wallMaterial.color = STONE;
            _wallMaterial.enableInstancing = true;
        }
    }
}
