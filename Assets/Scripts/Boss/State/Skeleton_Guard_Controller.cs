using UnityEngine;

public class Skeleton_Guard_Controller : BossBaseController
{
    public BossBaseState runState { get; private set; }
    public BossBaseState strafeState { get; private set; }
    public BossBaseState dodgeState { get; private set; }
    public BossBaseState stunState { get; private set; }


    protected override void Awake()
    {
        base.Awake();

        decisionState = new BossDecisionState(this, stateMachine);
        idleState = new Humanoid_Boss_Idle_State(this, stateMachine);
        chaseState = new Humanoid_Boss_Walk_State(this, stateMachine);
        runState = new Humanoid_Boss_Run_State(this, stateMachine);
        strafeState = new Humanoid_Boss_Strafe_State(this, stateMachine);
        dodgeState = new Humanoid_Boss_Dodge_State(this, stateMachine);
        stunState = new Humanoid_Boss_Stun_State(this, stateMachine);
        attackState = new Humanoid_Boss_Attack_State(this, stateMachine);
        phaseChangeState = new Humanoid_Boss_PhaseChange_State(this, stateMachine);
        sleepState = new Humanoid_Boss_Sleep_State(this, stateMachine);
        deadState = new Humanoid_Boss_Dead_State(this, stateMachine);

        if (animator != null)
        {
            overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
            animator.runtimeAnimatorController = overrideController;
        }

        foreach (var action in availableActions)
        {
            switch (action.stateType)
            {
                case BossStateType.Run: action.targetState = runState; break;
                case BossStateType.Strafe: action.targetState = strafeState; break;
                case BossStateType.Dodge: action.targetState = dodgeState; break;
                case BossStateType.Stun: action.targetState = stunState; break;
                case BossStateType.Walk: action.targetState = chaseState; break;
                case BossStateType.Idle: action.targetState = idleState; break;
                case BossStateType.Attack: action.targetState = attackState; break;
            }
        }

     
    }

    protected override void Start()
    {
        base.Start();

        if (sleepState != null)
        {
            stateMachine.Initialize(sleepState);
        }
    }

    protected override void Update()
    {
        base.Update();
    }
}