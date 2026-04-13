using UnityEngine;

[CreateAssetMenu(fileName = "Roll_SO_Data", menuName = "Scriptable Objects/Roll_SO_Data")]

[System.Serializable]
public class Roll_SO_Data : ScriptableObject
{
    [Header("Animation")]
    public AnimationClip clip;

    [Header("Timings (0.0 ~ 1.0)")]
    [Tooltip("무적 판정 시작 시점")]
    public float iframeStart = 0.1f;
    [Tooltip("무적 판정 종료 시점")]
    public float iframeEnd = 0.6f;

    [Tooltip("다음 행동(공격, 구르기) 선입력을 받기 시작하는 시점")]
    public float preInputWindowStart = 0.5f;

    [Tooltip("선입력된 행동이 실제로 실행되거나, 이동으로 캔슬 가능한 시점")]
    public float cancelWindow = 0.75f;

    [Tooltip("아무 입력이 없을 때 구르기 상태가 완전히 끝나는 시점 (Idle로 복귀)")]
    public float animationEnd = 0.95f;

    [Header("Stats")]
    public float staminaCost = 15f;
}
