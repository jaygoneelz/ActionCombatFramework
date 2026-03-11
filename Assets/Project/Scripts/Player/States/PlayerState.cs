using ActionCombat.Core.StateMachine;

namespace ActionCombat.Player.States
{
    /// <summary>
    /// Base class for all player states. Caches shared references.
    /// </summary>
    public abstract class PlayerState : State
    {
        protected PlayerController player;
        protected PlayerMovement movement;
        protected PlayerInputHandler input;
        protected PlayerAnimator animator;

        public PlayerState(string name, StateMachine stateMachine,
            PlayerController player) : base(name, stateMachine)
        {
            this.player = player;
            this.movement = player.Movement;
            this.input = player.Input;
            this.animator = player.Animator;
        }
    }
}