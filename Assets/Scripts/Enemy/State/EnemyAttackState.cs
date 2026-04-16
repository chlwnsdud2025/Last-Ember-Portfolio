using UnityEngine;

public class EnemyAttackState : EnemyBaseState
{
    private EnemyAttack_SO_Data attackData;
    private int attackAnimHash;
    private EnemyWeaponController currentHitbox;
    private bool isRotationLocked = false;
    public float lastUsedTime = -999f;
    // ❗ [변경] 생성자 마지막에 string stateName을 추가로 받습니다.
    public EnemyAttackState(EnemyController enemy, EnemyStateMachine enemyStateMachine, EnemyAttack_SO_Data attackData, string stateName) : base(enemy, enemyStateMachine)
    {
        this.attackData = attackData;

        // ❗ [변경] SO의 이름이 아니라, 컨트롤러가 덮어씌운 "Attack0", "Attack1" 등의 이름으로 해시를 만듭니다.
        this.attackAnimHash = Animator.StringToHash(stateName);
    }

    public override void Enter()
    {
        enemy.lastAttackTime = Time.time; // 에너미 전체의 글로벌 쿨다운 타이머 리셋
        this.lastUsedTime = Time.time;

        // 루트 모션 활성화
        enemy.animator.applyRootMotion = true;
        isRotationLocked = false;

        // SO에 적힌 이름으로 애니메이션 실행
        enemy.animator.CrossFadeInFixedTime(attackAnimHash, 0.2f, 0, 0f);

        //  [변경] SO에 설정된 무기 인덱스(칼, 방패 등)를 컨트롤러에서 꺼내옵니다.
        // (EnemyController에 GetWeapon(int index) 함수가 있어야 합니다!)
        currentHitbox = enemy.GetWeapon(attackData.weaponIndex);

        // 데미지 배율 적용 (적 컨트롤러에 기본 데미지가 있다고 가정)
        // if (currentHitbox != null) currentHitbox.currentDamage = enemy.baseDamage * attackData.damageMultiplier;

        //  여기서 무조건 Hitbox를 켜던 로직은 삭제합니다! (Update에서 타이밍에 맞춰 켭니다)
    }

    public override void Update()
    {
        AnimatorStateInfo stateInfo = enemy.animator.GetCurrentAnimatorStateInfo(0);
        float progress = stateInfo.normalizedTime;
        bool isPlayingAttack = stateInfo.shortNameHash == attackAnimHash;

        if (isPlayingAttack && !enemy.animator.IsInTransition(0))
        {
            // -------------------------------------------------------------
            // 1. 유도력 (Tracking) - SO 데이터 활용
            // -------------------------------------------------------------
            // SO에 설정된 유도력 종료 시점(trackingWindowEnd)까지만 회전합니다.
            if (progress <= attackData.trackingWindowEnd)
            {
                RotateTowardsTarget(20f);
            }
            else if (progress > attackData.trackingWindowEnd && !isRotationLocked)
            {
                isRotationLocked = true; // 소울류 특유의 묵직한 공격 방향 고정
            }

            // -------------------------------------------------------------
            // 2. 다중 히트박스 판정 껐다 켜기 - SO 배열 활용
            // -------------------------------------------------------------
            if (currentHitbox != null)
            {
                bool shouldBeActive = false;

                // SO에 등록해둔 모든 타격 타이밍을 뒤져봅니다.
                foreach (HitTiming timing in attackData.hitTimings)
                {
                    if (progress >= timing.startTime && progress <= timing.endTime)
                    {
                        shouldBeActive = true;
                        break; // 판정이 켜져야 하는 구간이면 즉시 반복문 탈출
                    }
                }

                if (shouldBeActive)
                {
                    currentHitbox.EnableHitbox();
                }
                else
                {
                    currentHitbox.DisableHitbox();
                }
            }

            // -------------------------------------------------------------
            // 3. 종료 단계 - SO 데이터 활용
            // -------------------------------------------------------------
            // SO에 설정된 종료 시점(transitionPoint)이 되면 다음 상태로 넘어갑니다.
            if (progress >= attackData.transitionPoint)
            {
                enemyStateMachine.ChangeState(enemy.strafeState);
            }
        }
    }

    private void RotateTowardsTarget(float speed)
    {
        if (enemy.target != null)
        {
            Vector3 dirToTarget = (enemy.target.position - enemy.transform.position).normalized;
            dirToTarget.y = 0f;
            if (dirToTarget != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(dirToTarget);
                enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, targetRot, speed * Time.deltaTime);
            }
        }
    }

    public override void Exit()
    {
        enemy.animator.applyRootMotion = false;

        //  [변경] 안전장치: 상태를 빠져나갈 때 현재 무기의 판정을 확실하게 끕니다.
        if (currentHitbox != null)
        {
            currentHitbox.DisableHitbox();
        }

        isRotationLocked = false;
    }
}