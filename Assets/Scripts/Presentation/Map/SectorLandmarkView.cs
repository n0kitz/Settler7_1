using UnityEngine;

namespace Settlers.Presentation
{
    /// <summary>
    /// Procedural landmark structure at a sector centre (§14.10): a multi-story
    /// home castle in the owning player's colour, or a menacing fortified
    /// stronghold on guarded neutral sectors. Built once at spawn and decorative
    /// only — no colliders, so sector clicks still reach the hex mesh underneath.
    /// </summary>
    public class SectorLandmarkView : MonoBehaviour
    {
        private static readonly Color STONE_LIGHT = new(0.66f, 0.63f, 0.58f);
        private static readonly Color STONE_DARK = new(0.40f, 0.39f, 0.37f);
        private static readonly Color ROOF_SLATE = new(0.30f, 0.32f, 0.40f);
        private static readonly Color GOLD = new(0.87f, 0.71f, 0.22f);
        private static readonly Color IRON = new(0.18f, 0.18f, 0.20f);
        private static readonly Color STRONGHOLD_ROOF = new(0.24f, 0.10f, 0.10f);

        private static Material _stoneLightMat, _stoneDarkMat, _slateMat,
            _goldMat, _ironMat, _bannerMat, _strongholdRoofMat;
        private static Mesh _coneMesh;
        private static readonly int ColorProp = Shader.PropertyToID("_BaseColor");

        private GameObject _root;

        /// <summary>Build a dominating home castle in the given player colour.</summary>
        public void BuildHomeCastle(float radius, Color playerColor)
        {
            if (_root != null) return;
            Init();
            _root = NewRoot("HomeCastle");
            float u = radius * 0.30f;

            // Central keep — two stone stories with battlements between
            Prim(PrimitiveType.Cube, _stoneLightMat,
                new Vector3(0f, u * 0.70f, 0f), new Vector3(u * 1.4f, u * 1.4f, u * 1.4f));
            Merlons(u * 1.42f, u * 0.72f, 3, _stoneLightMat, u * 0.20f);
            Prim(PrimitiveType.Cube, _stoneLightMat,
                new Vector3(0f, u * 1.75f, 0f), new Vector3(u * 0.95f, u * 0.75f, u * 0.95f));

            // Four corner towers; front pair carries gold finials
            float t = u * 0.88f;
            BuildTower(new Vector3(t, 0f, t), u, true);
            BuildTower(new Vector3(-t, 0f, t), u, true);
            BuildTower(new Vector3(t, 0f, -t), u, false);
            BuildTower(new Vector3(-t, 0f, -t), u, false);

            // Gatehouse door on the +Z (camera-facing) wall
            Prim(PrimitiveType.Cube, _stoneDarkMat,
                new Vector3(0f, u * 0.5f, u * 0.72f), new Vector3(u * 0.5f, u * 0.9f, u * 0.16f));

            // Player banner crowning the keep
            Banner(new Vector3(0f, u * 2.15f, 0f), u * 1.1f, playerColor);
        }

        /// <summary>Build a squat, dark, cannon-studded stronghold on a guarded sector.</summary>
        public void BuildStronghold(float radius)
        {
            if (_root != null) return;
            Init();
            _root = NewRoot("Stronghold");
            float u = radius * 0.34f;

            // Wide, low keep of dark stone
            Prim(PrimitiveType.Cube, _stoneDarkMat,
                new Vector3(0f, u * 0.55f, 0f), new Vector3(u * 1.7f, u * 1.1f, u * 1.7f));
            Merlons(u * 1.12f, u * 0.87f, 4, _stoneDarkMat, u * 0.22f);

            // Central watchtower with a blood-red roof
            float th = u * 2.4f, tr = u * 0.5f;
            Prim(PrimitiveType.Cylinder, _stoneDarkMat,
                new Vector3(0f, th * 0.5f, 0f), new Vector3(tr, th * 0.5f, tr));
            Cone(_strongholdRoofMat, new Vector3(0f, th, 0f),
                new Vector3(tr * 1.3f, u * 1.0f, tr * 1.3f));

            // Cannon barrels poking through the walls, one per cardinal side
            CannonBarrel(new Vector3(0f, u * 0.5f, u * 0.85f), 0f);
            CannonBarrel(new Vector3(u * 0.85f, u * 0.5f, 0f), 90f);
            CannonBarrel(new Vector3(0f, u * 0.5f, -u * 0.85f), 180f);

            Banner(new Vector3(0f, th, 0f), u * 0.9f, STRONGHOLD_ROOF);
        }

        // --- Building blocks ---

        private void BuildTower(Vector3 basePos, float u, bool goldFinial)
        {
            float th = u * 2.2f, tr = u * 0.44f;
            Prim(PrimitiveType.Cylinder, _stoneDarkMat,
                basePos + new Vector3(0f, th * 0.5f, 0f), new Vector3(tr, th * 0.5f, tr));
            Cone(_slateMat, basePos + new Vector3(0f, th, 0f),
                new Vector3(tr * 1.25f, u * 0.9f, tr * 1.25f));
            if (goldFinial)
                Prim(PrimitiveType.Sphere, _goldMat,
                    basePos + new Vector3(0f, th + u * 0.85f, 0f), Vector3.one * (tr * 0.4f));
        }

