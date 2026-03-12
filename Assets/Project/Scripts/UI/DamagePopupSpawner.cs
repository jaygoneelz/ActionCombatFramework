using ActionCombat.Combat;
using UnityEngine;

namespace ActionCombat.UI
{
    /// <summary>
    /// Spawns damage popups when any HealthComponent takes damage.
    /// Attach to a manager object. Needs a prefab reference.
    /// </summary>
    public class DamagePopupSpawner : MonoBehaviour
    {
        private static DamagePopupSpawner instance;

        [SerializeField] private GameObject damagePopupPrefab;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
        }

        public static void Spawn(float damage, Vector3 position, DamageType type)
        {
            if (instance == null || instance.damagePopupPrefab == null) return;

            // Add slight random offset so numbers don't stack
            Vector3 offset = new Vector3(
                Random.Range(-0.3f, 0.3f),
                Random.Range(0f, 0.3f),
                Random.Range(-0.3f, 0.3f));

            GameObject popup = Instantiate(instance.damagePopupPrefab, position + offset, Quaternion.identity);
            DamagePopup popupScript = popup.GetComponent<DamagePopup>();

            if (popupScript != null)
            {
                popupScript.Setup(damage, position + offset, type);
            }
        }

        private void OnDestroy()
        {
            if (instance == this) instance = null;
        }
    }
}