using UnityEngine;
using UnityEngine.InputSystem;
using Settlers.Simulation;
using Settlers.UI;

namespace Settlers.Presentation
{
    /// <summary>
    /// Handles building placement: ghost preview follows mouse,
    /// click to confirm placement within a valid sector.
    /// Uses New Input System.
    /// </summary>
    public class BuildingPlacer : MonoBehaviour
    {
        [SerializeField] private Material _ghostMaterial;
        [SerializeField] private Color _validColor = new Color(0.2f, 0.8f, 0.2f, 0.4f);
        [SerializeField] private Color _invalidColor = new Color(0.8f, 0.2f, 0.2f, 0.4f);
        [SerializeField] private LayerMask _groundLayer;

        private GameObject _ghostObject;
        private MeshRenderer[] _ghostRenderers;
        private BaseBuildingType _selectedType;
        private bool _isPlacing;
        private Camera _mainCamera;
        private int _hoveredSectorId = -1;
        private NotificationUI _notificationUI;
        private string _lastReason;

        /// <summary>Whether a building placement is currently active.</summary>
        public bool IsPlacing => _isPlacing;

        /// <summary>The currently selected building type being placed.</summary>
        public BaseBuildingType SelectedType => _selectedType;

        /// <summary>Fired when placement succeeds: (sectorId, buildingType, worldPosition).</summary>
        public event System.Action<int, BaseBuildingType, Vector3> OnBuildingPlaced;

        /// <summary>Fired when placement is cancelled.</summary>
        public event System.Action OnPlacementCancelled;

        private void Start()
        {
            _mainCamera = Camera.main;
            _notificationUI = FindAnyObjectByType<NotificationUI>();
            EnsureGhostMaterial();
        }

        private void Update()
        {
            if (!_isPlacing) return;

            UpdateGhostPosition();
            HandlePlacementInput();
        }

        /// <summary>Begin placement mode for a building type.</summary>
        public void BeginPlacement(BaseBuildingType type)
        {
            CancelPlacement();

            _selectedType = type;
            _isPlacing = true;
            _lastReason = null;
            CreateGhost(type);
        }

        /// <summary>Cancel the current placement.</summary>
        public void CancelPlacement()
        {
            _isPlacing = false;

            if (_ghostObject != null)
            {
                Destroy(_ghostObject);
                _ghostObject = null;
                _ghostRenderers = null;
            }

            OnPlacementCancelled?.Invoke();
        }

        private void UpdateGhostPosition()
        {
            if (Mouse.current == null || _ghostObject == null || _mainCamera == null)
                return;

            var mousePos = Mouse.current.position.ReadValue();
            var ray = _mainCamera.ScreenPointToRay(mousePos);

            // Raycast against ground plane (Y=0)
            var groundPlane = new Plane(Vector3.up, Vector3.zero);
            if (groundPlane.Raycast(ray, out float distance))
            {
                var hitPoint = ray.GetPoint(distance);

                // Determine which sector we're over
                _hoveredSectorId = FindSectorAtPosition(hitPoint);

                // Snap ghost to sector center if over a valid sector
                if (_hoveredSectorId >= 0)
                {
                    var gc = GameController.Instance;
                    var sectorView = gc?.GetSectorView(_hoveredSectorId);
                    if (sectorView != null)
                    {
                        var sectorPos = sectorView.transform.position;
                        _ghostObject.transform.position = new Vector3(sectorPos.x, 0f, sectorPos.z);
                    }
                    else
                    {
                        _ghostObject.transform.position = new Vector3(hitPoint.x, 0f, hitPoint.z);
                    }
                }
                else
                {
                    _ghostObject.transform.position = new Vector3(hitPoint.x, 0f, hitPoint.z);
                }

                string reason = GetPlacementReason(_hoveredSectorId);
                bool isValid = reason == null;
                SetGhostColor(isValid ? _validColor : _invalidColor);

                if (reason != _lastReason && reason != null && _notificationUI != null)
                    _notificationUI.Show(reason);
                _lastReason = reason;
            }
        }

        private void HandlePlacementInput()
        {
            if (Mouse.current == null || Keyboard.current == null)
                return;

            // Escape or right-click to cancel
            if (Keyboard.current.escapeKey.wasPressedThisFrame ||
                Mouse.current.rightButton.wasPressedThisFrame)
            {
                CancelPlacement();
                return;
            }

            // Left click to place
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (_hoveredSectorId >= 0 && GetPlacementReason(_hoveredSectorId) == null)
                {
                    var position = _ghostObject.transform.position;
                    var placedType = _selectedType;
                    CancelPlacement();
                    OnBuildingPlaced?.Invoke(_hoveredSectorId, placedType, position);
                    if (_notificationUI != null)
                        _notificationUI.Show("Building placed");
                }
            }
        }

