using ActionCombat.Core.StateMachine;

namespace ActionCombat.Player.States
{
    public class JumpState : PlayerState
    {
        private bool hasLanded;

        public JumpState(StateMachine stateMachine, PlayerController player)
            : base("Jump", stateMachine, player) { }

        public override void Enter()
        {
            base.Enter();
            hasLanded = false;
            movement.TryJump();
            animator.PlayAnimation("Jump");
            animator.SetBool("IsGrounded", false);
        }

        public override void Execute()
        {
            base.Execute();

            if (input.MoveInput.sqrMagnitude > 0.01f)
            {
                movement.Move(input.MoveInput);
            }

            animator.SetFloat("VerticalVelocity", movement.Velocity.y);

            if (TimeInState > 0.1f && movement.IsGrounded)
            {
                hasLanded = true;
            }
        }

        public bool HasLanded => hasLanded;

        public override void Exit()
        {
            base.Exit();
            animator.SetBool("IsGrounded", true);
        }
    }
}