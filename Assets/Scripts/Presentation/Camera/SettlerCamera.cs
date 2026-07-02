using UnityEngine;
using UnityEngine.InputSystem;

namespace Settlers.Presentation
{
    /// <summary>
    /// Orbit camera for the Settlers map.
    /// Pan (WASD / edge scroll), Zoom (scroll wheel), Rotate (Q/E).
    /// Tilt auto-adjusts with zoom: close = angled, far = top-down.
    /// </summary>
    public class SettlerCamera : MonoBehaviour
    {
        [Header("Speed")]
        [SerializeField] private float _panSpeed = 20f;
        [SerializeField] private float _zoomSpeed = 10f;
        [SerializeField] private float _rotateSpeed = 90f;

        [Header("Zoom Limits")]
        [SerializeField] private float _minDistance = 15f;
        [SerializeField] private float _maxDistance = 200f;

        [Header("Elevation (radians)")]
        [SerializeField] private float _minElevation = 0.5f;
        [SerializeField] private float _maxElevation = 1.3f;

        [Header("Edge Scroll")]
        [SerializeField] private float _edgeScrollMargin = 10f;
        [SerializeField] private bool _enableEdgeScroll = true;

        [Header("Map Bounds")]
        [SerializeField] private float _mapMinX = -60f;
        [SerializeField] private float _mapMaxX = 60f;
        [SerializeField] private float _mapMinZ = -60f;
        [SerializeField] private float _mapMaxZ = 60f;

        private Vector3 _target;
        private float _distance = 50f;
        private float _azimuth;
        private float _elevation = 0.8f;

        private Keyboard _kb;
        private Mouse _mouse;
        private Vector2 _lastMousePos;
        private bool _isDragging;

        private void Awake()
        {
            _target = Vector3.zero;
            _azimuth = 0f;
            _distance = 50f;
            _elevation = 0.8f;
        }

        private void LateUpdate()
        {
            _kb = Keyboard.current;
            _mouse = Mouse.current;
            if (_kb == null || _mouse == null)
                return;

            float dt = Time.deltaTime;

            HandlePan(dt);
            HandleRotation(dt);
            HandleZoom();
            UpdateElevation();
            ApplyTransform();
        }

        private void HandlePan(float dt)
        {
            // Build a pan direction in camera-relative XZ
            var forward = transform.forward;
            forward.y = 0f;
            forward.Normalize();
            var right = transform.right;
            right.y = 0f;
            right.Normalize();

            var panDir = Vector3.zero;

            // WASD
            if (_kb.wKey.isPressed) panDir += forward;
            if (_kb.sKey.isPressed) panDir -= forward;
            if (_kb.dKey.isPressed) panDir += right;
            if (_kb.aKey.isPressed) panDir -= right;

            // Middle-mouse drag pan
            if (_mouse.middleButton.isPressed)
            {
                Vector2 mousePos = _mouse.position.ReadValue();
                if (_isDragging)
                {
                    Vector2 delta = mousePos - _lastMousePos;
                    float speedScale = _distance / 50f;
                    float dragScale = speedScale * 0.05f;
                    panDir -= right * (delta.x * dragScale);
                    panDir -= forward * (delta.y * dragScale);
                }
                _lastMousePos = mousePos;
                _isDragging = true;
            }
            else
            {
                _isDragging = false;
            }

            // Edge scrolling — only while the window has focus, otherwise the
            // camera drifts into the map-bounds corner whenever focus is lost
            if (_enableEdgeScroll && Application.isFocused)
            {
                Vector2 mousePos = _mouse.position.ReadValue();
                float screenW = Screen.width;
                float screenH = Screen.height;

                if (mousePos.x < _edgeScrollMargin) panDir -= right;
                if (mousePos.x > screenW - _edgeScrollMargin) panDir += right;
                if (mousePos.y < _edgeScrollMargin) panDir -= forward;
                if (mousePos.y > screenH - _edgeScrollMargin) panDir += forward;
            }

            if (panDir.sqrMagnitude > 0.001f)
            {
                // Pan speed scales with zoom distance for consistent feel
                float speedScale = _distance / 50f;
                _target += panDir.normalized * (_panSpeed * speedScale * dt);

                // Clamp to map bounds
                _target.x = Mathf.Clamp(_target.x, _mapMinX, _mapMaxX);
                _target.z = Mathf.Clamp(_target.z, _mapMinZ, _mapMaxZ);
            }
        }

        private void HandleRotation(float dt)
        {
            if (_kb.qKey.isPressed) _azimuth -= _rotateSpeed * dt * Mathf.Deg2Rad;
            if (_kb.eKey.isPressed) _azimuth += _rotateSpeed * dt * Mathf.Deg2Rad;

            // Right-click drag rotation
            if (_mouse.rightButton.isPressed)
            {
                Vector2 delta = _mouse.delta.ReadValue();
                _azimuth += delta.x * 0.003f;
            }
        }

        private void HandleZoom()
        {
            float scroll = _mouse.scroll.ReadValue().y;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                _distance -= scroll * _zoomSpeed * 0.1f;
                _distance = Mathf.Clamp(_distance, _minDistance, _maxDistance);
            }
        }

        /// <summary>
        /// Auto-adjust elevation based on zoom: close = angled, far = top-down.
        /// </summary>
        private void UpdateElevation()
        {
            float t = Mathf.InverseLerp(_minDistance, _maxDistance, _distance);
            _elevation = Mathf.Lerp(_minElevation, _maxElevation, t);
        }

        private void ApplyTransform()
        {
            // Spherical coordinates → cartesian offset
            float x = _distance * Mathf.Cos(_elevation) * Mathf.Sin(_azimuth);
            float y = _distance * Mathf.Sin(_elevation);
            float z = _distance * Mathf.Cos(_elevation) * Mathf.Cos(_azimuth);

            transform.position = _target + new Vector3(x, y, z);
            transform.LookAt(_target);
        }

        /// <summary>Snap camera to look at a world position.</summary>
        public void FocusOn(Vector3 worldPos)
        {
            _target = worldPos;
            _target.y = 0f;
        }

        /// <summary>Current zoom-normalized value (0 = closest, 1 = farthest).</summary>
        public float ZoomNormalized => Mathf.InverseLerp(_minDistance, _maxDistance, _distance);
    }
}
