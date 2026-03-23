using UnityEngine;

namespace Settlers.Presentation
{
    /// <summary>
    /// Visual representation of a single map sector.
    /// Reads simulation state from the SectorGraph — never modifies it directly.
    /// </summary>
    public class SectorView : MonoBehaviour
    {
        [Header("Sector Binding")]
        [SerializeField] private int _sectorId;

        [Header("Visual Components")]
        [SerializeField] private MeshRenderer _terrainRenderer;
        [SerializeField] private LineRenderer _borderRenderer;

        [Header("Ownership Colors")]
        [SerializeField] private Color _unownedColor = new Color(0.6f, 0.6f, 0.6f);
        [SerializeField] private Color _neutralColor = new Color(0.8f, 0.7f, 0.5f);
        [SerializeField] private Color[] _playerColors = new Color[]
        {
            new Color(0.2f, 0.5f, 0.9f),  // Player 0: Blue
            new Color(0.9f, 0.2f, 0.2f),  // Player 1: Red
            new Color(0.2f, 0.8f, 0.3f),  // Player 2: Green
            new Color(0.9f, 0.8f, 0.1f),  // Player 3: Yellow
        };

        [Header("Selection")]
        [SerializeField] private GameObject _selectionHighlight;
        [SerializeField] private Color _highlightColor = new Color(1f, 1f, 1f, 0.3f);

        private MaterialPropertyBlock _propertyBlock;
        private bool _isSelected;
        private static readonly int ColorProperty = Shader.PropertyToID("_BaseColor");

        /// <summary>The simulation sector ID this view represents.</summary>
        public int SectorId => _sectorId;

        /// <summary>Whether this sector is currently selected by the player.</summary>
        public bool IsSelected => _isSelected;

        private void Awake()
        {
            _propertyBlock = new MaterialPropertyBlock();

            if (_selectionHighlight != null)
                _selectionHighlight.SetActive(false);
        }

        /// <summary>
        /// Initialize this view with a sector ID and world position.
        /// Called by MapBuilder or a setup script.
        /// </summary>
        public void Initialize(int sectorId, Vector3 position)
        {
            _sectorId = sectorId;
            transform.position = position;
        }

        /// <summary>
        /// Update the visual state based on the current simulation owner.
        /// Call this from a presentation manager each frame or on ownership change.
        /// </summary>
        public void UpdateOwnership(int ownerId)
        {
            Color color = GetOwnerColor(ownerId);
            ApplyTerrainColor(color);
        }

        /// <summary>Set this sector as selected and show highlight.</summary>
        public void Select()
        {
            _isSelected = true;
            if (_selectionHighlight != null)
                _selectionHighlight.SetActive(true);
        }

        /// <summary>Deselect this sector and hide highlight.</summary>
        public void Deselect()
        {
            _isSelected = false;
            if (_selectionHighlight != null)
                _selectionHighlight.SetActive(false);
        }

        /// <summary>
        /// Set up the border outline from sector boundary points.
        /// Points should form a closed loop in world space.
        /// </summary>
        public void SetBorderPoints(Vector3[] points)
        {
            if (_borderRenderer == null || points == null || points.Length < 3)
                return;

            // Close the loop by adding first point at the end
            var closed = new Vector3[points.Length + 1];
            points.CopyTo(closed, 0);
            closed[points.Length] = points[0];

            _borderRenderer.positionCount = closed.Length;
            _borderRenderer.SetPositions(closed);
        }

        /// <summary>Update border color to match current ownership.</summary>
        public void UpdateBorderColor(int ownerId)
        {
            if (_borderRenderer == null)
                return;

            Color color = GetOwnerColor(ownerId);
            _borderRenderer.startColor = color;
            _borderRenderer.endColor = color;
        }

        private Color GetOwnerColor(int ownerId)
        {
            if (ownerId == Simulation.Sector.UNOWNED)
                return _unownedColor;
            if (ownerId == Simulation.Sector.NEUTRAL)
                return _neutralColor;
            if (ownerId >= 0 && ownerId < _playerColors.Length)
                return _playerColors[ownerId];

            return _unownedColor;
        }

        private void ApplyTerrainColor(Color color)
        {
            if (_terrainRenderer == null)
                return;

            _terrainRenderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetColor(ColorProperty, color);
            _terrainRenderer.SetPropertyBlock(_propertyBlock);
        }
    }
}
