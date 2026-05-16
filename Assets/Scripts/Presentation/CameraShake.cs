using System.Collections;
using UnityEngine;
using Settlers.Simulation;

namespace Settlers.Presentation
{
    /// <summary>
    /// Applies a brief randomized position shake to the main camera.
    /// Triggered by combat and building destruction events.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CameraShake : MonoBehaviour
    {
        private Vector3 _originalOffset;
        private bool _shaking;

        public void Initialize(EventBus bus)
        {
            bus.Subscribe<CombatResolvedEvent>(_ => Shake(0.15f, 0.25f));
            bus.Subscribe<BuildingDestroyedEvent>(_ => Shake(0.08f, 0.15f));
        }

        /// <summary>Trigger a shake of given magnitude for duration seconds.</summary>
        public void Shake(float magnitude, float duration)
        {
            if (_shaking) return; // don't interrupt ongoing shake
            StartCoroutine(DoShake(magnitude, duration));
        }

        private IEnumerator DoShake(float magnitude, float duration)
        {
            _shaking = true;
            var cam = GetComponent<SettlerCamera>();
            if (cam == null) { _shaking = false; yield break; }

            Vector3 origin = transform.localPosition;
            float elapsed  = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float ratio     = 1f - (elapsed / duration);
                float offsetX   = Random.Range(-1f, 1f) * magnitude * ratio;
                float offsetY   = Random.Range(-1f, 1f) * magnitude * ratio * 0.5f;
                transform.localPosition = origin + new Vector3(offsetX, offsetY, 0f);
                yield return null;
            }

            transform.localPosition = origin;
            _shaking = false;
        }
    }
}
