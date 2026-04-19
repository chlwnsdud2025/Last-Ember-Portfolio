using UnityEngine;

public enum BossStateType
{
    Run,
    Strafe,
    Dodge,
    Stun,
    Walk,
    Idle,
    Attack
}


[System.Serializable]
public class BossActionOption
{
    public string actionName;

    [Header("State Link")]
    public BossStateType stateType; 
    [HideInInspector] public BossBaseState targetState; 

    [Header("Conditions (����)")]
    public float minDistance = 0f;
    public float maxDistance = 999f;
    public float cooldown = 0f;
    [HideInInspector] public float lastUsedTime = -999f;

    [Header("Weight (����ġ)")]
    public int baseWeight = 10;

    [Header("Attack Data (Only for Attack State)")]
    public BossAttack_SO_Data attackData;
}