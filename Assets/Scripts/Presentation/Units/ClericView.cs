using UnityEngine;
using Settlers.Simulation;

namespace Settlers.Presentation
{
    /// <summary>
    /// Visual representation of clerics performing proselytism on a sector.
    /// Shows a group of small spheres circling the target sector.
    /// </summary>
    public class ClericView : MonoBehaviour
    {
        private int _sectorId;
        private int _ownerId;
        private float _angle;
        private MeshRenderer _renderer;
        private MaterialPropertyBlock _propBlock;

        private static readonly int ColorProp = Shader.PropertyToID("_BaseColor");

        public int SectorId => _sectorId;

        public static ClericView Create(Transform parent, int sectorId, int ownerId,
            Material material)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = $"Cleric_{ownerId}_s{sectorId}";
            go.transform.SetParent(parent, false);
            go.transform.localScale = new Vector3(0.4f, 0.6f, 0.4f);

            var collider = go.GetComponent<Collider>();
            if (collider != null) Object.Destroy(collider);

            var view = go.AddComponent<ClericView>();
            view._sectorId = sectorId;
            view._ownerId = ownerId;
            view._angle = Random.Range(0f, Mathf.PI * 2f);

            view._renderer = go.GetComponent<MeshRenderer>();
            if (material != null)
                view._renderer.sharedMaterial = material;
            view._propBlock = new MaterialPropertyBlock();

            // White/gold color for clerics
            view.SetColor(new Color(0.95f, 0.9f, 0.6f));

            return view;
        }

        /// <summary>Update position: orbit around sector center.</summary>
        public void UpdatePosition(float progress)
        {
            var gc = GameController.Instance;
            if (gc == null) return;

            var center = gc.GetSectorPosition(_sectorId);
            _angle += Time.deltaTime * 1.5f;

            float radius = 2f;
            float x = center.x + Mathf.Cos(_angle) * radius;
            float z = center.z + Mathf.Sin(_angle) * radius;
            float y = 0.3f + Mathf.Sin(Time.time * 2f + _angle) * 0.1f;

            transform.position = new Vector3(x, y, z);
        }

        private void SetColor(Color color)
        {
            if (_renderer == null) return;
            _renderer.GetPropertyBlock(_propBlock);
            _propBlock.SetColor(ColorProp, color);
            _renderer.SetPropertyBlock(_propBlock);
        }
    }
}
