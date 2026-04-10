using UnityEngine;

public class AttackBehaviour : StateMachineBehaviour
{
    [Header("타이밍 설정 (0.0 ~ 1.0)")]
    public float comboWindowStart = 0.2f;
    public float comboWindowEnd = 0.7f;
    public float comboTransitionPoint = 0.6f;
    public float cancelWindowStart = 0.7f;

    private PlayerController player;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (player == null) player = animator.GetComponent<PlayerController>();

        player.canCombo = false;
        player.canNextAttack = false;
        player.canCancel = false;
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (player == null) return;

        //  [핵심 해결] 애니메이션이 서로 섞이는 트랜지션 구간에서는
        // 이전 애니메이션의 찌꺼기 값이 플래그를 망치지 못하게 강제로 0기화하고 리턴시킵니다.
        if (animator.IsInTransition(layerIndex))
        {
            player.canCombo = false;
            player.canNextAttack = false;
            player.canCancel = false;
            return;
        }

        float progress = stateInfo.normalizedTime;

        // 트랜지션이 완전히 끝나고 현재 애니메이션이 온전히 재생 중일 때만 판정
        player.canCombo = (progress >= comboWindowStart && progress <= comboWindowEnd);
        player.canNextAttack = (progress >= comboTransitionPoint);
        player.canCancel = (progress >= cancelWindowStart);
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (player == null) return;

        player.canCombo = false;
        player.canNextAttack = false;
        player.canCancel = false;
    }
}