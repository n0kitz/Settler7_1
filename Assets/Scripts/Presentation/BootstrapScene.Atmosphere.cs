using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Settlers.Presentation
{
    /// <summary>
    /// Partial: atmosphere per §14.10 — "dreamy fairy tale" look. Warm ambient
    /// sky with cool shadow tones, distance fog, and a runtime URP global
    /// volume (color grading, bloom, vignette).
    /// </summary>
    public partial class BootstrapScene
    {
        private static readonly Color SKY_HORIZON = new(0.62f, 0.74f, 0.84f);
        private static readonly Color AMBIENT_SKY = new(0.42f, 0.39f, 0.31f);
        private static readonly Color AMBIENT_EQUATOR = new(0.28f, 0.30f, 0.36f);
        private static readonly Color AMBIENT_GROUND = new(0.18f, 0.20f, 0.25f);
        private static readonly Color FOG_COLOR = new(0.74f, 0.72f, 0.60f);
        private const float FOG_START = 130f;
        private const float FOG_END = 450f;

        private const float GRADE_SATURATION = 12f;
        private static readonly Color GRADE_FILTER = new(1f, 0.98f, 0.92f);
        private const float GRADE_TEMPERATURE = 8f;
        private const float BLOOM_INTENSITY = 0.25f;
        private const float BLOOM_THRESHOLD = 1.1f;
        private const float VIGNETTE_INTENSITY = 0.16f;

        /// <summary>Call after CreateCamera — needs Camera.main to exist.</summary>
        private void SetupAtmosphere()
        {
            // Warm-lit / cool-shadow split (§14.10 art direction)
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = AMBIENT_SKY;
            RenderSettings.ambientEquatorColor = AMBIENT_EQUATOR;
            RenderSettings.ambientGroundColor = AMBIENT_GROUND;

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = FOG_COLOR;
            RenderSettings.fogStartDistance = FOG_START;
            RenderSettings.fogEndDistance = FOG_END;

            var cam = Camera.main;
            if (cam != null)
            {
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = SKY_HORIZON;
                cam.GetUniversalAdditionalCameraData().renderPostProcessing = true;
            }

            CreateGlobalVolume();
        }

        private void CreateGlobalVolume()
        {
            var go = new GameObject("GlobalVolume");
            var volume = go.AddComponent<Volume>();
            volume.isGlobal = true;

            var profile = ScriptableObject.CreateInstance<VolumeProfile>();

            // Neutral tonemapping rolls off highlights so sunlit ground can't blow out
            var tonemapping = profile.Add<Tonemapping>();
            tonemapping.mode.Override(TonemappingMode.Neutral);

            var color = profile.Add<ColorAdjustments>();
            color.saturation.Override(GRADE_SATURATION);
            color.colorFilter.Override(GRADE_FILTER);

            var whiteBalance = profile.Add<WhiteBalance>();
            whiteBalance.temperature.Override(GRADE_TEMPERATURE);

            var bloom = profile.Add<Bloom>();
            bloom.intensity.Override(BLOOM_INTENSITY);
            bloom.threshold.Override(BLOOM_THRESHOLD);

            var vignette = profile.Add<Vignette>();
            vignette.intensity.Override(VIGNETTE_INTENSITY);

            volume.profile = profile;
        }
    }
}
