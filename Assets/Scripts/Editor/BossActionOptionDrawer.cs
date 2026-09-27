using SOTimeline.EditorTools;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 보스 컨트롤러의 availableActions 항목(BossActionOption) 드로어.
/// 접으면 한 줄 요약, 펼치면 편집에 필요한 필드만 보여 준다. 색은 SO_Timeline 의 레인 색을 따른다.
/// </summary>
[CustomPropertyDrawer(typeof(BossActionOption))]
public class BossActionOptionDrawer : PropertyDrawer
{
    const float k_Pad = 2f;
    const float k_BoxW = 46f;

    static float Line { get { return EditorGUIUtility.singleLineHeight; } }

    public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
    {
        if (!prop.isExpanded)
            return Line + k_Pad * 2f;

        var rows = 3;                                     // 이름·상태 / 사거리 / 쿨타임·가중치
        if (IsAttack(prop)) rows += 1;                    // 패턴 SO
        return (Line + k_Pad) * (rows + 1) + k_Pad * 2f;  // + 헤더
    }

    public override void OnGUI(Rect rect, SerializedProperty prop, GUIContent label)
    {
        var name = prop.FindPropertyRelative("actionName");
        var stateType = prop.FindPropertyRelative("stateType");
        var minD = prop.FindPropertyRelative("minDistance");
        var maxD = prop.FindPropertyRelative("maxDistance");
        var cooldown = prop.FindPropertyRelative("cooldown");
        var weight = prop.FindPropertyRelative("baseWeight");
        var attackData = prop.FindPropertyRelative("attackData");

        var header = new Rect(rect.x, rect.y + k_Pad, rect.width, Line);
        prop.isExpanded = EditorGUI.Foldout(new Rect(header.x, header.y, 14f, header.height), prop.isExpanded, GUIContent.none);

        var typeName = EnumName(stateType);
        var title = string.IsNullOrEmpty(name.stringValue) ? typeName : name.stringValue;
        var summary = typeName + "   ·   " + minD.floatValue.ToString("0.#") + " ~ " + maxD.floatValue.ToString("0.#") +
                      " m   ·   쿨 " + cooldown.floatValue.ToString("0.#") + "s   ·   w " + weight.intValue;

        if (Event.current.type == EventType.Repaint)
        {
            EditorGUI.DrawRect(new Rect(header.x + 14f, header.y + 3f, 3f, header.height - 6f), DotColor(stateType));

            var ts = new GUIStyle(EditorStyles.miniLabel);
            ts.fontStyle = FontStyle.Bold;
            ts.normal.textColor = new Color(0.86f, 0.86f, 0.86f, 1f);
            GUI.Label(new Rect(header.x + 23f, header.y, 150f, header.height), title, ts);

            var ss = new GUIStyle(EditorStyles.miniLabel);
            ss.alignment = TextAnchor.MiddleRight;
            ss.normal.textColor = new Color(0.55f, 0.55f, 0.55f, 1f);
            GUI.Label(new Rect(header.x + 173f, header.y, header.width - 173f, header.height), summary, ss);
        }

        if (!prop.isExpanded)
            return;

        EditorGUI.indentLevel++;
        var y = header.yMax + k_Pad;
        var half = rect.width * 0.5f - 4f;

        EditorGUIUtility.labelWidth = 52f;
        var r = new Rect(rect.x, y, rect.width, Line);
        EditorGUI.PropertyField(new Rect(r.x, r.y, half, Line), name, new GUIContent("이름"));
        EditorGUI.PropertyField(new Rect(r.x + half + 8f, r.y, half, Line), stateType, new GUIContent("상태"));
        y += Line + k_Pad;

        r = new Rect(rect.x, y, rect.width, Line);
        var field = EditorGUI.PrefixLabel(r, new GUIContent("사거리"));
        var lo = minD.floatValue;
        var hi = maxD.floatValue;
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;
        lo = EditorGUI.FloatField(new Rect(field.x, r.y, k_BoxW, Line), lo);
        EditorGUI.MinMaxSlider(new Rect(field.x + k_BoxW + 4f, r.y, field.width - k_BoxW * 2f - 8f, Line), ref lo, ref hi, 0f, 30f);
        hi = EditorGUI.FloatField(new Rect(field.xMax - k_BoxW, r.y, k_BoxW, Line), hi);
        EditorGUI.indentLevel = indent;
        if (hi < lo) hi = lo;
        minD.floatValue = lo;
        maxD.floatValue = hi;
        y += Line + k_Pad;

        r = new Rect(rect.x, y, rect.width, Line);
        EditorGUI.PropertyField(new Rect(r.x, r.y, half, Line), cooldown, new GUIContent("쿨타임"));
        EditorGUI.PropertyField(new Rect(r.x + half + 8f, r.y, half, Line), weight, new GUIContent("가중치"));
        y += Line + k_Pad;

        if (IsAttack(prop))
        {
            EditorGUIUtility.labelWidth = 72f;
            EditorGUI.PropertyField(new Rect(rect.x, y, rect.width, Line), attackData, new GUIContent("패턴 SO"));
        }

        EditorGUIUtility.labelWidth = 0f;
        EditorGUI.indentLevel--;
    }

    static Color DotColor(SerializedProperty stateType)
    {
        if (stateType.enumValueIndex == (int)BossStateType.Attack) return TimelineGUI.Hitbox;
        if (stateType.enumValueIndex == (int)BossStateType.Dodge) return TimelineGUI.Cancel;
        return new Color(0.42f, 0.42f, 0.42f, 1f);
    }

    static string EnumName(SerializedProperty stateType)
    {
        var i = stateType.enumValueIndex;
        var names = stateType.enumDisplayNames;
        return i >= 0 && i < names.Length ? names[i] : "-";
    }

    static bool IsAttack(SerializedProperty prop)
    {
        var stateType = prop.FindPropertyRelative("stateType");
        return stateType != null && stateType.enumValueIndex == (int)BossStateType.Attack;
    }
}
