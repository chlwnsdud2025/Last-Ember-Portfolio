using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SOTimeline.EditorTools
{
    [CustomEditor(typeof(BossAttack_SO_Data))]
    internal sealed class BossAttack_SO_DataEditor : Editor
    {
        const float k_PreviewW = 168f;
        const float k_PreviewH = 150f;
        const float k_CtrlH = 20f;
        const float k_FieldH = 18f;
        const float k_SummaryRowH = 17f;
        const float k_HeaderH = 20f;

        // 초록 = 읽기 전용 요약 · 정보, 밝은 회색 = 값을 직접 만지는 설정 그룹
        static readonly Color k_BarInfo = new Color(0.56f, 0.81f, 0.56f, 1f);
        static readonly Color k_BarEdit = new Color(0.72f, 0.72f, 0.72f, 1f);

        SerializedProperty _attackName, _clip;
        SerializedProperty _minDistance, _maxDistance, _cooldown, _weight, _requiredPhase;
        SerializedProperty _validMinAngle, _validMaxAngle;
        SerializedProperty _transitionDuration, _speed, _useRootMotion;
        SerializedProperty _trackingType, _turnSpeed, _stopTracking;
        SerializedProperty _damage, _poise, _hyperArmor, _method, _hitbox;
        SerializedProperty _prefab, _spawnOffset, _spawnOnTarget;
        SerializedProperty _hitStart, _hitEnd;
        SerializedProperty _comboEnable, _attackEnd, _nextCombo;

        ClipPosePreview _preview;
        float _playhead = 0.25f;
        bool _playing;
        double _playT0;
        bool _showRigSettings;
        bool _stripRootMotion = true;
        string _keyPrefix;

        // ------------------------------------------------------------ lifecycle

        void OnEnable()
        {
            _attackName = serializedObject.FindProperty("attackName");
            _clip = serializedObject.FindProperty("attackClip");

            _minDistance = serializedObject.FindProperty("minDistance");
            _maxDistance = serializedObject.FindProperty("maxDistance");
            _cooldown = serializedObject.FindProperty("cooldown");
            _weight = serializedObject.FindProperty("weight");
            _requiredPhase = serializedObject.FindProperty("requiredPhase");
            _validMinAngle = serializedObject.FindProperty("validMinAngle");
            _validMaxAngle = serializedObject.FindProperty("validMaxAngle");

            _transitionDuration = serializedObject.FindProperty("transitionDuration");
            _speed = serializedObject.FindProperty("animationSpeedMultiplier");
            _useRootMotion = serializedObject.FindProperty("useRootMotion");

            _trackingType = serializedObject.FindProperty("trackingType");
            _turnSpeed = serializedObject.FindProperty("turnSpeed");
            _stopTracking = serializedObject.FindProperty("stopTrackingTime");

            _damage = serializedObject.FindProperty("damage");
            _poise = serializedObject.FindProperty("poiseDamage");
            _hyperArmor = serializedObject.FindProperty("hasHyperArmor");
            _method = serializedObject.FindProperty("attackMethod");
            _hitbox = serializedObject.FindProperty("targetHitbox");
            _prefab = serializedObject.FindProperty("attackPrefab");
            _spawnOffset = serializedObject.FindProperty("spawnOffset");
            _spawnOnTarget = serializedObject.FindProperty("spawnOnTarget");
            _hitStart = serializedObject.FindProperty("hitboxStartTime");
            _hitEnd = serializedObject.FindProperty("hitboxEndTime");

            _comboEnable = serializedObject.FindProperty("comboEnableTime");
            _attackEnd = serializedObject.FindProperty("attackEndTime");
            _nextCombo = serializedObject.FindProperty("nextCombo");

            _keyPrefix = "BossAttackSO." + target.GetInstanceID() + ".";
        }

        void OnDisable()
        {
            if (_preview != null)
            {
                _preview.Dispose();
                _preview = null;
            }

            _playing = false;
        }

        // ------------------------------------------------------------ inspector

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SectionHeader("패턴 요약", AttackTitle());
            DrawSummary();

            EditorGUILayout.Space(8f);

            SectionHeader("진행도 타임라인", DurationText());

            EditorGUI.BeginChangeCheck();
            var picked = EditorGUILayout.ObjectField("애니메이션 클립", _clip.objectReferenceValue, typeof(AnimationClip), false);
            if (EditorGUI.EndChangeCheck())
                _clip.objectReferenceValue = picked;

            EditorGUILayout.Space(6f);

            var clip = _clip.objectReferenceValue as AnimationClip;
            var melee = _method.enumValueIndex == (int)AttackMethod.Melee;
            var hasCombo = _nextCombo.objectReferenceValue != null;

            var body = GUILayoutUtility.GetRect(0f, k_PreviewH + k_CtrlH, GUILayout.ExpandWidth(true));

            DrawPreview(new Rect(body.x, body.y, k_PreviewW, k_PreviewH), clip);

            var rightX = body.x + k_PreviewW + 8f;
            var rightW = Mathf.Max(120f, body.xMax - rightX);
            var ry = body.y;

            TimelineGUI.Ruler(new Rect(rightX, ry, rightW, TimelineGUI.RulerHeight));
            ry += TimelineGUI.RulerHeight;

            var lanesTop = ry;

            switch ((RotationTrackingType)_trackingType.enumValueIndex)
            {
                case RotationTrackingType.OnlyStartup:
                    TimelineGUI.HeadLane(new Rect(rightX, ry, rightW, TimelineGUI.LaneHeight), "유도", TimelineGUI.Cancel, _stopTracking);
                    break;
                case RotationTrackingType.Continuous:
                    TimelineGUI.ReadonlyLane(new Rect(rightX, ry, rightW, TimelineGUI.LaneHeight), "유도", TimelineGUI.Cancel, 0f, 1f);
                    break;
                default:
                    TimelineGUI.ReadonlyLane(new Rect(rightX, ry, rightW, TimelineGUI.LaneHeight), "유도", TimelineGUI.Cancel, 0f, 0f);
                    break;
            }
            ry += TimelineGUI.LaneStride;

            if (melee)
                TimelineGUI.BandLane(new Rect(rightX, ry, rightW, TimelineGUI.LaneHeight), "판정", TimelineGUI.Hitbox, _hitStart, _hitEnd);
            else
                TimelineGUI.MarkerLane(new Rect(rightX, ry, rightW, TimelineGUI.LaneHeight), "소환", TimelineGUI.Hitbox, _hitStart);
            ry += TimelineGUI.LaneStride;

            if (hasCombo)
                TimelineGUI.TailLane(new Rect(rightX, ry, rightW, TimelineGUI.LaneHeight), "연계", TimelineGUI.Combo, _comboEnable);
            else
                TimelineGUI.ReadonlyLane(new Rect(rightX, ry, rightW, TimelineGUI.LaneHeight), "연계", TimelineGUI.Combo, 0f, 0f);
            ry += TimelineGUI.LaneStride;

            TimelineGUI.MarkerLane(new Rect(rightX, ry, rightW, TimelineGUI.LaneHeight), "종료", TimelineGUI.Transition, _attackEnd);
            ry += TimelineGUI.LaneStride;

            var lanesArea = new Rect(rightX, lanesTop, rightW, ry - lanesTop - TimelineGUI.LaneGap);
            var newHead = TimelineGUI.PlayheadOverlay(lanesArea, _playhead);
            if (!Mathf.Approximately(newHead, _playhead))
            {
                _playhead = newHead;
                _playing = false;
            }

            ry += 6f;

            var chipCursor = new Rect(rightX + TimelineGUI.LabelWidth, ry, 0f, 16f);
            TimelineGUI.ActiveChip(ref chipCursor, "유도", TimelineGUI.Cancel, TrackingActive());
            TimelineGUI.ActiveChip(ref chipCursor, melee ? "판정" : "소환", TimelineGUI.Hitbox,
                melee ? _playhead >= Mathf.Min(_hitStart.floatValue, _hitEnd.floatValue) && _playhead <= Mathf.Max(_hitStart.floatValue, _hitEnd.floatValue)
                      : _playhead >= _hitStart.floatValue);
            TimelineGUI.ActiveChip(ref chipCursor, "연계", TimelineGUI.Combo, hasCombo && _playhead >= _comboEnable.floatValue);

            EditorGUILayout.Space(4f);

            var colW = (rightW - 8f) / 3f;
            var row = GUILayoutUtility.GetRect(0f, k_FieldH, GUILayout.ExpandWidth(true));
            using (new EditorGUI.DisabledScope(_trackingType.enumValueIndex != (int)RotationTrackingType.OnlyStartup))
                MiniFloat(new Rect(rightX, row.y, colW, k_FieldH), "유도 종료", _stopTracking);
            MiniFloat(new Rect(rightX + colW + 4f, row.y, colW, k_FieldH), melee ? "판정 시작" : "소환 시점", _hitStart);
            using (new EditorGUI.DisabledScope(!melee))
                MiniFloat(new Rect(rightX + 2f * (colW + 4f), row.y, colW, k_FieldH), "판정 종료", _hitEnd);

            row = GUILayoutUtility.GetRect(0f, k_FieldH + 3f, GUILayout.ExpandWidth(true));
            using (new EditorGUI.DisabledScope(!hasCombo))
                MiniFloat(new Rect(rightX, row.y, colW, k_FieldH), "연계 시점", _comboEnable);
            MiniFloat(new Rect(rightX + colW + 4f, row.y, colW, k_FieldH), "종료", _attackEnd);
            MiniFloat(new Rect(rightX + 2f * (colW + 4f), row.y, colW, k_FieldH), "전환 시간", _transitionDuration);

            EditorGUILayout.Space(8f);
            DrawChain();

            EditorGUILayout.Space(8f);
            DrawInspection();

            EditorGUILayout.Space(8f);
            DrawSettings();

            EditorGUILayout.Space(4f);
            DrawRigSettings();

            serializedObject.ApplyModifiedProperties();

            TickPlayback();
        }

        // -------------------------------------------------------------- summary

        void DrawSummary()
        {
            var melee = _method.enumValueIndex == (int)AttackMethod.Melee;
            var clip = _clip.objectReferenceValue as AnimationClip;
            var duration = clip != null ? clip.length / Mathf.Max(0.01f, _speed.floatValue) : 0f;

            var area = GUILayoutUtility.GetRect(0f, k_SummaryRowH * 4f + 10f, GUILayout.ExpandWidth(true));

            if (Event.current.type == EventType.Repaint)
            {
                EditorGUI.DrawRect(area, new Color(0.17f, 0.17f, 0.17f, 1f));
                EditorGUI.DrawRect(new Rect(area.x, area.y + k_SummaryRowH * 2f + 5f, area.width, 1f), new Color(0.24f, 0.24f, 0.24f, 1f));
            }

            var cell = (area.width - 10f) / 5f;
            var top = area.y + 4f;
            var bottom = area.y + k_SummaryRowH * 2f + 9f;

            var x = area.x + 5f;
            Cell(ref x, top, cell, "사거리", _minDistance.floatValue.ToString("0.#") + " ~ " + _maxDistance.floatValue.ToString("0.#") + " m");
            Cell(ref x, top, cell, "쿨타임", _cooldown.floatValue.ToString("0.#") + " s");
            Cell(ref x, top, cell, "가중치", _weight.intValue.ToString());
            Cell(ref x, top, cell, "페이즈", _requiredPhase.intValue <= 1 ? "1~" : _requiredPhase.intValue + " 이상");
            Cell(ref x, top, cell, "재생 길이", duration > 0f ? duration.ToString("0.00") + " s" : "클립 없음");

            x = area.x + 5f;
            Cell(ref x, bottom, cell, "데미지", _damage.floatValue.ToString("0.#"));
            Cell(ref x, bottom, cell, "강인도", _poise.floatValue.ToString("0.#"));
            Cell(ref x, bottom, cell, "슈퍼아머", _hyperArmor.boolValue ? "적용" : "없음",
                 _hyperArmor.boolValue ? Color.white : new Color(0.55f, 0.55f, 0.55f, 1f));
            Cell(ref x, bottom, cell, melee ? "히트박스" : "소환 프리팹", HitboxText(melee), TimelineGUI.Hitbox);
            Cell(ref x, bottom, cell, melee ? "판정 구간" : "소환 시점", WindowText(melee, duration), TimelineGUI.Hitbox);
        }

        string HitboxText(bool melee)
        {
            if (melee)
                return ((HitboxType)_hitbox.enumValueIndex).ToString();

            return _prefab.objectReferenceValue != null ? _prefab.objectReferenceValue.name : "비어 있음";
        }

        string WindowText(bool melee, float duration)
        {
            if (!melee)
                return _hitStart.floatValue.ToString("0.00");

            var text = _hitStart.floatValue.ToString("0.00") + " – " + _hitEnd.floatValue.ToString("0.00");
            if (duration > 0f)
                text += "  (" + ((_hitEnd.floatValue - _hitStart.floatValue) * duration).ToString("0.00") + "s)";

            return text;
        }

        static void Cell(ref float x, float y, float w, string label, string value)
        {
            Cell(ref x, y, w, label, value, Color.white);
        }

        static void Cell(ref float x, float y, float w, string label, string value, Color valueColor)
        {
            if (Event.current.type == EventType.Repaint)
            {
                var ls = new GUIStyle(EditorStyles.miniLabel);
                ls.normal.textColor = new Color(0.55f, 0.55f, 0.55f, 1f);
                GUI.Label(new Rect(x, y, w, k_SummaryRowH), label, ls);

                var vs = new GUIStyle(EditorStyles.miniLabel);
                vs.fontStyle = FontStyle.Bold;
                vs.normal.textColor = valueColor;
                GUI.Label(new Rect(x, y + k_SummaryRowH - 2f, w, k_SummaryRowH), value, vs);
            }

            x += w;
        }

        // ---------------------------------------------------------------- chain

        void DrawChain()
        {
            var first = target as BossAttack_SO_Data;
            if (first == null || first.nextCombo == null)
                return;

            SectionHeader("연계 순서", "클릭하면 해당 패턴으로 이동");

            var row = GUILayoutUtility.GetRect(0f, 18f, GUILayout.ExpandWidth(true));

            var ls = new GUIStyle(EditorStyles.miniLabel);
            ls.normal.textColor = new Color(0.62f, 0.62f, 0.62f, 1f);

            var x = row.x + 9f;
            var cur = first;
            var guard = 0;

            while (cur != null && guard++ < 8)
            {
                var label = string.IsNullOrEmpty(cur.attackName) ? cur.name : cur.attackName;
                var w = EditorStyles.miniButton.CalcSize(new GUIContent(label)).x + 8f;
                if (x + w > row.xMax)
                    break;

                var r = new Rect(x, row.y, w, 16f);

                using (new EditorGUI.DisabledScope(cur == first))
                {
                    if (GUI.Button(r, label, EditorStyles.miniButton))
                        Selection.activeObject = cur;
                }

                x += w;
                cur = cur.nextCombo;

                if (cur != null && x + 12f < row.xMax)
                {
                    GUI.Label(new Rect(x, row.y, 12f, row.height), "›", ls);
                    x += 12f;
                }
            }
        }

        // ------------------------------------------------------------- warnings

        struct Issue
        {
            public string text;
            public MessageType type;
        }

        void DrawInspection()
        {
            var issues = CollectIssues();
            SectionHeader("검사", issues.Count == 0 ? "이상 없음" : issues.Count + "건 확인 필요");

            if (issues.Count == 0)
            {
                Hint("거리 · 쿨타임 · 타이밍 설정에서 발견된 문제가 없습니다.");
                return;
            }

            for (var i = 0; i < issues.Count; i++)
                EditorGUILayout.HelpBox(issues[i].text, issues[i].type);
        }

        List<Issue> CollectIssues()
        {
            var list = new List<Issue>();
            var melee = _method.enumValueIndex == (int)AttackMethod.Melee;
            var hs = _hitStart.floatValue;
            var he = _hitEnd.floatValue;
            var end = _attackEnd.floatValue;
            var combo = _comboEnable.floatValue;

            if (_clip.objectReferenceValue == null)
                Add(list, MessageType.Error, "클립이 비어 있습니다. Attack 상태가 진입하자마자 Decision 으로 되돌아갑니다.");

            if (melee && he < hs)
                Add(list, MessageType.Error, "판정 종료(" + he.ToString("0.00") + ")가 시작(" + hs.ToString("0.00") + ")보다 앞에 있습니다. 히트박스가 한 번도 켜지지 않습니다.");

            if (!melee && _prefab.objectReferenceValue == null)
                Add(list, MessageType.Error, "SpawnPrefab 방식인데 소환 프리팹이 비어 있습니다.");

            if (melee && end < he)
                Add(list, MessageType.Warning, "종료(" + end.ToString("0.00") + ")가 판정 종료(" + he.ToString("0.00") + ")보다 앞에 있습니다. 히트박스가 켜진 채로 상태를 빠져나갑니다.");

            if (_nextCombo.objectReferenceValue != null && combo > end)
                Add(list, MessageType.Warning, "연계 시점(" + combo.ToString("0.00") + ")이 종료(" + end.ToString("0.00") + ")보다 뒤에 있습니다. nextCombo 가 실행되지 않습니다.");

            if (_maxDistance.floatValue <= _minDistance.floatValue)
                Add(list, MessageType.Warning, "사거리 하한이 상한보다 크거나 같습니다. 후보로 뽑히지 않습니다.");

            if (_weight.intValue <= 0)
                Add(list, MessageType.Warning, "가중치가 0 이라 룰렛에서 선택되지 않습니다.");

            return list;
        }

        static void Add(List<Issue> list, MessageType type, string text)
        {
            var issue = new Issue();
            issue.text = text;
            issue.type = type;
            list.Add(issue);
        }

        // -------------------------------------------------------------- settings

        void DrawSettings()
        {
            var melee = _method.enumValueIndex == (int)AttackMethod.Melee;

            if (Fold("trigger", "발동 조건",
                     _minDistance.floatValue.ToString("0.#") + " ~ " + _maxDistance.floatValue.ToString("0.#") + " m  ·  쿨 " +
                     _cooldown.floatValue.ToString("0.#") + "s  ·  w " + _weight.intValue))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_attackName, new GUIContent("이름"));
                EditorGUILayout.PropertyField(_minDistance, new GUIContent("최소 거리"));
                EditorGUILayout.PropertyField(_maxDistance, new GUIContent("최대 거리"));
                EditorGUILayout.PropertyField(_cooldown, new GUIContent("쿨타임"));
                EditorGUILayout.PropertyField(_weight, new GUIContent("가중치"));
                EditorGUILayout.PropertyField(_requiredPhase, new GUIContent("필요 페이즈"));
                EditorGUILayout.PropertyField(_validMinAngle, new GUIContent("각도 하한"));
                EditorGUILayout.PropertyField(_validMaxAngle, new GUIContent("각도 상한"));
                Hint("각도 조건은 현재 Decision 필터에서 사용하지 않습니다.");
                EditorGUI.indentLevel--;
            }

            if (Fold("anim", "애니메이션", AnimSummary()))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_speed, new GUIContent("속도 배율"));
                EditorGUILayout.PropertyField(_transitionDuration, new GUIContent("전환 시간"));
                EditorGUILayout.PropertyField(_useRootMotion, new GUIContent("루트 모션"));
                EditorGUILayout.PropertyField(_trackingType, new GUIContent("유도 방식"));
                EditorGUILayout.PropertyField(_turnSpeed, new GUIContent("회전 속도"));
                EditorGUI.indentLevel--;
            }

            if (Fold("combat", "전투 · 판정",
                     "데미지 " + _damage.floatValue.ToString("0.#") + "  ·  " + (melee ? HitboxText(true) : "프리팹 소환")))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_damage, new GUIContent("데미지"));
                EditorGUILayout.PropertyField(_poise, new GUIContent("강인도"));
                EditorGUILayout.PropertyField(_hyperArmor, new GUIContent("슈퍼아머"));
                EditorGUILayout.PropertyField(_method, new GUIContent("공격 방식"));

                if (melee)
                {
                    EditorGUILayout.PropertyField(_hitbox, new GUIContent("히트박스 부위"));
                }
                else
                {
                    EditorGUILayout.PropertyField(_prefab, new GUIContent("소환 프리팹"));
                    EditorGUILayout.PropertyField(_spawnOnTarget, new GUIContent("타깃 발밑 소환"));
                    using (new EditorGUI.DisabledScope(_spawnOnTarget.boolValue))
                        EditorGUILayout.PropertyField(_spawnOffset, new GUIContent("소환 오프셋"));
                }

                EditorGUI.indentLevel--;
            }

            if (Fold("combo", "연계", _nextCombo.objectReferenceValue != null ? "→ " + _nextCombo.objectReferenceValue.name : "없음"))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_nextCombo, new GUIContent("다음 패턴"));
                EditorGUI.indentLevel--;
            }
        }

        string AnimSummary()
        {
            var clip = _clip.objectReferenceValue as AnimationClip;
            if (clip == null)
                return "클립 없음";

            return clip.name + "  ·  ×" + _speed.floatValue.ToString("0.##");
        }

        /// <summary>왼쪽 액센트 바 + 굵은 제목 + 오른쪽 요약. 파트를 나누는 고정 헤더.</summary>
        static void SectionHeader(string title, string summary)
        {
            var r = GUILayoutUtility.GetRect(0f, k_HeaderH, GUILayout.ExpandWidth(true));
            DrawHeaderChrome(r, title, summary, k_BarInfo, null);
        }

        /// <summary>같은 모양에 ▸ / ▾ 를 붙인 접이식 헤더. 바는 열림 여부와 상관없이 밝은 회색을 유지한다.</summary>
        bool Fold(string key, string title, string summary)
        {
            var r = GUILayoutUtility.GetRect(0f, k_HeaderH, GUILayout.ExpandWidth(true));
            var stateKey = _keyPrefix + key;
            var open = SessionState.GetBool(stateKey, false);

            DrawHeaderChrome(r, title, summary, k_BarEdit, open ? "▾" : "▸");
            EditorGUIUtility.AddCursorRect(r, MouseCursor.Link);

            var e = Event.current;
            if (e.type == EventType.MouseDown && e.button == 0 && r.Contains(e.mousePosition))
            {
                open = !open;
                SessionState.SetBool(stateKey, open);
                e.Use();
            }

            return open;
        }

        static void DrawHeaderChrome(Rect r, string title, string summary, Color bar, string glyph)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            EditorGUI.DrawRect(new Rect(r.x, r.y + 4f, 3f, r.height - 8f), bar);

            var x = r.x + 9f;

            if (!string.IsNullOrEmpty(glyph))
            {
                var gs = new GUIStyle(EditorStyles.miniLabel);
                gs.normal.textColor = new Color(0.62f, 0.62f, 0.62f, 1f);
                GUI.Label(new Rect(x, r.y, 12f, r.height), glyph, gs);
                x += 13f;
            }

            var ts = new GUIStyle(EditorStyles.label);
            ts.fontStyle = FontStyle.Bold;
            ts.normal.textColor = new Color(0.86f, 0.86f, 0.86f, 1f);
            GUI.Label(new Rect(x, r.y, 220f, r.height), title, ts);

            var ss = new GUIStyle(EditorStyles.miniLabel);
            ss.alignment = TextAnchor.MiddleRight;
            ss.normal.textColor = new Color(0.55f, 0.55f, 0.55f, 1f);
            GUI.Label(new Rect(x + 220f, r.y, r.xMax - x - 222f, r.height), summary, ss);
        }

        string AttackTitle()
        {
            var data = target as BossAttack_SO_Data;
            if (data == null) return string.Empty;
            return string.IsNullOrEmpty(data.attackName) ? data.name : data.attackName;
        }

        string DurationText()
        {
            var clip = _clip.objectReferenceValue as AnimationClip;
            if (clip == null) return "normalizedTime 0 ~ 1";

            var duration = clip.length / Mathf.Max(0.01f, _speed.floatValue);
            return "normalizedTime 0 ~ 1  ·  실제 " + duration.ToString("0.00") + "초";
        }

        /// <summary>섹션 제목과 같은 선에 맞춘 회색 안내 한 줄.</summary>
        static void Hint(string text)
        {
            var r = EditorGUI.IndentedRect(GUILayoutUtility.GetRect(0f, 16f, GUILayout.ExpandWidth(true)));

            if (Event.current.type != EventType.Repaint)
                return;

            var st = new GUIStyle(EditorStyles.miniLabel);
            st.alignment = TextAnchor.MiddleLeft;
            st.normal.textColor = new Color(0.5f, 0.5f, 0.5f, 1f);

            var x = EditorGUI.indentLevel > 0 ? r.x : r.x + 9f;
            GUI.Label(new Rect(x, r.y, r.xMax - x, r.height), text, st);
        }

        // --------------------------------------------------------------- preview

        void DrawRigSettings()
        {
            _showRigSettings = EditorGUILayout.Foldout(_showRigSettings, "미리보기 설정", true);
            if (!_showRigSettings)
                return;

            EditorGUI.indentLevel++;

            var current = PreviewRigSettings.Prefab;
            var next = (GameObject)EditorGUILayout.ObjectField("리그 프리팹", current, typeof(GameObject), false);

            if (next != current)
            {
                PreviewRigSettings.Prefab = next;

                if (_preview != null)
                {
                    _preview.Dispose();
                    _preview = null;
                }
            }

            EditorGUI.indentLevel--;
        }

        void DrawPreview(Rect rect, AnimationClip clip)
        {
            if (_preview == null)
                _preview = new ClipPosePreview();

            _preview.Draw(rect, clip, _playhead, PreviewRigSettings.Prefab, _stripRootMotion);

            var ctrl = new Rect(rect.x, rect.yMax, rect.width, k_CtrlH);
            if (Event.current.type == EventType.Repaint)
                EditorGUI.DrawRect(ctrl, new Color(0.17f, 0.17f, 0.17f, 1f));

            var btn = new Rect(ctrl.x + 3f, ctrl.y + 2f, 26f, 16f);
            if (GUI.Button(btn, _playing ? "■" : "▶", EditorStyles.miniButton))
            {
                _playing = !_playing;

                if (_playing && clip != null && clip.length > 0f)
                    _playT0 = EditorApplication.timeSinceStartup - _playhead * clip.length;
            }

            var rm = new Rect(btn.xMax + 3f, ctrl.y + 2f, 34f, 16f);
            var canStrip = _preview.CanStripRootMotion;
            var tip = canStrip
                ? "켜면 제자리에서 동작만 보입니다. 끄면 루트 모션이 그대로 재생됩니다."
                : "리그 프리팹에서 Animator 를 찾지 못해 루트 모션을 되돌릴 수 없습니다.";

            using (new EditorGUI.DisabledScope(!canStrip))
                _stripRootMotion = GUI.Toggle(rm, _stripRootMotion && canStrip,
                    new GUIContent("RM", tip), EditorStyles.miniButton);

            var len = clip != null ? clip.length : 0f;
            var info = _playhead.ToString("0.00") + " / " + (_playhead * len).ToString("0.00") + "s";
            var st = new GUIStyle(EditorStyles.miniLabel);
            st.alignment = TextAnchor.MiddleRight;
            st.normal.textColor = new Color(0.62f, 0.62f, 0.62f, 1f);
            GUI.Label(new Rect(rm.xMax + 4f, ctrl.y + 2f, ctrl.xMax - rm.xMax - 8f, 16f), info, st);
        }

        void TickPlayback()
        {
            if (!_playing)
                return;

            var clip = _clip.objectReferenceValue as AnimationClip;
            if (clip == null || clip.length <= 0f)
            {
                _playing = false;
                return;
            }

            var t = (float)((EditorApplication.timeSinceStartup - _playT0) / clip.length);
            _playhead = Mathf.Repeat(t, 1f);
            Repaint();
        }

        bool TrackingActive()
        {
            switch ((RotationTrackingType)_trackingType.enumValueIndex)
            {
                case RotationTrackingType.Continuous: return true;
                case RotationTrackingType.OnlyStartup: return _playhead <= _stopTracking.floatValue;
                default: return false;
            }
        }

        static void MiniFloat(Rect r, string label, SerializedProperty p, float labelW = 62f)
        {
            var lr = new Rect(r.x, r.y, labelW, r.height);
            var fr = new Rect(r.x + labelW, r.y, Mathf.Max(28f, r.width - labelW), r.height);

            var ls = new GUIStyle(EditorStyles.miniLabel);
            ls.normal.textColor = new Color(0.68f, 0.68f, 0.68f, 1f);
            GUI.Label(lr, label, ls);

            EditorGUI.BeginChangeCheck();
            var v = EditorGUI.FloatField(fr, p.floatValue, EditorStyles.miniTextField);
            if (EditorGUI.EndChangeCheck())
                p.floatValue = v;
        }
    }
}
