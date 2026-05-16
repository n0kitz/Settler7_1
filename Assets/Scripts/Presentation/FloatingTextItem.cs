using System.Collections;
using UnityEngine;
using TMPro;

namespace Settlers.Presentation
{
    /// <summary>
    /// A single floating text label that rises upward and fades out over its lifetime.
    /// Returned to the pool when done. Created by FloatingTextManager.
    /// </summary>
    public class FloatingTextItem : MonoBehaviour
    {
        private TextMeshPro _tmp;
        private float _lifetime;
        private float _riseSpeed;

        private void Awake()
        {
            _tmp = gameObject.AddComponent<TextMeshPro>();
            _tmp.fontSize       = 4f;
            _tmp.alignment      = TextAlignmentOptions.Center;
            _tmp.sortingOrder   = 5;
        }

        /// <summary>
        /// Show this item at a world position with the given text, color, and lifetime.
        /// </summary>
        public void Play(Vector3 worldPos, string text, Color color,
            float lifetime = 1.6f, float riseSpeed = 2.5f)
        {
            transform.position = worldPos + Vector3.up * 0.5f;
            _tmp.text          = text;
            _tmp.color         = color;
            _lifetime          = lifetime;
            _riseSpeed         = riseSpeed;
            gameObject.SetActive(true);
            StopAllCoroutines();
            StartCoroutine(Animate());
        }

        private IEnumerator Animate()
        {
            float elapsed = 0f;
            var startColor = _tmp.color;

            while (elapsed < _lifetime)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / _lifetime;

                // Rise
                transform.position += Vector3.up * (_riseSpeed * Time.deltaTime);

                // Fade out in second half
                float alpha = t < 0.5f ? 1f : 1f - ((t - 0.5f) * 2f);
                _tmp.color = new Color(startColor.r, startColor.g, startColor.b, alpha);

                yield return null;
            }

            gameObject.SetActive(false);
        }
    }
}
