using ActionCombat.Combat;
using ActionCombat.Core.StateMachine;
using UnityEngine;
using SM = ActionCombat.Core.StateMachine.StateMachine;

namespace ActionCombat.AI
{
    /// <summary>
    /// Enemy orchestrator. Same state machine pattern as player.
    /// Manages AI states: Idle, Patrol, Chase, Attack, Stagger, Death.
    /// </summary>
    [RequireComponent(typeof(HealthComponent))]
    public class EnemyController : MonoBehaviour
    {
        [Header("AI Settings")]
        [SerializeField] private float detectionRange = 10f;
        [SerializeField] private float attackRange = 2f;
        [SerializeField] private float moveSpeed = 3.5f;
        [SerializeField] private float rotationSpeed = 8f;
        [SerializeField] private float attackCooldown = 1.5f;

        [Header("References")]
        [SerializeField] private HitboxController hitbox;

        // Public access for states
        public float DetectionRange => detectionRange;
        public float AttackRange => attackRange;
        public float MoveSpeed => moveSpeed;
        public float RotationSpeed => rotationSpeed;
        public float AttackCooldown => attackCooldown;
        public HitboxController Hitbox => hitbox;
        public HealthComponent Health { get; private set; }
        public Transform Target { get; set; }
        public CharacterController CharController { get; private set; }
        public SM DebugStateMachine => stateMachine;

        private SM stateMachine;

        // States
        public EnemyIdleState IdleState { get; private set; }
        public EnemyChaseState ChaseState { get; private set; }
        public EnemyAttackState AttackState { get; private set; }
        public EnemyStaggerState StaggerState { get; private set; }
        public EnemyDeathState DeathState { get; private set; }

        // Shared timers
        public float LastAttackTime { get; set; }

        private void Awake()
        {
            Health = GetComponent<HealthComponent>();
            CharController = GetComponent<CharacterController>();

            Health.OnDamaged += OnDamaged;
            Health.OnDeath += OnDeath;

            InitialiseStateMachine();
        }

        private void Start()
        {
            // Find the player
            var player = FindObjectOfType<ActionCombat.Player.PlayerController>();
            if (player != null)
                Target = player.transform;
        }

        private void InitialiseStateMachine()
        {
            stateMachine = new SM();

            IdleState = new EnemyIdleState(stateMachine, this);
            ChaseState = new EnemyChaseState(stateMachine, this);
            AttackState = new EnemyAttackState(stateMachine, this);
            StaggerState = new EnemyStaggerState(stateMachine, this);
            DeathState = new EnemyDeathState(stateMachine, this);

            // Idle: detect player → chase
            IdleState.AddTransition(new Transition(
                "Idle→Chase", ChaseState,
                () => Target != null && DistanceToTarget() < detectionRange));

            // Chase: in range → attack, lost target → idle
            ChaseState.AddTransition(new Transition(
                "Chase→Attack", AttackState,
                () => DistanceToTarget() < attackRange &&
                      Time.time - LastAttackTime > attackCooldown));

            ChaseState.AddTransition(new Transition(
                "Chase→Idle", IdleState,
                () => Target == null || DistanceToTarget() > detectionRange * 1.5f));

            // Attack: complete → chase
            AttackState.AddTransition(new Transition(
                "Attack→Chase", ChaseState,
                () => AttackState.IsComplete));

            // Stagger: complete → chase
            StaggerState.AddTransition(new Transition(
                "Stagger→Chase", ChaseState,
                () => StaggerState.IsComplete && Health.IsAlive));

            stateMachine.Initialise(IdleState);
        }

        private void Update()
        {
            if (!Health.IsAlive) return;
            stateMachine.Update();
        }

        public float DistanceToTarget()
        {
            if (Target == null) return float.MaxValue;
            return Vector3.Distance(transform.position, Target.position);
        }

        public void RotateTowardsTarget()
        {
            if (Target == null) return;
            Vector3 direction = (Target.position - transform.position);
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.01f) return;

            Quaternion targetRot = Quaternion.LookRotation(direction.normalized);
            transform.rotation = Quaternion.Slerp(
                transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        public void MoveTowardsTarget()
        {
            if (Target == null || CharController == null) return;

            Vector3 direction = (Target.position - transform.position);
            direction.y = 0f;
            direction.Normalize();

            Vector3 move = direction * moveSpeed * Time.deltaTime;
            move.y = -9.81f * Time.deltaTime; // gravity
            CharController.Move(move);
        }

        private void OnDamaged(DamageData data)
        {
            if (!Health.IsAlive) return;
            StaggerState.SetStaggerDuration(data.StaggerDuration);
            stateMachine.ForceState(StaggerState, "TookDamage");
        }

        private void OnDeath()
        {
            stateMachine.ForceState(DeathState, "Died");
        }

        private void OnDrawGizmosSelected()
        {
            // Detection range
            Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
            Gizmos.DrawWireSphere(transform.position, detectionRange);

            // Attack range
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }

        private void OnDestroy()
        {
            if (Health != null)
            {
                Health.OnDamaged -= OnDamaged;
                Health.OnDeath -= OnDeath;
            }
        }
    }
}