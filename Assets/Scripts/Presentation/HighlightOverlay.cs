using UnityEngine;

namespace Settlers.Presentation
{
    /// <summary>
    /// Pulsing transparent ring rendered around the currently selected sector.
    /// Driven by scale animation on a flat cylinder mesh placed at ground level.
    /// </summary>
    public class HighlightOverlay : MonoBehaviour
    {
        [SerializeField] private MeshRenderer _renderer;
        [SerializeField] private float _pulseSpeed  = 2.5f;
        [SerializeField] private float _minScale    = 0.9f;
        [SerializeField] private float _maxScale    = 1.05f;
        [SerializeField] private Color _ringColor   = new Color(1f, 1f, 0.5f, 0.35f);

        private bool _visible;
        private float _phase;

        private void Awake()
        {
            if (_renderer == null) _renderer = GetComponent<MeshRenderer>();
            gameObject.SetActive(false);
        }

        public void Show(Vector3 worldPos)
        {
            transform.position = worldPos + Vector3.up * 0.05f;
            gameObject.SetActive(true);
            _visible = true;
            _phase   = 0f;
        }

        public void Hide()
        {
            _visible = false;
            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (!_visible) return;
            _phase += Time.deltaTime * _pulseSpeed;
            float t = (Mathf.Sin(_phase) + 1f) * 0.5f;
            float s = Mathf.Lerp(_minScale, _maxScale, t);
            transform.localScale = new Vector3(s, 0.02f, s);

            if (_renderer != null)
            {
                var mpb = new MaterialPropertyBlock();
                _renderer.GetPropertyBlock(mpb);
                float alpha = Mathf.Lerp(0.2f, _ringColor.a, t);
                mpb.SetColor("_BaseColor",
                    new Color(_ringColor.r, _ringColor.g, _ringColor.b, alpha));
                _renderer.SetPropertyBlock(mpb);
            }
        }

        public static HighlightOverlay Create(Transform root)
        {
            var go = new GameObject("HighlightOverlay");
            go.transform.SetParent(root, false);

            var mf = go.AddComponent<MeshFilter>();
            mf.mesh = CreateRingMesh(1f, 0.12f, 32);

            var mr = go.AddComponent<MeshRenderer>();
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows    = false;

            var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            if (mat != null)
            {
                mat.SetFloat("_Surface", 1f); // transparent
                mat.SetFloat("_Blend",   0f); // alpha
                mat.color = new Color(1f, 1f, 0.5f, 0.35f);
            }
            mr.material = mat;

            var overlay = go.AddComponent<HighlightOverlay>();
            overlay._renderer = mr;
            go.SetActive(false);
            return overlay;
        }

        private static Mesh CreateRingMesh(float outerR, float thickness, int segments)
        {
            var mesh = new Mesh();
            float innerR = outerR - thickness;
            int vCount = segments * 2;
            var verts  = new Vector3[vCount];
            var tris   = new int[segments * 6];
            var uvs    = new Vector2[vCount];

            for (int i = 0; i < segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                float cos   = Mathf.Cos(angle);
                float sin   = Mathf.Sin(angle);
                verts[i * 2]     = new Vector3(cos * outerR, 0f, sin * outerR);
                verts[i * 2 + 1] = new Vector3(cos * innerR, 0f, sin * innerR);
                uvs[i * 2]       = new Vector2(cos * 0.5f + 0.5f, sin * 0.5f + 0.5f);
                uvs[i * 2 + 1]   = new Vector2(cos * 0.4f + 0.5f, sin * 0.4f + 0.5f);

                int next = (i + 1) % segments;
                int idx  = i * 6;
                tris[idx]     = i * 2;
                tris[idx + 1] = next * 2;
                tris[idx + 2] = i * 2 + 1;
                tris[idx + 3] = next * 2;
                tris[idx + 4] = next * 2 + 1;
                tris[idx + 5] = i * 2 + 1;
            }

            mesh.vertices  = verts;
            mesh.triangles = tris;
            mesh.uv        = uvs;
            mesh.RecalculateNormals();
            return mesh;
        }
    }
}
