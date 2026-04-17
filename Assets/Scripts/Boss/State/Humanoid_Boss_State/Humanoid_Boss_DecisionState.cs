using System.Collections.Generic;
using UnityEngine;

public class BossDecisionState : BossBaseState
{
    public BossDecisionState(BossBaseController boss, BossStateMachine stateMachine) : base(boss, stateMachine) { }

    public override void Enter()
    {
        // 진입하자마자 생각하고, 바로 다음 상태로 전환합니다.
        BossBaseState nextState = ThinkAndDecide();

        if (nextState != null)
        {
            stateMachine.ChangeState(nextState);
        }
        else
        {
            // 만약 아무 조건도 맞지 않는다면 기본 상태(예: Idle이나 Strafe)로 강제 전환
            Debug.LogWarning("조건에 맞는 행동이 없습니다! 기본 대기 상태로 전환합니다.");
            stateMachine.ChangeState(boss.chaseState);
        }
    }

    private BossBaseState ThinkAndDecide()
    {
        // 보스와 플레이어 사이의 현재 거리 측정
        float distanceToPlayer = Vector3.Distance(boss.transform.position, boss.target.position);

        List<BossActionOption> validOptions = new List<BossActionOption>();
        int totalWeight = 0;

        // 1. 조건 필터링 (행동 트리 / 유틸리티 평가)
        // boss.availableActions는 BossBaseController에 정의해둔 BossActionOption 리스트입니다.
        foreach (var option in boss.availableActions)
        {
            // 거리 조건 확인
            if (distanceToPlayer < option.minDistance || distanceToPlayer > option.maxDistance)
                continue;

            // 쿨타임 조건 확인
            if (Time.time < option.lastUsedTime + option.cooldown)
                continue;

            // 모든 조건을 통과했다면 후보에 추가하고, 룰렛 가중치를 누적합니다.
            validOptions.Add(option);
            totalWeight += option.baseWeight;
        }

        // 유효한 옵션이 없으면 null 반환
        if (validOptions.Count == 0) return null;

        // 2. 가중치 랜덤 선택 (룰렛 돌리기)
        int randomValue = Random.Range(0, totalWeight);
        int cumulativeWeight = 0;

        foreach (var option in validOptions)
        {
            cumulativeWeight += option.baseWeight;
            if (randomValue < cumulativeWeight)
            {
                // 선택된 행동의 쿨타임 기록 갱신
                option.lastUsedTime = Time.time;
                Debug.Log($"[Decision] '{option.actionName}' 선택됨! (가중치: {option.baseWeight})");

                return option.targetState; // 선택된 상태 리턴
            }
        }

        return validOptions[0].targetState; // 안전장치
    }
}