using UnityEditor;
using UnityEngine;

/// <summary>
/// BossAttack_SO_Data 커스텀 인스펙터.
/// 위쪽에 패턴 요약 · 진행도 타임라인 · 연계 순서 · 검사를 두고, 아래에 설정 항목을 접이식 그룹으로 나눠 그린다.
/// 그리기는 BossPatternDrawer 가 담당한다.
/// </summary>
[CustomEditor(typeof(BossAttack_SO_Data))]
[CanEditMultipleObjects]
public class BossAttackSOEditor : Editor
{
    private readonly BossPatternDrawer.Context context = new BossPatternDrawer.Context();
    private string keyPrefix;

    private void OnEnable()
    {
        keyPrefix = "BossAttackSOEditor." + target.GetInstanceID() + ".";
    }

    public override void OnInspectorGUI()
    {
        if (serializedObject.isEditingMultipleObjects)
        {
            EditorGUILayout.HelpBox("여러 개를 동시에 선택했습니다. 기본 인스펙터로 표시합니다.", MessageType.Info);
            DrawDefaultInspector();
            return;
        }

        serializedObject.Update();

        context.data = (BossAttack_SO_Data)target;
        context.so = serializedObject;
        context.palette = BossPatternDrawer.Palette.Current;

        BossPatternDrawer.DrawSummarySection(context);

        EditorGUILayout.Space(12f);
        BossPatternDrawer.DrawTimelineSection(context);

        EditorGUILayout.Space(12f);
        BossPatternDrawer.DrawComboSection(context);

        EditorGUILayout.Space(12f);
        BossPatternDrawer.DrawIssueSection(context);

        EditorGUILayout.Space(14f);
        DrawSettings();

        serializedObject.ApplyModifiedProperties();
    }

    // ==========================================================
    // 설정 · 접이식 그룹
    // ==========================================================
    private void DrawSettings()
    {
        BossAttack_SO_Data d = context.data;
        bool melee = d.attackMethod == AttackMethod.Melee;

        if (Group("trigger", "발동 조건", $"{d.minDistance:0.#} ~ {d.maxDistance:0.#} m  ·  쿨 {d.cooldown:0.#}s  ·  w {d.weight}"))
        {
            Field("attackName");
            Field("minDistance");
            Field("maxDistance");
            Field("cooldown");
            Field("weight");
            Field("requiredPhase");
            EditorGUILayout.Space(2f);
            Field("validMinAngle");
            Field("validMaxAngle");
            EditorGUILayout.LabelField(" ", "각도 조건은 현재 Decision 필터에서 사용하지 않습니다.", EditorStyles.miniLabel);
            End();
        }

        if (Group("anim", "애니메이션", d.attackClip != null ? $"{d.attackClip.name}  ·  ×{d.animationSpeedMultiplier:0.##}" : "클립 없음"))
        {
            Field("attackClip");
            Field("animationSpeedMultiplier");
            Field("transitionDuration");
            Field("useRootMotion");
            End();
        }

        if (Group("tracking", "회전 · 유도", TrackingSummary(d)))
        {
            Field("trackingType");
            Field("turnSpeed");
            using (new EditorGUI.DisabledScope(d.trackingType != RotationTrackingType.OnlyStartup))
                Field("stopTrackingTime");
            End();
        }

        if (Group("combat", "전투 · 판정", $"데미지 {d.damage:0.#}  ·  " + (melee ? d.targetHitbox.ToString() : "프리팹 소환")))
        {
            Field("damage");
            Field("poiseDamage");
            Field("hasHyperArmor");
            EditorGUILayout.Space(2f);
            Field("attackMethod");

            if (melee)
            {
                Field("targetHitbox");
            }
            else
            {
                Field("attackPrefab");
                Field("spawnOnTarget");
                using (new EditorGUI.DisabledScope(d.spawnOnTarget))
                    Field("spawnOffset");
            }

            EditorGUILayout.Space(2f);
            Field("hitboxStartTime");
            using (new EditorGUI.DisabledScope(!melee))
                Field("hitboxEndTime");
            End();
        }

        if (Group("combo", "연계 · 종료", d.nextCombo != null ? $"→ {ChainName(d.nextCombo)}" : $"종료 {d.attackEndTime:0.00}"))
        {
            Field("comboEnableTime");
            Field("attackEndTime");
            Field("nextCombo");
            End();
        }
    }

    private bool Group(string key, string title, string summary)
    {
        bool open = BossPatternDrawer.DrawFoldoutHeader(context, keyPrefix + key, title, summary);
        if (open)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUI.indentLevel++;
        }
        else
        {
            EditorGUILayout.Space(2f);
        }
        return open;
    }

    private void End()
    {
        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(4f);
    }

    private void Field(string propertyName)
    {
        SerializedProperty p = serializedObject.FindProperty(propertyName);
        if (p != null) EditorGUILayout.PropertyField(p, true);
    }

    private static string TrackingSummary(BossAttack_SO_Data d)
    {
        switch (d.trackingType)
        {
            case RotationTrackingType.Continuous: return "공격 내내 추적";
            case RotationTrackingType.OnlyStartup: return $"{d.stopTrackingTime:0.00} 까지만 추적";
            default: return "추적 없음";
        }
    }

    private static string ChainName(BossAttack_SO_Data next)
    {
        return string.IsNullOrEmpty(next.attackName) ? next.name : next.attackName;
    }
}
