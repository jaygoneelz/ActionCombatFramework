using UnityEngine;
using ActionCombat.Combat;
using TMPro;

namespace ActionCombat.UI
{
    /// <summary>
    /// Floating damage number that spawns at hit point,
    /// floats upward, and fades out. Destroys itself after lifetime.
    /// </summary>
    public class DamagePopup : MonoBehaviour
    {
        [SerializeField] public TextMeshPro textMesh;
        [SerializeField] private float floatSpeed = 2f;
        [SerializeField] private float lifetime = 0.8f;
        [SerializeField] private float scaleUpAmount = 1.5f;

        private float timer;
        private Color startColor;
        private Vector3 startScale;

        /// <summary>
        /// Call this right after Instantiate to configure the popup.
        /// </summary>
        public void Setup(float damage, Vector3 position, DamageType type)
        {
            transform.position = position + Vector3.up * 1.5f;

            if (textMesh == null)
                textMesh = GetComponent<TextMeshPro>();

            textMesh.text = Mathf.RoundToInt(damage).ToString();

            // Color by damage type
            switch (type)
            {
                case DamageType.Light:
                    textMesh.color = Color.white;
                    break;
                case DamageType.Heavy:
                    textMesh.color = new Color(1f, 0.7f, 0f); // orange
                    textMesh.fontSize = 8f;
                    break;
                case DamageType.Special:
                    textMesh.color = new Color(1f, 0.2f, 0.2f); // red
                    textMesh.fontSize = 10f;
                    break;
            }

            startColor = textMesh.color;
            startScale = transform.localScale;
        }

        private void Update()
        {
            timer += Time.deltaTime;
            float normalisedTime = timer / lifetime;

            // Float upward
            transform.position += Vector3.up * floatSpeed * Time.deltaTime;

            // Scale up slightly then back down
            float scaleMultiplier = 1f + Mathf.Sin(normalisedTime * Mathf.PI) * (scaleUpAmount - 1f);
            transform.localScale = startScale * scaleMultiplier;

            // Fade out in second half
            if (normalisedTime > 0.5f)
            {
                float alpha = Mathf.Lerp(1f, 0f, (normalisedTime - 0.5f) / 0.5f);
                textMesh.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            }

            // Always face camera
            if (Camera.main != null)
                transform.forward = Camera.main.transform.forward;

            if (timer >= lifetime)
            {
                Destroy(gameObject);
            }
        }
    }
}