using UnityEngine;
using Settlers.Simulation;

namespace Settlers.Presentation
{
    /// <summary>
    /// Procedural nature decoration inside a sector (§14.10): trees on forest
    /// sectors, rock clusters on mineral deposits, bushes on fertile land.
    /// Placement is deterministic per sector and avoids the center build area
    /// and the six road corridors. Always visible regardless of ownership.
    /// </summary>
    public class SectorDecorView : MonoBehaviour
    {
        // Placement band (fraction of sector radius) keeps decor out of the
        // central build slots while staying inside the walls
        private const float BAND_MIN = 0.50f;
        private const float BAND_MAX = 0.82f;
        // Roads leave the sector roughly through edge midpoints (every 60°)
        private const float ROAD_CLEARANCE_DEG = 14f;

        private const int TREE_COUNT = 12;
        private const int ROCK_CLUSTER_COUNT = 5;
        private const int BUSH_COUNT = 7;

        private static readonly Color TRUNK_COLOR = new(0.42f, 0.30f, 0.18f);
        private static readonly Color CANOPY_A = new(0.22f, 0.45f, 0.18f);
        private static readonly Color CANOPY_B = new(0.31f, 0.55f, 0.22f);
        private static readonly Color ROCK_COLOR = new(0.52f, 0.50f, 0.46f);
        private static readonly Color BUSH_COLOR = new(0.32f, 0.52f, 0.24f);

        private static Material _trunkMat, _canopyAMat, _canopyBMat, _rockMat, _bushMat;

        private GameObject _root;

        /// <summary>Build decoration once, from the sector's resource nodes.</summary>
        public void Build(float radius, Sector sector)
        {
            if (_root != null) return;
            _root = new GameObject("Decor");
            _root.transform.SetParent(transform, false);
            EnsureMaterials();

            var rng = new System.Random(sector.Id * 4241 + 907);

            if (sector.HasResource(ResourceNodeType.Forest))
                for (int i = 0; i < TREE_COUNT; i++)
                    PlaceTree(RandomSpot(rng, radius), rng);

            if (sector.HasResource(ResourceNodeType.Stone) ||
                sector.HasResource(ResourceNodeType.Coal) ||
                sector.HasResource(ResourceNodeType.Iron) ||
                sector.HasResource(ResourceNodeType.Gold))
                for (int i = 0; i < ROCK_CLUSTER_COUNT; i++)
                    PlaceRockCluster(RandomSpot(rng, radius), rng);

            if (sector.HasResource(ResourceNodeType.FertileLand))
                for (int i = 0; i < BUSH_COUNT; i++)
                    PlaceBush(RandomSpot(rng, radius), rng);
        }

        /// <summary>Random position in the decor band, skipping road corridors.
        /// Returns Vector3.zero (skip marker) if no clear angle is found.</summary>
        private Vector3 RandomSpot(System.Random rng, float radius)
        {
            for (int attempt = 0; attempt < 8; attempt++)
            {
                float angleDeg = (float)rng.NextDouble() * 360f;
                float withinCorridor = angleDeg % 60f;
                float nearestCorridor = Mathf.Min(withinCorridor, 60f - withinCorridor);
                if (nearestCorridor < ROAD_CLEARANCE_DEG) continue;
                float dist = radius * Mathf.Lerp(BAND_MIN, BAND_MAX, (float)rng.NextDouble());
                float a = angleDeg * Mathf.Deg2Rad;
                return new Vector3(Mathf.Cos(a) * dist, 0f, Mathf.Sin(a) * dist);
            }
            return Vector3.zero;
        }

        private void PlaceTree(Vector3 pos, System.Random rng)
        {
            if (pos == Vector3.zero) return;
            float scale = 0.8f + (float)rng.NextDouble() * 0.5f;

            var trunk = CreatePart(PrimitiveType.Cylinder, _trunkMat,
                pos + new Vector3(0f, 0.35f * scale, 0f),
                new Vector3(0.13f, 0.35f, 0.13f) * scale);

            var canopyMat = rng.NextDouble() < 0.5 ? _canopyAMat : _canopyBMat;
            CreatePart(PrimitiveType.Sphere, canopyMat,
                pos + new Vector3(0f, 1.0f * scale, 0f),
                new Vector3(0.85f, 0.75f, 0.85f) * scale);
            CreatePart(PrimitiveType.Sphere, canopyMat,
                pos + new Vector3(0f, 1.45f * scale, 0f),
                new Vector3(0.55f, 0.5f, 0.55f) * scale);

            trunk.transform.localRotation =
                Quaternion.Euler(0f, (float)rng.NextDouble() * 360f, 0f);
        }

        private void PlaceRockCluster(Vector3 pos, System.Random rng)
        {
            if (pos == Vector3.zero) return;
            int stones = 2 + rng.Next(3);
            for (int i = 0; i < stones; i++)
            {
                float s = 0.30f + (float)rng.NextDouble() * 0.40f;
                var offset = new Vector3(
                    ((float)rng.NextDouble() - 0.5f) * 0.8f, s * 0.30f,
                    ((float)rng.NextDouble() - 0.5f) * 0.8f);
                var rock = CreatePart(PrimitiveType.Cube, _rockMat,
                    pos + offset, new Vector3(s, s * 0.7f, s));
                rock.transform.localRotation = Quaternion.Euler(
                    ((float)rng.NextDouble() - 0.5f) * 20f,
                    (float)rng.NextDouble() * 360f,
                    ((float)rng.NextDouble() - 0.5f) * 20f);
            }
        }

        private void PlaceBush(Vector3 pos, System.Random rng)
        {
            if (pos == Vector3.zero) return;
            float s = 0.30f + (float)rng.NextDouble() * 0.25f;
            CreatePart(PrimitiveType.Sphere, _bushMat,
                pos + new Vector3(0f, s * 0.5f, 0f),
                new Vector3(s * 1.5f, s, s * 1.5f));
        }

        private GameObject CreatePart(PrimitiveType type, Material mat,
            Vector3 localPos, Vector3 scale)
        {
            var go = GameObject.CreatePrimitive(type);
            go.transform.SetParent(_root.transform, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = scale;
            // No collider — sector clicks must reach the hex mesh
            Destroy(go.GetComponent<Collider>());
            var renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = mat;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            return go;
        }

        private static void EnsureMaterials()
        {
            if (_trunkMat != null) return;
            _trunkMat = CreateMaterial("DecorTrunk", TRUNK_COLOR);
            _canopyAMat = CreateMaterial("DecorCanopyA", CANOPY_A);
            _canopyBMat = CreateMaterial("DecorCanopyB", CANOPY_B);
            _rockMat = CreateMaterial("DecorRock", ROCK_COLOR);
            _bushMat = CreateMaterial("DecorBush", BUSH_COLOR);
        }

        private static Material CreateMaterial(string name, Color color)
        {
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"))
            {
                name = name,
                color = color,
                enableInstancing = true,
            };
            return mat;
        }
    }
}
