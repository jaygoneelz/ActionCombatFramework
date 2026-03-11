using UnityEngine;

namespace ActionCombat.Combat
{
    /// <summary>
    /// Visual-only component that draws the hurtbox (the area that
    /// CAN be hit) in the Scene view. The actual hit detection is done
    /// by the Collider on this GameObject — this just adds the gizmo.
    /// 
    /// Attach to any entity that can receive damage alongside a Collider.
    /// </summary>
    public class HurtboxController : MonoBehaviour
    {
        [SerializeField] private Color gizmoColor = new Color(0f, 1f, 0f, 0.3f);

        private Collider hurtboxCollider;

        private void Awake()
        {
            hurtboxCollider = GetComponent<Collider>();
        }

        private void OnDrawGizmos()
        {
            Collider col = hurtboxCollider != null ? hurtboxCollider : GetComponent<Collider>();
            if (col == null) return;

            Gizmos.color = gizmoColor;

            if (col is CapsuleCollider capsule)
            {
                // Draw approximate capsule shape
                Vector3 center = transform.TransformPoint(capsule.center);
                Gizmos.DrawWireSphere(center + Vector3.up * (capsule.height * 0.5f - capsule.radius), capsule.radius);
                Gizmos.DrawWireSphere(center - Vector3.up * (capsule.height * 0.5f - capsule.radius), capsule.radius);
            }
            else if (col is BoxCollider box)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawWireCube(box.center, box.size);
                Gizmos.matrix = Matrix4x4.identity;
            }
            else if (col is SphereCollider sphere)
            {
                Vector3 center = transform.TransformPoint(sphere.center);
                Gizmos.DrawWireSphere(center, sphere.radius);
            }
        }
    }
}