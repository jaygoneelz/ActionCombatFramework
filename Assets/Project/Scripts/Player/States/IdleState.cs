using ActionCombat.Core.StateMachine;

namespace ActionCombat.Player.States
{
    public class IdleState : PlayerState
    {
        public IdleState(StateMachine stateMachine, PlayerController player)
            : base("Idle", stateMachine, player) { }

        public override void Enter()
        {
            base.Enter();
            animator.PlayAnimation("Idle");
            animator.SetFloat("Speed", 0f);
        }

        public override void Execute()
        {
            base.Execute();
            movement.Decelerate();
        }
    }
}