        /// <summary>A ring of merlon cubes along a square perimeter (battlements).</summary>
        private void Merlons(float y, float halfW, int perSide, Material mat, float size)
        {
            float m = halfW;
            for (int s = 0; s < 4; s++)
                for (int k = 0; k <= perSide; k++)
                {
                    float f = -m + (2f * m) * k / perSide;
                    Vector3 p = s switch
                    {
                        0 => new Vector3(f, y, m),
                        1 => new Vector3(f, y, -m),
                        2 => new Vector3(m, y, f),
                        _ => new Vector3(-m, y, f),
                    };
                    Prim(PrimitiveType.Cube, mat, p, new Vector3(size, size, size));
                }
        }

        private void CannonBarrel(Vector3 pos, float yawDeg)
        {
            var go = Prim(PrimitiveType.Cylinder, _ironMat, pos,
                new Vector3(0.12f, 0.35f, 0.12f));
            // Cylinders stand along Y; tilt to point outward horizontally
            go.transform.localRotation = Quaternion.Euler(90f, yawDeg, 0f);
        }

        private void Banner(Vector3 baseTop, float height, Color color)
        {
            Prim(PrimitiveType.Cylinder, _ironMat,
                baseTop + new Vector3(0f, height * 0.5f, 0f),
                new Vector3(0.06f, height * 0.5f, 0.06f));
            var flag = Prim(PrimitiveType.Cube, _bannerMat,
                baseTop + new Vector3(height * 0.28f, height * 0.82f, 0f),
                new Vector3(height * 0.5f, height * 0.32f, 0.04f));
            SetColor(flag, color);
        }

        // --- Primitive helpers (mirrors SectorDecorView) ---

        private GameObject NewRoot(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            return go;
        }

        private GameObject Prim(PrimitiveType type, Material mat, Vector3 localPos, Vector3 scale)
        {
            var go = GameObject.CreatePrimitive(type);
            go.transform.SetParent(_root.transform, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = scale;
            Destroy(go.GetComponent<Collider>());
            var r = go.GetComponent<MeshRenderer>();
            r.sharedMaterial = mat;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            return go;
        }

        private GameObject Cone(Material mat, Vector3 localPos, Vector3 scale)
        {
            var go = new GameObject("Roof");
            go.transform.SetParent(_root.transform, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = scale;
            go.AddComponent<MeshFilter>().sharedMesh = _coneMesh;
            var r = go.AddComponent<MeshRenderer>();
            r.sharedMaterial = mat;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            return go;
        }

        private static void SetColor(GameObject go, Color color)
        {
            var r = go.GetComponent<MeshRenderer>();
            var block = new MaterialPropertyBlock();
            block.SetColor(ColorProp, color);
            r.SetPropertyBlock(block);
        }

        private static void Init()
        {
            if (_stoneLightMat != null) return;
            _stoneLightMat = Mat("CastleStoneLight", STONE_LIGHT);
            _stoneDarkMat = Mat("CastleStoneDark", STONE_DARK);
            _slateMat = Mat("CastleRoofSlate", ROOF_SLATE);
            _goldMat = Mat("CastleGold", GOLD);
            _ironMat = Mat("CastleIron", IRON);
            _bannerMat = Mat("CastleBanner", Color.white);
            _strongholdRoofMat = Mat("StrongholdRoof", STRONGHOLD_ROOF);
            _coneMesh = BuildConeMesh();
        }

        private static Material Mat(string name, Color color) =>
            new(Shader.Find("Universal Render Pipeline/Lit"))
            { name = name, color = color, enableInstancing = true };

        /// <summary>Unit cone: base circle (radius 0.5) at y=0, apex at y=1.
        /// Wound clockwise-from-outside so the sides face out and aren't culled.</summary>
        private static Mesh BuildConeMesh()
        {
            const int seg = 12;
            var verts = new Vector3[seg + 2];
            var tris = new int[seg * 6];
            verts[0] = Vector3.zero;              // base centre
            int apex = seg + 1;
            verts[apex] = new Vector3(0f, 1f, 0f);
            for (int i = 0; i < seg; i++)
            {
                float a = 2f * Mathf.PI * i / seg;
                verts[i + 1] = new Vector3(Mathf.Cos(a) * 0.5f, 0f, Mathf.Sin(a) * 0.5f);
            }
            int t = 0;
            for (int i = 0; i < seg; i++)
            {
                int cur = i + 1, nxt = (i + 1) % seg + 1;
                tris[t++] = apex; tris[t++] = cur; tris[t++] = nxt;  // outward side
                tris[t++] = 0; tris[t++] = nxt; tris[t++] = cur;     // downward base
            }
            var m = new Mesh { name = "LandmarkCone", vertices = verts, triangles = tris };
            m.RecalculateNormals();
            m.RecalculateBounds();
            return m;
        }
    }
}
