using UnityEngine;

namespace ActionCombat.Player
{
    /// <summary>
    /// Handles all physical character movement. States call methods
    /// on this class — they don't move the character directly.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float sprintSpeed = 9f;
        [SerializeField] private float rotationSpeed = 10f;
        [SerializeField] private float acceleration = 10f;

        [Header("Jump & Gravity")]
        [SerializeField] private float jumpForce = 8f;
        [SerializeField] private float gravity = -20f;
        [SerializeField] private float groundCheckRadius = 0.3f;
        [SerializeField] private LayerMask groundLayer;

        [Header("References")]
        [SerializeField] private Transform cameraTransform;

        public bool IsGrounded { get; private set; }
        public bool IsMoving => currentSpeed > 0.1f;
        public float CurrentSpeed => currentSpeed;
        public Vector3 Velocity => characterController.velocity;
        public float NormalisedSpeed => currentSpeed / sprintSpeed;

        private CharacterController characterController;
        private Vector3 verticalVelocity;
        private float currentSpeed;
        private Vector3 currentMoveDirection;
        private Vector3 dampVelocity;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();

            if (cameraTransform == null && Camera.main != null)
                cameraTransform = Camera.main.transform;
        }

        private void Update()
        {
            UpdateGroundCheck();
            ApplyGravity();
        }

        public void Move(Vector2 input, bool sprint = false)
        {
            if (input.sqrMagnitude < 0.01f)
            {
                Decelerate();
                return;
            }

            Vector3 cameraForward = cameraTransform.forward;
            Vector3 cameraRight = cameraTransform.right;
            cameraForward.y = 0f;
            cameraRight.y = 0f;
            cameraForward.Normalize();
            cameraRight.Normalize();

            Vector3 targetDirection = (cameraForward * input.y + cameraRight * input.x).normalized;

            currentMoveDirection = Vector3.SmoothDamp(
                currentMoveDirection, targetDirection, ref dampVelocity, 0.05f);

            float targetSpeed = sprint ? sprintSpeed : moveSpeed;
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.deltaTime);

            Vector3 move = currentMoveDirection * currentSpeed;
            characterController.Move((move + verticalVelocity) * Time.deltaTime);

            if (currentMoveDirection.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(currentMoveDirection);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }

        public void Decelerate()
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, acceleration * 2f * Time.deltaTime);

            if (currentSpeed > 0.01f)
            {
                Vector3 move = currentMoveDirection * currentSpeed;
                characterController.Move((move + verticalVelocity) * Time.deltaTime);
            }
            else
            {
                characterController.Move(verticalVelocity * Time.deltaTime);
            }
        }

        public void ApplyRootMotion(Vector3 displacement)
        {
            characterController.Move(displacement + verticalVelocity * Time.deltaTime);
        }

        public bool TryJump()
        {
            if (!IsGrounded) return false;
            verticalVelocity.y = jumpForce;
            return true;
        }

        public void RotateTowards(Vector3 targetPosition)
        {
            Vector3 direction = (targetPosition - transform.position);
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }

        private void UpdateGroundCheck()
        {
            Vector3 origin = transform.position + Vector3.up * 0.1f;
            IsGrounded = Physics.CheckSphere(origin, groundCheckRadius, groundLayer);
        }

        private void ApplyGravity()
        {
            if (IsGrounded && verticalVelocity.y < 0f)
            {
                verticalVelocity.y = -2f;
            }
            else
            {
                verticalVelocity.y += gravity * Time.deltaTime;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = IsGrounded ? Color.green : Color.red;
            Vector3 origin = transform.position + Vector3.up * 0.1f;
            Gizmos.DrawWireSphere(origin, groundCheckRadius);
        }
    }
}