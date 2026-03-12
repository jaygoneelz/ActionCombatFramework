using System.Collections;
using UnityEngine;
using Cinemachine;

namespace ActionCombat.Combat.Effects
{
    /// <summary>
    /// Triggers Cinemachine impulse for screen shake.
    /// Call ScreenShake.Trigger() from anywhere.
    /// </summary>
    public class ScreenShake : MonoBehaviour
    {
        private static ScreenShake instance;

        [Header("Settings")]
        [SerializeField] private float lightShakeForce = 0.5f;
        [SerializeField] private float heavyShakeForce = 1.2f;

        private CinemachineImpulseSource impulseSource;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;

            impulseSource = GetComponent<CinemachineImpulseSource>();
            if (impulseSource == null)
                impulseSource = gameObject.AddComponent<CinemachineImpulseSource>();
        }

        public static void Trigger(DamageType type = DamageType.Light)
        {
            if (instance == null || instance.impulseSource == null) return;

            float force = type == DamageType.Heavy
                ? instance.heavyShakeForce
                : instance.lightShakeForce;

            instance.impulseSource.GenerateImpulse(force);
        }

        public static void Trigger(float customForce)
        {
            if (instance == null || instance.impulseSource == null) return;
            instance.impulseSource.GenerateImpulse(customForce);
        }

        private void OnDestroy()
        {
            if (instance == this) instance = null;
        }
    }
}