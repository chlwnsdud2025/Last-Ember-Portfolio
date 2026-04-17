using UnityEngine;

public class Skeleton_Guard_Controller : BossBaseController
{
    //  부모(BossBaseController)에 없는 이 보스만의 고유 상태들
    public BossBaseState runState { get; private set; }
    public BossBaseState strafeState { get; private set; }
    public BossBaseState dodgeState { get; private set; }
    public BossBaseState stunState { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        // 1. 상태 레고 블록 생성 (메모리 할당)
        decisionState = new BossDecisionState(this, stateMachine);
        idleState = new Humanoid_Boss_Idle_State(this, stateMachine);
        chaseState = new Humanoid_Boss_Walk_State(this, stateMachine);
        runState = new Humanoid_Boss_Run_State(this, stateMachine);
        strafeState = new Humanoid_Boss_Strafe_State(this, stateMachine);
        dodgeState = new Humanoid_Boss_Dodge_State(this, stateMachine);
        stunState = new Humanoid_Boss_Stun_State(this, stateMachine);

        // -------------------------------------------------------------
        //  [추가] 인스펙터에서 만든 옵션들에 '진짜 State'를 연결해줍니다!
        // -------------------------------------------------------------
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
            }
        }

        //  이제 코드에서 하드코딩으로 availableActions.Add(...) 하던 부분은 
        // 전부 지우시거나 주석 처리하시면 됩니다! 
        // 오직 인스펙터에서 추가한 데이터만 깔끔하게 사용하게 됩니다.
    }

    protected override void Start()
    {
        base.Start();

        // 3. 게임이 시작되면 가장 먼저 '생각(Decision)'부터 하도록 시작!
        if (decisionState != null)
        {
            stateMachine.Initialize(decisionState);
        }
    }

    protected override void Update()
    {
        base.Update();
    }
}