using UnityEngine;

public class BossController : MonoBehaviour
{
    [Header("Base Components")]
    public Animator animator;
    public CharacterController characterController;
    public Transform player; // 플레이어 참조

    [Header("Base Stats")]
    public float maxHealth = 1000f;
    public float currentHealth;
    public int currentPhase = 1;

    // 공용 상태 머신
    public BossStateMachine bossStateMachine { get; protected set; }

    protected virtual void Awake()
    {
        bossStateMachine = new BossStateMachine();
        currentHealth = maxHealth;
    }

    protected virtual void Start() { }

    protected virtual void Update()
    {
        bossStateMachine.Update();
    }

    public void TakeDamage(float damage)
    {
        bossStateMachine.CurrentState.OnTakeDamage(damage);
    }
}
