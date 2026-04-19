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
            //attack�� ���
            if (chosenOption.targetState == boss.attackState && chosenOption.attackData != null)
            {
                // AttackState�� ����ȯ�� ��, currentAttackData�� SO�� �� �Ⱦ��ݴϴ�!
                var attackState = (Humanoid_Boss_Attack_State)boss.attackState;
                attackState.currentAttackData = chosenOption.attackData;
            }

            // ������ ������ �������� �Ƚ��ϰ� ���¸� ��ȯ�մϴ�.
            stateMachine.ChangeState(chosenOption.targetState);
        }
        else
        {
            boss.ReturnToDecision(); // ������ġ
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
                //  ����: option.targetState�� �ƴ϶� option ��ü�� ����!
                return option;
            }
        }

        return validOptions[0]; // ������ġ
    }

}