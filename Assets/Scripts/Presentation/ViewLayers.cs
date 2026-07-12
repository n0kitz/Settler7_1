using UnityEngine;

namespace Settlers.Presentation
{
    /// <summary>
    /// Detail-culling layers (Sprint 8a, the 60 fps bar): unit figures and
    /// building detail are tiny at sector-overview zoom, yet thousands of
    /// their renderers were submitted every frame. The camera culls these
    /// layers by distance instead — landmarks, terrain and walls (default
    /// layer) carry the overview reading, exactly like the original's
    /// zoomed-out view.
    /// </summary>
    public static class ViewLayers
    {
        /// <summary>Layer for unit figures (workers, carriers, clerics, armies).</summary>
        public const int UNITS = 30;
        /// <summary>Layer for building views (bases + detail parts).</summary>
        public const int BUILDINGS = 29;

        /// <summary>Units vanish beyond this camera distance (≈ 1 m figures).</summary>
        public const float UNIT_CULL_DISTANCE = 70f;
        /// <summary>Building views vanish beyond this camera distance.</summary>
        public const float BUILDING_CULL_DISTANCE = 260f;

        /// <summary>Assign a GameObject and all children to a layer.</summary>
        public static void SetLayerRecursive(GameObject go, int layer)
        {
            go.layer = layer;
            var t = go.transform;
            for (int i = 0; i < t.childCount; i++)
                SetLayerRecursive(t.GetChild(i).gameObject, layer);
        }

        /// <summary>
        /// Disable shadow casting on all renderers under a root. Unit figures
        /// are ~0.3 m primitives — their shadows are invisible at game zoom but
        /// double the renderer cost of every figure.
        /// </summary>
        public static void DisableShadows(GameObject go)
        {
            var renderers = go.GetComponentsInChildren<MeshRenderer>(true);
            for (int i = 0; i < renderers.Length; i++)
                renderers[i].shadowCastingMode =
                    UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        /// <summary>Apply per-layer cull distances to the given camera.</summary>
        public static void ApplyCullDistances(Camera cam)
        {
            if (cam == null) return;
            var distances = cam.layerCullDistances; // float[32], 0 = far plane
            distances[UNITS] = UNIT_CULL_DISTANCE;
            distances[BUILDINGS] = BUILDING_CULL_DISTANCE;
            cam.layerCullDistances = distances;
            cam.layerCullSpherical = true;
        }
    }
}
