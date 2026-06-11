using UnityEngine;

namespace Settlers.Presentation
{
    /// <summary>
    /// Phase 10: VFX initialization — wires procedural particle effects,
    /// floating text, camera shake, and the sector highlight ring.
    /// Partial of BootstrapScene; called from Start() via WireVFX().
    /// </summary>
    public partial class BootstrapScene
    {
        private HighlightOverlay   _highlight;
        private FloatingTextManager   _floatText;
        private ParticleEffectsManager _particles;
        private CameraShake        _cameraShake;

        /// <summary>Create VFX managers and wire them to EventBus events.</summary>
        private void WireVFX()
        {
            var bus = GameController.Instance?.Events;
            var gc  = GameController.Instance;
            if (bus == null || gc == null) return;

            // Scene root for VFX GameObjects
            var vfxRoot = new GameObject("VFXRoot").transform;

            _floatText  = FloatingTextManager.Create(vfxRoot);
            _particles  = ParticleEffectsManager.Create(vfxRoot);
            _highlight  = HighlightOverlay.Create(vfxRoot);

            _floatText.Initialize(bus, gc);
            _particles.Initialize(bus, gc);

            // Camera shake — attach to main camera
            var cam = Camera.main;
            if (cam != null)
            {
                _cameraShake = cam.GetComponent<CameraShake>()
                    ?? cam.gameObject.AddComponent<CameraShake>();
                _cameraShake.Initialize(bus);
            }

            // Show highlight on sector selection
            bus.Subscribe<Simulation.SectorConqueredEvent>(_ => _highlight.Hide());

            // Carrier speech bubble (§14.1) when player production stalls on
            // missing goods — throttled so simultaneous stalls don't spam
            bus.Subscribe<Simulation.ProductionStalledEvent>(e =>
            {
                if (e.OwnerId != 0) return;
                if (Time.time - _lastStallBubbleTime < 4f) return;
                _lastStallBubbleTime = Time.time;
                _floatText.Spawn(gc.GetSectorPosition(e.SectorId),
                    Simulation.L.Get("ui.carrier.waiting"),
                    new Color(0.95f, 0.9f, 0.7f));
            });
        }

        private float _lastStallBubbleTime = -10f;

        /// <summary>Show pulsing highlight ring at the given world position.</summary>
        public void ShowSectorHighlight(Vector3 worldPos) =>
            _highlight?.Show(worldPos);

        /// <summary>Hide the sector highlight ring.</summary>
        public void HideSectorHighlight() =>
            _highlight?.Hide();
    }
}
