using UnityEngine;

namespace Settlers.Presentation
{
    /// <summary>
    /// Visual representation of clerics performing proselytism on a sector.
    /// A hooded, robed figure in cream-white circles the target sector,
    /// facing its direction of travel.
    /// </summary>
    public class ClericView : MonoBehaviour
    {
        private static readonly Color ROBE_COLOR = new(0.93f, 0.89f, 0.72f);

        private int _sectorId;
        private int _ownerId;
        private float _angle;

        public int SectorId => _sectorId;

        public static ClericView Create(Transform parent, int sectorId, int ownerId,
            Material material)
        {
            var go = new GameObject($"Cleric_{ownerId}_s{sectorId}");
            go.transform.SetParent(parent, false);

            UnitFigureFactory.CreateFigure(go.transform,
                UnitFigureFactory.Role.Cleric, ROBE_COLOR, material);

            var view = go.AddComponent<ClericView>();
            view._sectorId = sectorId;
            view._ownerId = ownerId;
            view._angle = Random.Range(0f, Mathf.PI * 2f);

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
            float y = 0.02f + Mathf.Abs(Mathf.Sin(Time.time * 5f + _angle)) * 0.04f;

            transform.position = new Vector3(x, y, z);

            // Face the orbit tangent (counter-clockwise travel direction)
            var tangent = new Vector3(-Mathf.Sin(_angle), 0f, Mathf.Cos(_angle));
            transform.rotation = Quaternion.LookRotation(tangent);
        }
    }
}
