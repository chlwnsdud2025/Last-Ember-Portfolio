using UnityEditor;
using UnityEngine;

/// <summary>
/// 보스 컨트롤러의 availableActions 항목(BossActionOption) 드로어.
/// 접으면 한 줄 요약, 펼치면 편집에 필요한 필드만 보여 준다. 색·라벨은 BossPatternDrawer 팔레트를 따른다.
/// </summary>
[CustomPropertyDrawer(typeof(BossActionOption))]
public class BossActionOptionDrawer : PropertyDrawer
{
    private const float Pad = 2f;
    private const float BoxW = 46f;

    private static float Line => EditorGUIUtility.singleLineHeight;

    public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
    {
        if (!prop.isExpanded) return Line + Pad * 2f;

        int rows = 3;                                   // 이름·상태 / 사거리 / 쿨타임·가중치
        if (IsAttack(prop)) rows += 1;                  // 패턴 SO
        return (Line + Pad) * (rows + 1) + Pad * 2f;    // + 헤더
    }

    public override void OnGUI(Rect rect, SerializedProperty prop, GUIContent label)
    {
        SerializedProperty name = prop.FindPropertyRelative("actionName");
        SerializedProperty stateType = prop.FindPropertyRelative("stateType");
        SerializedProperty minD = prop.FindPropertyRelative("minDistance");
        SerializedProperty maxD = prop.FindPropertyRelative("maxDistance");
        SerializedProperty cooldown = prop.FindPropertyRelative("cooldown");
        SerializedProperty weight = prop.FindPropertyRelative("baseWeight");
        SerializedProperty attackData = prop.FindPropertyRelative("attackData");

        BossPatternDrawer.Palette palette = BossPatternDrawer.Palette.Current;

        Rect header = new Rect(rect.x, rect.y + Pad, rect.width, Line);
        prop.isExpanded = EditorGUI.Foldout(new Rect(header.x, header.y, 14f, header.height), prop.isExpanded, GUIContent.none);

        string typeName = EnumName(stateType);
        string title = string.IsNullOrEmpty(name.stringValue) ? typeName : name.stringValue;
        string summary = $"{typeName}   ·   {minD.floatValue:0.#} ~ {maxD.floatValue:0.#} m   ·   " +
                         $"쿨 {cooldown.floatValue:0.#}s   ·   w {weight.intValue}";

        Rect summaryRect = new Rect(header.x + 12f, header.y, header.width - 12f, header.height);
        BossPatternDrawer.DrawInlineSummary(summaryRect, palette, title, summary, DotColor(palette, stateType));

        if (!prop.isExpanded) return;

        EditorGUI.indentLevel++;
        float y = header.yMax + Pad;
        float half = rect.width * 0.5f - 4f;

        EditorGUIUtility.labelWidth = 52f;
        Rect r = new Rect(rect.x, y, rect.width, Line);
        EditorGUI.PropertyField(new Rect(r.x, r.y, half, Line), name, new GUIContent("이름"));
        EditorGUI.PropertyField(new Rect(r.x + half + 8f, r.y, half, Line), stateType, new GUIContent("상태"));
        y += Line + Pad;

        r = new Rect(rect.x, y, rect.width, Line);
        Rect field = EditorGUI.PrefixLabel(r, new GUIContent("사거리"));
        float lo = minD.floatValue;
        float hi = maxD.floatValue;
        int indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;
        lo = EditorGUI.FloatField(new Rect(field.x, r.y, BoxW, Line), lo);
        EditorGUI.MinMaxSlider(new Rect(field.x + BoxW + 4f, r.y, field.width - BoxW * 2f - 8f, Line), ref lo, ref hi, 0f, 30f);
        hi = EditorGUI.FloatField(new Rect(field.xMax - BoxW, r.y, BoxW, Line), hi);
        EditorGUI.indentLevel = indent;
        if (hi < lo) hi = lo;
        minD.floatValue = lo;
        maxD.floatValue = hi;
        y += Line + Pad;

        r = new Rect(rect.x, y, rect.width, Line);
        EditorGUI.PropertyField(new Rect(r.x, r.y, half, Line), cooldown, new GUIContent("쿨타임"));
        EditorGUI.PropertyField(new Rect(r.x + half + 8f, r.y, half, Line), weight, new GUIContent("가중치"));
        y += Line + Pad;

        if (IsAttack(prop))
        {
            EditorGUIUtility.labelWidth = 72f;
            r = new Rect(rect.x, y, rect.width, Line);
            EditorGUI.PropertyField(r, attackData, new GUIContent("패턴 SO"));
        }

        EditorGUIUtility.labelWidth = 0f;
        EditorGUI.indentLevel--;
    }

    private static Color DotColor(BossPatternDrawer.Palette palette, SerializedProperty stateType)
    {
        if (stateType.enumValueIndex == (int)BossStateType.Attack) return palette.hit;
        if (stateType.enumValueIndex == (int)BossStateType.Dodge) return palette.tracking;
        return palette.dim;
    }

    private static string EnumName(SerializedProperty stateType)
    {
        int i = stateType.enumValueIndex;
        string[] names = stateType.enumDisplayNames;
        return i >= 0 && i < names.Length ? names[i] : "-";
    }

    private static bool IsAttack(SerializedProperty prop)
    {
        SerializedProperty stateType = prop.FindPropertyRelative("stateType");
        return stateType != null && stateType.enumValueIndex == (int)BossStateType.Attack;
    }
}
