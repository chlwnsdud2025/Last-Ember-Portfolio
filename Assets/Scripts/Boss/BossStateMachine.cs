using UnityEngine;

public class BossStateMachine
{
    // 현재 작동 중인 보스의 상태
    public BossBaseState CurrentState { get; private set; }

    // 처음 시작 상태를 설정할 때 사용
    public void Initialize(BossBaseState startingState)
    {
        CurrentState = startingState;
        CurrentState.Enter();
    }

    // 새로운 상태로 갈아끼울 때 사용
    public void ChangeState(BossBaseState newState)
    {
        if (CurrentState == null) return;

        CurrentState.Exit();
        CurrentState = newState;
        CurrentState.Enter();
    }

    // 보스 컨트롤러의 Update에서 매 프레임 호출해주어야 함
    public void Update()
    {
        if (CurrentState != null)
        {
            CurrentState.Update();
        }
    }
}