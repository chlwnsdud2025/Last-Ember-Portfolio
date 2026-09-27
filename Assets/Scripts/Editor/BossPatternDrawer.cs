using UnityEditor;
using UnityEngine;

/// <summary>
/// 보스 공격 패턴(BossAttack_SO_Data)의 요약 · 진행도 타임라인 · 연계 · 검사 항목을 그리는 드로어.
/// BossAttackSOEditor(패턴 인스펙터)와 BossActionOptionDrawer(행동 목록 항목)가 함께 사용한다.
/// </summary>
public static class BossPatternDrawer
{
    // ==========================================================
    // 팔레트
    // ==========================================================
    public struct Palette
    {
        public Color panel, panelAlt, line, text, dim, accent, tracking, hit, spawn, combo, body, marker, warn;

        public static Palette Dark => new Palette
        {
            panel = Hex(0x4A4A4A), panelAlt = Hex(0x3D3D3D), line = Hex(0x1E1E1E),
            text = Hex(0xE8E8E8), dim = Hex(0xA0A0A0), accent = Hex(0x8FCF8F),
            tracking = Hex(0x4E86C7), hit = Hex(0xF26A6A), spawn = Hex(0xF5C542), combo = Hex(0x8FCF8F),
            body = new Color(1f, 1f, 1f, 0.10f), marker = Color.white, warn = Hex(0xF5C542),
        };

        public static Palette Light => new Palette
        {
            panel = Hex(0xF4F4F5), panelAlt = Hex(0xFAFAFA), line = Hex(0x8A8F99),
            text = Hex(0x1A1D24), dim = Hex(0x6B7280), accent = Hex(0x2F8F4E),
            tracking = Hex(0x2F6FB5), hit = Hex(0xD9463F), spawn = Hex(0xB8860B), combo = Hex(0x2F8F4E),
            body = new Color(0f, 0f, 0f, 0.08f), marker = Hex(0x1A1D24), warn = Hex(0xB8860B),
        };

        public static Palette Current => EditorGUIUtility.isProSkin ? Dark : Light;
    }

    // ==========================================================
    // 그리기 컨텍스트
    // ==========================================================
    public class Context
    {
        public BossAttack_SO_Data data;
        public SerializedObject so;                 // 타임라인 드래그로 값을 되돌려 쓰기 위해 필요
        public Palette palette = Palette.Dark;

        public int dragHandle = -1;                 // 드래그 중인 핸들 (없으면 -1)

        /// <summary>속도 배율까지 반영한 실제 재생 시간(초). 클립이 없으면 0.</summary>
        public float Duration
        {
            get
            {
                if (data == null || data.attackClip == null) return 0f;
                return data.attackClip.length / Mathf.Max(0.01f, data.animationSpeedMultiplier);
            }
        }

        public float TrackingEnd
        {
            get
            {
                if (data == null) return 0f;
                switch (data.trackingType)
                {
                    case RotationTrackingType.Continuous: return 1f;
                    case RotationTrackingType.OnlyStartup: return data.stopTrackingTime;
                    default: return 0f;
                }
            }
        }
    }

    // ==========================================================
    // 스타일 (팔레트가 바뀔 때만 재생성)
    // ==========================================================
    private class Styles
    {
        public GUIStyle section, label, bold, dim, small, smallBold, dimRight;
        public GUIStyle barLabel, barLabelCenter, rowLabel;
        public float rowH, barH;
    }

    private static Styles styles;
    private static Color styleKeyColor;

    private static Styles S(Palette p)
    {
        if (styles != null && styleKeyColor == p.text)
            return styles;

        styleKeyColor = p.text;

        GUIStyle Make(int size, Color color, bool bold = false, TextAnchor anchor = TextAnchor.MiddleLeft)
        {
            GUIStyle st = new GUIStyle(EditorStyles.label)
            {
                fontSize = size,
                alignment = anchor,
                fontStyle = bold ? FontStyle.Bold : FontStyle.Normal,
                richText = false,
                clipping = TextClipping.Clip,
            };
            st.normal.textColor = color;
            st.hover.textColor = color;
            return st;
        }

        styles = new Styles
        {
            section = Make(13, p.text, true),
            label = Make(11, p.text),
            bold = Make(11, p.text, true),
            dim = Make(11, p.dim),
            small = Make(10, p.dim),
            smallBold = Make(10, p.text, true),
            dimRight = Make(11, p.dim, false, TextAnchor.MiddleRight),

            barLabel = Make(10, Color.white, true),
            barLabelCenter = Make(10, Color.white, true, TextAnchor.MiddleCenter),
            rowLabel = Make(10, p.dim, false, TextAnchor.MiddleRight),

            rowH = 20f,
            barH = 22f,
        };
        return styles;
    }

