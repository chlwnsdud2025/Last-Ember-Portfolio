using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace SOTimeline.EditorTools
{
    [CustomEditor(typeof(Attack_SO_Data))]
    internal sealed class Attack_SO_DataEditor : Editor
    {
        const float k_HeaderH = 20f;
        const float k_PreviewW = 168f;
        const float k_PreviewH = 150f;
        const float k_CtrlH = 20f;
        const float k_FieldH = 18f;

        SerializedProperty _weaponName;
        SerializedProperty _idle, _walk, _run, _sprint;
        SerializedProperty _combos;

        ReorderableList _list;
        ClipPosePreview _preview;

        int _openIndex = -1;
        float _playhead = 0.25f;
        bool _playing;
        double _playT0;
        bool _showRigSettings;
        bool _stripRootMotion = true;

        // ------------------------------------------------------------ lifecycle

        void OnEnable()
        {
            _weaponName = serializedObject.FindProperty("weaponName");
            _idle = serializedObject.FindProperty("idleClip");
            _walk = serializedObject.FindProperty("walkClip");
            _run = serializedObject.FindProperty("runClip");
            _sprint = serializedObject.FindProperty("sprintClip");
            _combos = serializedObject.FindProperty("comboAttacks");

            BuildList();
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

        // -------------------------------------------------------------- inspector

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_weaponName, new GUIContent("무기 이름"));

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("이동 애니메이션", EditorStyles.boldLabel);
            ClipField("Idle", _idle);
            ClipField("Walk", _walk);
            ClipField("Run", _run);
            ClipField("Sprint", _sprint);

            EditorGUILayout.Space(8f);
            _list.DoLayoutList();

            EditorGUILayout.Space(4f);
            DrawRigSettings();

            serializedObject.ApplyModifiedProperties();

            TickPlayback();
        }

        void TickPlayback()
        {
            if (!_playing || _openIndex < 0 || _openIndex >= _combos.arraySize)
                return;

            var clip = ClipAt(_openIndex);
            if (clip == null || clip.length <= 0f)
            {
                _playing = false;
                return;
            }

            var t = (float)((EditorApplication.timeSinceStartup - _playT0) / clip.length);
            _playhead = Mathf.Repeat(t, 1f);
            Repaint();
        }

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

            EditorGUILayout.HelpBox(
                "포즈를 입힐 리그된 프리팹입니다. 이 설정은 에디터에만 저장되며 SO 에셋에는 기록되지 않습니다.",
                MessageType.None);

            EditorGUI.indentLevel--;
        }

        // ------------------------------------------------------------------ list

        void BuildList()
        {
            _list = new ReorderableList(serializedObject, _combos, true, true, true, true);

            _list.drawHeaderCallback = r =>
            {
                EditorGUI.LabelField(r, "콤보 공격  (" + _combos.arraySize + "타)", EditorStyles.boldLabel);
            };

            _list.elementHeightCallback = ElementHeight;
            _list.drawElementCallback = DrawElement;

            _list.onRemoveCallback = l =>
            {
                ReorderableList.defaultBehaviours.DoRemoveButton(l);
                _openIndex = -1;
                _playing = false;
            };

            _list.onReorderCallback = l =>
            {
                _openIndex = -1;
                _playing = false;
            };
        }

        float ElementHeight(int index)
        {
            if (index != _openIndex)
                return k_HeaderH + 2f;

            var right = TimelineGUI.RulerHeight
                        + 4f * TimelineGUI.LaneStride
                        + 6f + 16f
                        + 8f + 2f * (k_FieldH + 3f)
                        + 6f + k_FieldH;

            var left = k_PreviewH + k_CtrlH;

            var warn = Warnings(index).Count;
            var warnH = warn > 0 ? 6f + warn * 32f : 0f;

            return k_HeaderH + 4f + Mathf.Max(left, right) + warnH + 8f;
        }

        void DrawElement(Rect rect, int index, bool active, bool focused)
        {
            var element = _combos.GetArrayElementAtIndex(index);
            var open = index == _openIndex;

            var header = new Rect(rect.x, rect.y + 1f, rect.width, k_HeaderH);
            DrawHeader(header, index, element, open);

            if (!open)
                return;

            var y = header.yMax + 4f;

            // 좌: 프리뷰 / 우: 타임라인
            var previewRect = new Rect(rect.x, y, k_PreviewW, k_PreviewH);
            DrawPreview(previewRect, index, ClipAt(index));

            var rightX = rect.x + k_PreviewW + 8f;
            var rightW = Mathf.Max(120f, rect.xMax - rightX);
            var ry = y;

            TimelineGUI.Ruler(new Rect(rightX, ry, rightW, TimelineGUI.RulerHeight));
            ry += TimelineGUI.RulerHeight;

            var lanesTop = ry;

            var hitA = element.FindPropertyRelative("hitboxStartTime");
            var hitB = element.FindPropertyRelative("hitboxEndTime");
            var cmbA = element.FindPropertyRelative("comboWindowStart");
            var cmbB = element.FindPropertyRelative("comboWindowEnd");
            var trans = element.FindPropertyRelative("comboTransitionPoint");
            var cancel = element.FindPropertyRelative("cancelWindowStart");

            TimelineGUI.BandLane(new Rect(rightX, ry, rightW, TimelineGUI.LaneHeight), "히트박스", TimelineGUI.Hitbox, hitA, hitB);
            ry += TimelineGUI.LaneStride;

            TimelineGUI.BandLane(new Rect(rightX, ry, rightW, TimelineGUI.LaneHeight), "콤보 입력", TimelineGUI.Combo, cmbA, cmbB);
            ry += TimelineGUI.LaneStride;

            TimelineGUI.MarkerLane(new Rect(rightX, ry, rightW, TimelineGUI.LaneHeight), "전환 시점", TimelineGUI.Transition, trans);
            ry += TimelineGUI.LaneStride;

            TimelineGUI.TailLane(new Rect(rightX, ry, rightW, TimelineGUI.LaneHeight), "캔슬 가능", TimelineGUI.Cancel, cancel);
            ry += TimelineGUI.LaneStride;

            var lanesArea = new Rect(rightX, lanesTop, rightW, ry - lanesTop - TimelineGUI.LaneGap);
            var newHead = TimelineGUI.PlayheadOverlay(lanesArea, _playhead);
            if (!Mathf.Approximately(newHead, _playhead))
            {
                _playhead = newHead;
                _playing = false;
            }

            ry += 6f;

            // 현재 시점에 열려 있는 구간
            var chipCursor = new Rect(rightX + TimelineGUI.LabelWidth, ry, 0f, 16f);
            TimelineGUI.ActiveChip(ref chipCursor, "히트박스", TimelineGUI.Hitbox, Between(_playhead, hitA.floatValue, hitB.floatValue));
            TimelineGUI.ActiveChip(ref chipCursor, "콤보 입력", TimelineGUI.Combo, Between(_playhead, cmbA.floatValue, cmbB.floatValue));
            TimelineGUI.ActiveChip(ref chipCursor, "캔슬", TimelineGUI.Cancel, _playhead >= cancel.floatValue);
            ry += 16f + 8f;

            // 수치 입력
            var colW = (rightW - 8f) / 3f;
            MiniFloat(new Rect(rightX, ry, colW, k_FieldH), "Hit 시작", hitA);
            MiniFloat(new Rect(rightX + colW + 4f, ry, colW, k_FieldH), "Hit 종료", hitB);
            MiniFloat(new Rect(rightX + 2f * (colW + 4f), ry, colW, k_FieldH), "전환", trans);
            ry += k_FieldH + 3f;

            MiniFloat(new Rect(rightX, ry, colW, k_FieldH), "콤보 시작", cmbA);
            MiniFloat(new Rect(rightX + colW + 4f, ry, colW, k_FieldH), "콤보 종료", cmbB);
            MiniFloat(new Rect(rightX + 2f * (colW + 4f), ry, colW, k_FieldH), "캔슬", cancel);
            ry += k_FieldH + 6f;

            // 스탯
            var dmg = element.FindPropertyRelative("damageMultiplier");
            var sta = element.FindPropertyRelative("steminareduce");
            MiniFloat(new Rect(rightX, ry, colW * 1.5f, k_FieldH), "데미지 배율", dmg, 74f);
            MiniFloat(new Rect(rightX + colW * 1.5f + 6f, ry, colW * 1.5f, k_FieldH), "스태미나", sta, 74f);
            ry += k_FieldH;

            // 경고
            var warnings = Warnings(index);
            if (warnings.Count > 0)
            {
                var wy = Mathf.Max(ry, previewRect.yMax + k_CtrlH) + 6f;
                foreach (var w in warnings)
                {
                    EditorGUI.HelpBox(new Rect(rect.x, wy, rect.width, 30f), w.Item1, w.Item2);
                    wy += 32f;
                }
            }
        }

        void DrawHeader(Rect rect, int index, SerializedProperty element, bool open)
        {
            var bg = open ? new Color(0.25f, 0.29f, 0.34f, 1f) : new Color(0.22f, 0.22f, 0.22f, 0.5f);
            if (Event.current.type == EventType.Repaint)
                EditorGUI.DrawRect(rect, bg);

            var arrow = new Rect(rect.x + 3f, rect.y + 2f, 14f, 16f);
            GUI.Label(arrow, open ? "▾" : "▸", EditorStyles.miniLabel);

            var badge = new Rect(arrow.xMax + 2f, rect.y + 3f, 22f, 14f);
            if (Event.current.type == EventType.Repaint)
            {
                EditorGUI.DrawRect(badge, open ? new Color(0.36f, 0.64f, 0.79f, 1f) : new Color(0.32f, 0.32f, 0.32f, 1f));
                var bs = new GUIStyle(EditorStyles.miniLabel);
                bs.alignment = TextAnchor.MiddleCenter;
                bs.normal.textColor = open ? new Color(0.07f, 0.13f, 0.17f) : new Color(0.78f, 0.78f, 0.78f);
                GUI.Label(badge, (index + 1).ToString("00"), bs);
            }

            // 접힌 상태에서도 클립을 바로 갈아끼울 수 있도록 헤더에 필드를 둔다.
            var summaryW = 150f;
            var clipX = badge.xMax + 6f;
            var clipW = Mathf.Max(90f, rect.xMax - summaryW - 8f - clipX);
            var clipRect = new Rect(clipX, rect.y + 1f, clipW, 18f);

            // PropertyField 를 쓰면 AttackData.clip 위의 [Header("Animation")] 장식까지 같이 그려져
            // 레이아웃이 밀린다. ObjectField 로 직접 그려서 장식을 건너뛴다.
            var clipProp = element.FindPropertyRelative("clip");
            EditorGUI.BeginChangeCheck();
            var picked = EditorGUI.ObjectField(clipRect, clipProp.objectReferenceValue, typeof(AnimationClip), false);
            if (EditorGUI.EndChangeCheck())
                clipProp.objectReferenceValue = picked;

            var hitA = element.FindPropertyRelative("hitboxStartTime").floatValue;
            var hitB = element.FindPropertyRelative("hitboxEndTime").floatValue;
            var dmg = element.FindPropertyRelative("damageMultiplier").floatValue;

            var summary = "Hit " + hitA.ToString("0.00") + "–" + hitB.ToString("0.00") + "   ×" + dmg.ToString("0.0");
            var sumStyle = new GUIStyle(EditorStyles.miniLabel);
            sumStyle.alignment = TextAnchor.MiddleRight;
            sumStyle.normal.textColor = new Color(0.55f, 0.55f, 0.55f, 1f);
            GUI.Label(new Rect(rect.xMax - summaryW - 2f, rect.y + 2f, summaryW, 16f), summary, sumStyle);

            // 클립 필드를 뺀 나머지 헤더 영역을 누르면 펼쳐진다.
            EditorGUIUtility.AddCursorRect(new Rect(rect.x, rect.y, clipX - rect.x, rect.height), MouseCursor.Link);

            var e = Event.current;
            if (e.type == EventType.MouseDown && e.button == 0 &&
                rect.Contains(e.mousePosition) && !clipRect.Contains(e.mousePosition))
            {
                _openIndex = open ? -1 : index;
                _playing = false;
                _playhead = 0.25f;
                e.Use();
                Repaint();
            }
        }

        void DrawPreview(Rect rect, int index, AnimationClip clip)
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

        // ----------------------------------------------------------- helpers

        /// <summary>
        /// [Header] 장식을 건너뛰고 클립 필드만 그린다.
        /// PropertyField 를 쓰면 SO 에 붙은 [Header("Movement Animations")] 등이 함께 그려져
        /// 내가 붙인 라벨과 중복된다.
        /// </summary>
        static void ClipField(string label, SerializedProperty p)
        {
            EditorGUI.BeginChangeCheck();
            var picked = EditorGUILayout.ObjectField(label, p.objectReferenceValue, typeof(AnimationClip), false);
            if (EditorGUI.EndChangeCheck())
                p.objectReferenceValue = picked;
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

        static bool Between(float t, float a, float b)
        {
            return t >= Mathf.Min(a, b) && t <= Mathf.Max(a, b);
        }

        AnimationClip ClipAt(int index)
        {
            if (index < 0 || index >= _combos.arraySize)
                return null;

            return _combos.GetArrayElementAtIndex(index)
                          .FindPropertyRelative("clip").objectReferenceValue as AnimationClip;
        }

        List<System.Tuple<string, MessageType>> Warnings(int index)
        {
            var result = new List<System.Tuple<string, MessageType>>();

            if (index < 0 || index >= _combos.arraySize)
                return result;

            var el = _combos.GetArrayElementAtIndex(index);
            var hs = el.FindPropertyRelative("hitboxStartTime").floatValue;
            var he = el.FindPropertyRelative("hitboxEndTime").floatValue;
            var cs = el.FindPropertyRelative("comboWindowStart").floatValue;
            var ce = el.FindPropertyRelative("comboWindowEnd").floatValue;
            var tp = el.FindPropertyRelative("comboTransitionPoint").floatValue;
            var cw = el.FindPropertyRelative("cancelWindowStart").floatValue;

            if (hs > he)
                result.Add(Tuple("히트박스 시작(" + hs.ToString("0.00") + ")이 종료(" + he.ToString("0.00") + ")보다 뒤에 있습니다. 판정이 한 번도 켜지지 않습니다.", MessageType.Error));

            if (cs > ce)
                result.Add(Tuple("콤보 입력 시작(" + cs.ToString("0.00") + ")이 종료(" + ce.ToString("0.00") + ")보다 뒤에 있습니다. 콤보 입력을 받을 수 없습니다.", MessageType.Error));

            if (OutOfRange(hs) || OutOfRange(he) || OutOfRange(cs) || OutOfRange(ce) || OutOfRange(tp) || OutOfRange(cw))
                result.Add(Tuple("0.0 ~ 1.0 범위를 벗어난 값이 있습니다.", MessageType.Error));

            if (tp < cs)
                result.Add(Tuple("전환 시점(" + tp.ToString("0.00") + ")이 콤보 입력 시작(" + cs.ToString("0.00") + ")보다 앞에 있습니다.", MessageType.Warning));

            if (Mathf.Abs(hs - cs) < 0.005f && Mathf.Abs(he - ce) < 0.005f)
                result.Add(Tuple("히트박스 구간과 콤보 입력 구간이 완전히 같습니다. 의도한 값인지 확인해 주세요.", MessageType.Info));

            return result;
        }

        static bool OutOfRange(float v)
        {
            return v < 0f || v > 1f;
        }

        static System.Tuple<string, MessageType> Tuple(string s, MessageType t)
        {
            return new System.Tuple<string, MessageType>(s, t);
        }
    }
}
