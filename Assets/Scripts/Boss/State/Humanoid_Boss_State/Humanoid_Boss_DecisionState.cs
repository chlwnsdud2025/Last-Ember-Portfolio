using System.Collections.Generic;
using UnityEngine;

public class BossDecisionState : BossBaseState
{
    public BossDecisionState(BossBaseController boss, BossStateMachine stateMachine) : base(boss, stateMachine) { }

    public override void Enter()
    {
        BossActionOption chosenOption = ThinkAndDecide();

        if (chosenOption != null)
        {
            //attack일 경우
            if (chosenOption.targetState == boss.attackState && chosenOption.attackData != null)
            {
                // AttackState로 형변환한 뒤, currentAttackData에 SO를 쏙 꽂아줍니다!
                var attackState = (Humanoid_Boss_Attack_State)boss.attackState;
                attackState.currentAttackData = chosenOption.attackData;
            }

            // 데이터 세팅이 끝났으니 안심하고 상태를 전환합니다.
            stateMachine.ChangeState(chosenOption.targetState);
        }
        else
        {
            boss.ReturnToDecision(); // 안전장치
        }
    }

    private BossActionOption ThinkAndDecide()
    {
        float distanceToPlayer = Vector3.Distance(boss.transform.position, boss.target.position);
        List<BossActionOption> validOptions = new List<BossActionOption>();
        int totalWeight = 0;

        foreach (var option in boss.availableActions)
        {
            if (distanceToPlayer < option.minDistance || distanceToPlayer > option.maxDistance) continue;
            if (Time.time < option.lastUsedTime + option.cooldown) continue;

            validOptions.Add(option);
            totalWeight += option.baseWeight;
        }

        if (validOptions.Count == 0) return null;

        int randomValue = Random.Range(0, totalWeight);
        int cumulativeWeight = 0;

        foreach (var option in validOptions)
        {
            cumulativeWeight += option.baseWeight;
            if (randomValue < cumulativeWeight)
            {
                option.lastUsedTime = Time.time;
                //  변경: option.targetState가 아니라 option 자체를 리턴!
                return option;
            }
        }

        return validOptions[0]; // 안전장치
    }

}