    // ==========================================================
    // 섹션 1 · 요약
    // ==========================================================
    public static void DrawSummarySection(Context c)
    {
        if (c.data == null) return;

        Styles st = S(c.palette);
        BossAttack_SO_Data d = c.data;
        bool melee = d.attackMethod == AttackMethod.Melee;

        string title = string.IsNullOrEmpty(d.attackName) ? d.name : d.attackName;
        DrawSectionHeader(c, "패턴 요약", title);

        Rect r = GUILayoutUtility.GetRect(10f, st.rowH * 4f + 14f, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(r, c.palette.panelAlt);
        DrawFrame(r, c.palette.line);

        float cell = (r.width - 8f) / 5f;
        float row1 = r.y + 3f;
        float row2 = r.y + st.rowH * 2f + 8f;
        EditorGUI.DrawRect(new Rect(r.x + 4f, row2 - 4f, r.width - 8f, 1f), c.palette.line);

        float x = r.x + 4f;
        Cell(c, ref x, row1, cell, "사거리", $"{d.minDistance:0.#} ~ {d.maxDistance:0.#} m");
        Cell(c, ref x, row1, cell, "쿨타임", $"{d.cooldown:0.#} s");
        Cell(c, ref x, row1, cell, "가중치", d.weight.ToString());
        Cell(c, ref x, row1, cell, "페이즈", d.requiredPhase <= 1 ? "1~" : $"{d.requiredPhase} 이상");
        Cell(c, ref x, row1, cell, "재생 길이", c.Duration > 0f ? $"{c.Duration:0.00} s" : "클립 없음");

        x = r.x + 4f;
        Cell(c, ref x, row2, cell, "데미지", $"{d.damage:0.#}");
        Cell(c, ref x, row2, cell, "강인도", $"{d.poiseDamage:0.#}");
        Cell(c, ref x, row2, cell, "슈퍼아머", d.hasHyperArmor ? "적용" : "없음", d.hasHyperArmor ? c.palette.text : c.palette.dim);
        Cell(c, ref x, row2, cell,
             melee ? "히트박스" : "소환 프리팹",
             melee ? d.targetHitbox.ToString() : (d.attackPrefab != null ? d.attackPrefab.name : "비어 있음"),
             melee ? c.palette.hit : c.palette.spawn);
        Cell(c, ref x, row2, cell,
             melee ? "판정 구간" : "소환 시점",
             HitWindowText(c, d, melee),
             melee ? c.palette.hit : c.palette.spawn);
    }

    private static string HitWindowText(Context c, BossAttack_SO_Data d, bool melee)
    {
        if (!melee)
            return c.Duration > 0f ? $"{d.hitboxStartTime:0.00}  ({d.hitboxStartTime * c.Duration:0.00}s)" : $"{d.hitboxStartTime:0.00}";

        string norm = $"{d.hitboxStartTime:0.00} – {d.hitboxEndTime:0.00}";
        if (c.Duration <= 0f) return norm;
        return $"{norm}  ({(d.hitboxEndTime - d.hitboxStartTime) * c.Duration:0.00}s)";
    }

    private static void Cell(Context c, ref float x, float y, float w, string label, string value)
    {
        Cell(c, ref x, y, w, label, value, c.palette.text);
    }

    private static void Cell(Context c, ref float x, float y, float w, string label, string value, Color valueColor)
    {
        Styles st = S(c.palette);
        GUI.Label(new Rect(x, y, w, st.rowH - 2f), label, st.small);

        GUIStyle valueStyle = st.bold;
        if (valueColor != c.palette.text)
        {
            valueStyle = new GUIStyle(st.bold);
            valueStyle.normal.textColor = valueColor;
        }
        GUI.Label(new Rect(x, y + st.rowH - 3f, w, st.rowH), value, valueStyle);
        x += w;
    }

    // ==========================================================
    // 섹션 2 · 진행도 타임라인
    // ==========================================================
    public static void DrawTimelineSection(Context c)
    {
        if (c.data == null) return;

        Styles st = S(c.palette);
        BossAttack_SO_Data d = c.data;
        bool spawn = d.attackMethod == AttackMethod.SpawnPrefab;
        bool hasCombo = d.nextCombo != null;

        DrawSectionHeader(c, "진행도 타임라인",
            c.Duration > 0f ? $"normalizedTime 0 ~ 1  ·  실제 {c.Duration:0.00}초" : "normalizedTime 0 ~ 1");

        const float labelW = 52f;
        const int rows = 4;
        Rect area = GUILayoutUtility.GetRect(10f, st.barH * rows + 26f, GUILayout.ExpandWidth(true));
        Rect track = new Rect(area.x + labelW, area.y + 16f, area.width - labelW - 42f, st.barH * rows);

        EditorGUI.DrawRect(track, c.palette.panel);
        DrawFrame(track, c.palette.line);
        DrawRuler(c, track);

        Rect r0 = Row(c, track, 0);
        RowLabel(c, area, labelW, r0, "유도");
        if (c.TrackingEnd > 0f) Bar(c, r0, track, 0f, c.TrackingEnd, c.palette.tracking, $"{c.TrackingEnd:0.00}");
        else Note(c, r0, "회전 없음");

        Rect r1 = Row(c, track, 1);
        RowLabel(c, area, labelW, r1, spawn ? "소환" : "판정");
        if (spawn) Bar(c, r1, track, d.hitboxStartTime, Mathf.Min(1f, d.hitboxStartTime + 0.02f), c.palette.spawn, string.Empty);
        else Bar(c, r1, track, d.hitboxStartTime, d.hitboxEndTime, c.palette.hit, $"{d.hitboxStartTime:0.00} – {d.hitboxEndTime:0.00}");

        Rect r2 = Row(c, track, 2);
        RowLabel(c, area, labelW, r2, "연계");
        if (hasCombo) Bar(c, r2, track, d.comboEnableTime, 1f, c.palette.combo, $"{d.comboEnableTime:0.00} –");
        else Note(c, r2, "nextCombo 없음");

        Rect r3 = Row(c, track, 3);
        RowLabel(c, area, labelW, r3, "본체");
        Bar(c, r3, track, 0f, d.attackEndTime, c.palette.body, string.Empty);
        Marker(c, track, r3, d.attackEndTime, $"종료 {d.attackEndTime:0.00}");

        Handle(c, track, r0, c.TrackingEnd, d.trackingType == RotationTrackingType.OnlyStartup);
        Handle(c, track, r1, d.hitboxStartTime, true);
        Handle(c, track, r1, d.hitboxEndTime, !spawn);
        Handle(c, track, r2, d.comboEnableTime, hasCombo);
        Handle(c, track, r3, d.attackEndTime, true);

        HandleDrag(c, track);

        GUILayout.Space(2f);
        Rect hint = GUILayoutUtility.GetRect(10f, 14f, GUILayout.ExpandWidth(true));
        GUI.Label(hint, "흰 손잡이를 끌어 조정 · Ctrl 드래그 = 0.05 단위", S(c.palette).small);
    }

    private static Rect Row(Context c, Rect track, int index)
    {
        float h = S(c.palette).barH;
        return new Rect(track.x, track.y + h * index + 3f, track.width, h - 6f);
    }

    private static void RowLabel(Context c, Rect area, float labelW, Rect row, string text)
    {
        GUI.Label(new Rect(area.x, row.y - 1f, labelW - 6f, row.height + 2f), text, S(c.palette).rowLabel);
    }

    private static void Note(Context c, Rect row, string text)
    {
        GUI.Label(new Rect(row.x + 6f, row.y, row.width, row.height), text, S(c.palette).small);
    }

    private static void Bar(Context c, Rect row, Rect track, float from, float to, Color color, string caption)
    {
        from = Mathf.Clamp01(from);
        to = Mathf.Clamp01(to);
        if (to < from) { float t = from; from = to; to = t; }

        float x0 = track.x + track.width * from;
        float x1 = track.x + track.width * to;
        Rect bar = new Rect(x0, row.y, Mathf.Max(3f, x1 - x0), row.height);
        EditorGUI.DrawRect(bar, color);

        if (!string.IsNullOrEmpty(caption) && bar.width > 56f)
            GUI.Label(bar, caption, S(c.palette).barLabelCenter);
    }

    private static void Marker(Context c, Rect track, Rect row, float t, string caption)
    {
        float x = track.x + track.width * Mathf.Clamp01(t);
        EditorGUI.DrawRect(new Rect(x - 1f, track.y, 2f, track.height), c.palette.marker);

        Styles st = S(c.palette);
        float w = 64f;
        float lx = Mathf.Min(x + 5f, track.xMax - w);
        GUI.Label(new Rect(lx, row.y, w, row.height), caption, st.smallBold);
    }

    private static void DrawRuler(Context c, Rect track)
    {
        Styles st = S(c.palette);
        GUIStyle center = new GUIStyle(st.small) { alignment = TextAnchor.UpperCenter };
        float dur = c.Duration;

        for (int i = 0; i <= 4; i++)
        {
            float t = i * 0.25f;
            float x = track.x + track.width * t;
            EditorGUI.DrawRect(new Rect(x, track.y, 1f, track.height), c.palette.line);
            string label = dur > 0f ? $"{t:0.00}  ({t * dur:0.00}s)" : $"{t:0.00}";
            GUI.Label(new Rect(x - 42f, track.y - 15f, 84f, 14f), label, center);
        }
    }

    private static void Handle(Context c, Rect track, Rect row, float t, bool enabled)
    {
        if (!enabled) return;
        float x = track.x + track.width * Mathf.Clamp01(t);
        Rect h = new Rect(x - 4.5f, row.y, 9f, row.height);
        EditorGUI.DrawRect(h, c.palette.marker);
        EditorGUIUtility.AddCursorRect(h, MouseCursor.ResizeHorizontal);
    }

    private static void HandleDrag(Context c, Rect track)
    {
        Event e = Event.current;
        if (e == null || c.so == null) return;

        if (e.type == EventType.MouseDown && e.button == 0 && track.Contains(e.mousePosition))
        {
            int row = Mathf.Clamp(Mathf.FloorToInt((e.mousePosition.y - track.y) / S(c.palette).barH), 0, 3);
            float t = Mathf.Clamp01((e.mousePosition.x - track.x) / track.width);
            c.dragHandle = PickHandle(c, row, t);
            if (c.dragHandle >= 0) { GUI.FocusControl(null); e.Use(); }
        }
        else if (e.type == EventType.MouseDrag && c.dragHandle >= 0)
        {
            float t = Mathf.Clamp01((e.mousePosition.x - track.x) / track.width);
            if (e.control || e.command) t = Mathf.Round(t * 20f) / 20f;
            Apply(c, c.dragHandle, Mathf.Round(t * 1000f) / 1000f);
            e.Use();
        }
        else if (e.type == EventType.MouseUp && c.dragHandle >= 0)
        {
            c.dragHandle = -1;
            e.Use();
        }
    }

    private static int PickHandle(Context c, int row, float t)
    {
        BossAttack_SO_Data d = c.data;
        switch (row)
        {
            case 0: return d.trackingType == RotationTrackingType.OnlyStartup ? 0 : -1;
            case 1:
                if (d.attackMethod == AttackMethod.SpawnPrefab) return 1;
                return Mathf.Abs(t - d.hitboxStartTime) <= Mathf.Abs(t - d.hitboxEndTime) ? 1 : 2;
            case 2: return d.nextCombo != null ? 3 : -1;
            default: return 4;
        }
    }

    private static void Apply(Context c, int handle, float t)
    {
        c.so.Update();
        switch (handle)
        {
            case 0: Set(c, "stopTrackingTime", t); break;
            case 1: Set(c, "hitboxStartTime", Mathf.Min(t, c.data.hitboxEndTime)); break;
            case 2: Set(c, "hitboxEndTime", Mathf.Max(t, c.data.hitboxStartTime)); break;
            case 3: Set(c, "comboEnableTime", t); break;
            case 4: Set(c, "attackEndTime", t); break;
        }
        c.so.ApplyModifiedProperties();
    }

    private static void Set(Context c, string field, float value)
    {
        SerializedProperty p = c.so.FindProperty(field);
        if (p != null) p.floatValue = value;
    }

    // ==========================================================
    // 섹션 3 · 연계
    // ==========================================================
    public static void DrawComboSection(Context c)
    {
        if (c.data == null || c.data.nextCombo == null) return;

        Styles st = S(c.palette);
        DrawSectionHeader(c, "연계 순서", "클릭하면 해당 패턴으로 이동");

        Rect r = GUILayoutUtility.GetRect(10f, st.rowH + 6f, GUILayout.ExpandWidth(true));
        float x = r.x + 2f;

        BossAttack_SO_Data cur = c.data;
        int guard = 0;
        while (cur != null && guard++ < 8)
        {
            string label = string.IsNullOrEmpty(cur.attackName) ? cur.name : cur.attackName;
            float w = Mathf.Max(64f, st.label.CalcSize(new GUIContent(label)).x + 16f);
            if (x + w > r.xMax) break;

            Rect chip = new Rect(x, r.y + 2f, w, st.rowH);
            bool self = cur == c.data;
            EditorGUI.DrawRect(chip, self ? c.palette.accent : c.palette.panelAlt);
            DrawFrame(chip, c.palette.line);

            GUIStyle chipStyle = new GUIStyle(st.smallBold) { alignment = TextAnchor.MiddleCenter };
            if (self) chipStyle.normal.textColor = EditorGUIUtility.isProSkin ? Hex(0x1A1D24) : Color.white;
            GUI.Label(chip, label, chipStyle);

            if (!self && GUI.Button(chip, GUIContent.none, GUIStyle.none))
                Selection.activeObject = cur;

            x += w;
            cur = cur.nextCombo;
            if (cur != null && x + 16f < r.xMax)
            {
                GUI.Label(new Rect(x, r.y + 2f, 16f, st.rowH), "→", st.dim);
                x += 16f;
            }
        }
    }

    // ==========================================================
    // 섹션 4 · 검사
    // ==========================================================
    public static void DrawIssueSection(Context c)
    {
        BossAttack_SO_Data d = c.data;
        if (d == null) return;

        int count = 0;

        if (d.attackClip == null)
            Issue(ref count, c, MessageType.Error, "클립이 비어 있습니다. Attack 상태가 진입하자마자 Decision으로 돌아갑니다.");

        if (d.hitboxEndTime < d.hitboxStartTime)
            Issue(ref count, c, MessageType.Error, "판정 종료가 시작보다 앞섭니다. 히트박스가 켜지지 않습니다.");

        if (d.attackMethod == AttackMethod.SpawnPrefab && d.attackPrefab == null)
            Issue(ref count, c, MessageType.Error, "SpawnPrefab 방식인데 소환 프리팹이 비어 있습니다.");

        if (d.attackEndTime < d.hitboxEndTime)
            Issue(ref count, c, MessageType.Warning, "판정이 끝나기 전에 상태가 종료됩니다. 히트박스가 켜진 채로 빠져나갈 수 있습니다.");

        if (d.nextCombo != null && d.comboEnableTime > d.attackEndTime)
            Issue(ref count, c, MessageType.Warning, "연계 시점이 종료 시점보다 늦어 nextCombo가 실행되지 않습니다.");

        if (d.maxDistance <= d.minDistance)
            Issue(ref count, c, MessageType.Warning, "사거리 하한이 상한보다 크거나 같아 후보로 뽑히지 않습니다.");

        if (d.weight <= 0)
            Issue(ref count, c, MessageType.Warning, "가중치가 0이라 룰렛에서 선택되지 않습니다.");

        if (!Mathf.Approximately(d.validMinAngle, -45f) || !Mathf.Approximately(d.validMaxAngle, 45f))
            Issue(ref count, c, MessageType.Info, "각도 조건(validMinAngle · validMaxAngle)은 현재 Decision 필터에서 사용하지 않습니다.");

        if (count == 0)
        {
            DrawSectionHeader(c, "검사", "이상 없음");
            Rect r = GUILayoutUtility.GetRect(10f, S(c.palette).rowH, GUILayout.ExpandWidth(true));
            GUI.Label(new Rect(r.x + 9f, r.y, r.width - 9f, r.height), "거리 · 쿨타임 · 타이밍 설정에서 발견된 문제가 없습니다.", S(c.palette).dim);
        }
    }

    private static void Issue(ref int count, Context c, MessageType type, string message)
    {
        if (count == 0) DrawSectionHeader(c, "검사", "확인이 필요한 항목");
        count++;
        EditorGUILayout.HelpBox(message, type);
    }

    // ==========================================================
    // 공통 유틸
    // ==========================================================
    public static void DrawSectionHeader(Context c, string title, string summary)
    {
        Styles st = S(c.palette);
        Rect r = GUILayoutUtility.GetRect(10f, st.rowH + 4f, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(new Rect(r.x, r.y + 5f, 3f, r.height - 10f), c.palette.accent);
        GUI.Label(new Rect(r.x + 9f, r.y, 200f, r.height), title, st.section);
        GUI.Label(new Rect(r.x + 200f, r.y, r.width - 200f, r.height), summary, st.dimRight);
        GUILayout.Space(2f);
    }

    /// <summary>섹션 헤더와 같은 모양의 접이식 헤더. 열림 상태는 SessionState에 남는다.</summary>
    public static bool DrawFoldoutHeader(Context c, string key, string title, string summary)
    {
        Styles st = S(c.palette);
        bool open = SessionState.GetBool(key, false);

        Rect r = GUILayoutUtility.GetRect(10f, st.rowH + 4f, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(new Rect(r.x, r.y + 5f, 3f, r.height - 10f), open ? c.palette.accent : c.palette.dim);
        GUI.Label(new Rect(r.x + 9f, r.y, 14f, r.height), open ? "▾" : "▸", st.dim);
        GUI.Label(new Rect(r.x + 23f, r.y, 210f, r.height), title, st.section);
        GUI.Label(new Rect(r.x + 233f, r.y, r.width - 233f, r.height), summary, st.dimRight);
        EditorGUIUtility.AddCursorRect(r, MouseCursor.Link);

        Event e = Event.current;
        if (e != null && e.type == EventType.MouseDown && e.button == 0 && r.Contains(e.mousePosition))
        {
            open = !open;
            SessionState.SetBool(key, open);
            e.Use();
        }

        GUILayout.Space(2f);
        return open;
    }

    public static void DrawFrame(Rect r, Color color)
    {
        EditorGUI.DrawRect(new Rect(r.x, r.y, r.width, 1f), color);
        EditorGUI.DrawRect(new Rect(r.x, r.yMax - 1f, r.width, 1f), color);
        EditorGUI.DrawRect(new Rect(r.x, r.y, 1f, r.height), color);
        EditorGUI.DrawRect(new Rect(r.xMax - 1f, r.y, 1f, r.height), color);
    }

    /// <summary>행동 목록 항목 등 좁은 줄에 쓰는 한 줄 요약 라벨.</summary>
    public static void DrawInlineSummary(Rect rect, Palette palette, string title, string summary, Color dot)
    {
        Styles st = S(palette);
        EditorGUI.DrawRect(new Rect(rect.x, rect.y + 4f, 3f, rect.height - 8f), dot);
        GUI.Label(new Rect(rect.x + 9f, rect.y, 150f, rect.height), title, st.bold);
        GUI.Label(new Rect(rect.x + 159f, rect.y, rect.width - 159f, rect.height), summary, st.dimRight);
    }

    public static Color Hex(int rgb) =>
        new Color(((rgb >> 16) & 0xFF) / 255f, ((rgb >> 8) & 0xFF) / 255f, (rgb & 0xFF) / 255f, 1f);
}
