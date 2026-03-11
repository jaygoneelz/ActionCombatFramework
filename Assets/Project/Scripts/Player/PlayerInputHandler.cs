using UnityEngine;
using UnityEngine.InputSystem;

namespace ActionCombat.Player
{
    /// <summary>
    /// Reads raw input from Unity's New Input System and exposes
    /// processed, frame-safe input state. Other systems poll this —
    /// they never read input directly.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class PlayerInputHandler : MonoBehaviour
    {
        // --- Continuous Input (polled every frame) ---
        public Vector2 MoveInput { get; private set; }
        public Vector2 LookInput { get; private set; }
        public bool IsBlocking { get; private set; }

        // --- Buffered Input (consumed on read) ---
        private bool lightAttackQueued;
        private bool heavyAttackQueued;
        private bool dodgeQueued;
        private bool jumpQueued;
        private bool lockOnQueued;

        private float lightAttackBufferTime;
        private float heavyAttackBufferTime;
        private float dodgeBufferTime;
        private float jumpBufferTime;

        [SerializeField] private float inputBufferDuration = 0.15f;

        private PlayerInputActions inputActions;

        private void Awake()
        {
            inputActions = new PlayerInputActions();
        }

        private void OnEnable()
        {
            inputActions.Player.Enable();

            inputActions.Player.LightAttack.performed += OnLightAttack;
            inputActions.Player.HeavyAttack.performed += OnHeavyAttack;
            inputActions.Player.Dodge.performed += OnDodge;
            inputActions.Player.Jump.performed += OnJump;
            inputActions.Player.LockOn.performed += OnLockOn;
        }

        private void OnDisable()
        {
            inputActions.Player.LightAttack.performed -= OnLightAttack;
            inputActions.Player.HeavyAttack.performed -= OnHeavyAttack;
            inputActions.Player.Dodge.performed -= OnDodge;
            inputActions.Player.Jump.performed -= OnJump;
            inputActions.Player.LockOn.performed -= OnLockOn;

            inputActions.Player.Disable();
        }

        private void Update()
        {
            MoveInput = inputActions.Player.Move.ReadValue<Vector2>();
            LookInput = inputActions.Player.Look.ReadValue<Vector2>();
            IsBlocking = inputActions.Player.Block.IsPressed();

            float time = Time.time;
            if (lightAttackQueued && time - lightAttackBufferTime > inputBufferDuration)
                lightAttackQueued = false;
            if (heavyAttackQueued && time - heavyAttackBufferTime > inputBufferDuration)
                heavyAttackQueued = false;
            if (dodgeQueued && time - dodgeBufferTime > inputBufferDuration)
                dodgeQueued = false;
            if (jumpQueued && time - jumpBufferTime > inputBufferDuration)
                jumpQueued = false;
        }

        public bool ConsumeLightAttack()
        {
            if (!lightAttackQueued) return false;
            lightAttackQueued = false;
            return true;
        }

        public bool ConsumeHeavyAttack()
        {
            if (!heavyAttackQueued) return false;
            heavyAttackQueued = false;
            return true;
        }

        public bool ConsumeDodge()
        {
            if (!dodgeQueued) return false;
            dodgeQueued = false;
            return true;
        }

        public bool ConsumeJump()
        {
            if (!jumpQueued) return false;
            jumpQueued = false;
            return true;
        }

        public bool ConsumeLockOn()
        {
            if (!lockOnQueued) return false;
            lockOnQueued = false;
            return true;
        }

        public bool HasBufferedAttack => lightAttackQueued || heavyAttackQueued;

        private void OnLightAttack(InputAction.CallbackContext ctx)
        {
            lightAttackQueued = true;
            lightAttackBufferTime = Time.time;
        }

        private void OnHeavyAttack(InputAction.CallbackContext ctx)
        {
            heavyAttackQueued = true;
            heavyAttackBufferTime = Time.time;
        }

        private void OnDodge(InputAction.CallbackContext ctx)
        {
            dodgeQueued = true;
            dodgeBufferTime = Time.time;
        }

        private void OnJump(InputAction.CallbackContext ctx)
        {
            jumpQueued = true;
            jumpBufferTime = Time.time;
        }

        private void OnLockOn(InputAction.CallbackContext ctx)
        {
            lockOnQueued = true;
        }
    }
}