        private int FindSectorAtPosition(Vector3 worldPosition)
        {
            var gc = GameController.Instance;
            if (gc == null || gc.Graph == null) return -1;

            float bestDist = float.MaxValue;
            int bestId = -1;

            for (int i = 0; i < gc.Graph.SectorCount; i++)
            {
                var view = gc.GetSectorView(i);
                if (view == null) continue;

                float dist = Vector3.Distance(
                    new Vector3(worldPosition.x, 0f, worldPosition.z),
                    new Vector3(view.transform.position.x, 0f, view.transform.position.z));

                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestId = i;
                }
            }

            // Only valid if within sector radius (roughly)
            return bestDist < 10f ? bestId : -1;
        }

        /// <summary>Returns null if placement is valid, or a reason string if invalid.</summary>
        private string GetPlacementReason(int sectorId)
        {
            if (sectorId < 0) return "No sector";

            var gc = GameController.Instance;
            if (gc == null) return "No sector";

            var sector = gc.Graph.GetSector(sectorId);

            if (sector.OwnerId != 0) return "Not your sector";

            int currentBuildings = gc.GetBuildingCountInSector(sectorId);
            if (currentBuildings >= sector.BuildSlots) return "No build slots";

            var resources = gc.GetPlayerResources(0);
            if (resources != null)
            {
                BuildingCosts.Get(_selectedType, out int plankCost, out int stoneCost);
                if (!resources.CanAfford(plankCost, stoneCost))
                    return $"Need {plankCost} planks, {stoneCost} stone";
            }

            return null;
        }

        // Building costs use shared BuildingCosts.Get() — see Simulation/Economy/BuildingCosts.cs

        private void CreateGhost(BaseBuildingType type)
        {
            // Reuse BuildingView's primitive creation but with ghost material
            _ghostObject = new GameObject($"Ghost_{type}");

            float height = GetBuildingHeight(type);
            float width = GetBuildingWidth(type);

            var baseObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            baseObj.name = "Base";
            baseObj.transform.SetParent(_ghostObject.transform, false);
            baseObj.transform.localScale = new Vector3(width, height, width);
            baseObj.transform.localPosition = new Vector3(0f, height * 0.5f, 0f);
            Destroy(baseObj.GetComponent<Collider>());

            var roofObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            roofObj.name = "Roof";
            roofObj.transform.SetParent(_ghostObject.transform, false);
            roofObj.transform.localScale = new Vector3(width * 1.2f, height * 0.4f, width * 1.2f);
            roofObj.transform.localPosition = new Vector3(0f, height + height * 0.15f, 0f);
            Destroy(roofObj.GetComponent<Collider>());

            // Apply ghost material
            _ghostRenderers = _ghostObject.GetComponentsInChildren<MeshRenderer>();
            foreach (var r in _ghostRenderers)
                r.sharedMaterial = _ghostMaterial;

            SetGhostColor(_validColor);
        }

        private void SetGhostColor(Color color)
        {
            if (_ghostRenderers == null) return;

            foreach (var r in _ghostRenderers)
            {
                var block = new MaterialPropertyBlock();
                block.SetColor("_BaseColor", color);
                r.SetPropertyBlock(block);
            }
        }

        private float GetBuildingHeight(BaseBuildingType type)
        {
            return type switch
            {
                BaseBuildingType.Lodge => 1.5f,
                BaseBuildingType.Farm => 1.2f,
                BaseBuildingType.MountainShelter => 1.8f,
                BaseBuildingType.Residence => 2.5f,
                BaseBuildingType.NobleResidence => 3.0f,
                _ => 1.5f
            };
        }

        private float GetBuildingWidth(BaseBuildingType type)
        {
            return type switch
            {
                BaseBuildingType.Lodge => 1.2f,
                BaseBuildingType.Farm => 1.5f,
                BaseBuildingType.MountainShelter => 1.0f,
                BaseBuildingType.Residence => 1.4f,
                BaseBuildingType.NobleResidence => 1.6f,
                _ => 1.2f
            };
        }

        private void EnsureGhostMaterial()
        {
            if (_ghostMaterial != null) return;

            _ghostMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            _ghostMaterial.name = "GhostBuilding";
            _ghostMaterial.SetFloat("_Surface", 1f); // Transparent
            _ghostMaterial.SetFloat("_Blend", 0f);
            _ghostMaterial.renderQueue = 3000;
            _ghostMaterial.color = _validColor;
        }
    }
}
