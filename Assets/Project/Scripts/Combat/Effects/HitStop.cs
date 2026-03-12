using System.Collections;
using UnityEngine;

namespace ActionCombat.Combat.Effects
{
    /// <summary>
    /// Freezes time briefly on hit to sell impact. AAA staple.
    /// Call HitStop.Trigger() from anywhere — it's a singleton.
    /// </summary>
    public class HitStop : MonoBehaviour
    {
        private static HitStop instance;

        [Header("Settings")]
        [SerializeField] private float lightHitDuration = 0.04f;
        [SerializeField] private float heavyHitDuration = 0.08f;
        [SerializeField] private float timeScaleDuringStop = 0.05f;

        private Coroutine activeCoroutine;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
        }

        /// <summary>
        /// Call this when a hit connects.
        /// </summary>
        public static void Trigger(DamageType type = DamageType.Light)
        {
            if (instance == null) return;

            float duration = type == DamageType.Heavy
                ? instance.heavyHitDuration
                : instance.lightHitDuration;

            instance.DoHitStop(duration);
        }

        /// <summary>
        /// Call with custom duration.
        /// </summary>
        public static void Trigger(float customDuration)
        {
            if (instance == null) return;
            instance.DoHitStop(customDuration);
        }

        private void DoHitStop(float duration)
        {
            if (activeCoroutine != null)
                StopCoroutine(activeCoroutine);

            activeCoroutine = StartCoroutine(HitStopRoutine(duration));
        }

        private IEnumerator HitStopRoutine(float duration)
        {
            Time.timeScale = timeScaleDuringStop;

            // Wait in real time (unscaled) so the pause feels consistent
            yield return new WaitForSecondsRealtime(duration);

            Time.timeScale = 1f;
            activeCoroutine = null;
        }

        private void OnDestroy()
        {
            // Safety: always restore time scale
            Time.timeScale = 1f;
            if (instance == this) instance = null;
        }
    }
}