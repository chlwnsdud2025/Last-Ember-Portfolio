using UnityEngine;

public enum BossStateType
{
    Run,
    Strafe,
    Dodge,
    Stun,
    Walk,
    Idle
}


[System.Serializable]
public class BossActionOption
{
    public string actionName;

    [Header("State Link")]
    public BossStateType stateType; //  인스펙터에서는 이 드롭다운 메뉴만 고르게 됩니다.
    [HideInInspector] public BossBaseState targetState; //  실제 런타임에 코드가 연결해줄 변수 (숨김)

    [Header("Conditions (조건)")]
    public float minDistance = 0f;
    public float maxDistance = 999f;
    public float cooldown = 0f;
    [HideInInspector] public float lastUsedTime = -999f;

    [Header("Weight (가중치)")]
    public int baseWeight = 10;
}