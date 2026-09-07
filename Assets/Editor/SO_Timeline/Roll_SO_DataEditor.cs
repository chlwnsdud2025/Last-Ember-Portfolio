using UnityEditor;
using UnityEngine;

namespace SOTimeline.EditorTools
{
    [CustomEditor(typeof(Roll_SO_Data))]
    internal sealed class Roll_SO_DataEditor : Editor
    {
        const float k_PreviewW = 168f;
        const float k_PreviewH = 150f;
        const float k_CtrlH = 20f;
        const float k_FieldH = 18f;

        SerializedProperty _clip;
        SerializedProperty _iframeStart, _iframeEnd;
        SerializedProperty _preInput, _cancel, _end;
        SerializedProperty _stamina;

        ClipPosePreview _preview;
        float _playhead = 0.25f;
        bool _playing;
        double _playT0;
        bool _showRigSettings;
        bool _stripRootMotion = true;

        void OnEnable()
        {
            _clip = serializedObject.FindProperty("clip");
            _iframeStart = serializedObject.FindProperty("iframeStart");
            _iframeEnd = serializedObject.FindProperty("iframeEnd");
            _preInput = serializedObject.FindProperty("preInputWindowStart");
            _cancel = serializedObject.FindProperty("cancelWindow");
            _end = serializedObject.FindProperty("animationEnd");
            _stamina = serializedObject.FindProperty("staminaCost");
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

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // PropertyField 는 Roll_SO_Data.clip 위의 [Header("Animation")] 까지 그려서 라벨이 겹친다.
            EditorGUI.BeginChangeCheck();
            var picked = EditorGUILayout.ObjectField("애니메이션 클립", _clip.objectReferenceValue, typeof(AnimationClip), false);
            if (EditorGUI.EndChangeCheck())
                _clip.objectReferenceValue = picked;

            EditorGUILayout.Space(6f);

            var body = GUILayoutUtility.GetRect(0f, k_PreviewH + k_CtrlH, GUILayout.ExpandWidth(true));

            var previewRect = new Rect(body.x, body.y, k_PreviewW, k_PreviewH);
            DrawPreview(previewRect, _clip.objectReferenceValue as AnimationClip);

            var rightX = body.x + k_PreviewW + 8f;
            var rightW = Mathf.Max(120f, body.xMax - rightX);
            var ry = body.y;

            TimelineGUI.Ruler(new Rect(rightX, ry, rightW, TimelineGUI.RulerHeight));
            ry += TimelineGUI.RulerHeight;

            var lanesTop = ry;

            TimelineGUI.BandLane(new Rect(rightX, ry, rightW, TimelineGUI.LaneHeight), "무적", TimelineGUI.IFrame, _iframeStart, _iframeEnd);
            ry += TimelineGUI.LaneStride;

            TimelineGUI.TailLane(new Rect(rightX, ry, rightW, TimelineGUI.LaneHeight), "선입력", TimelineGUI.Combo, _preInput);
            ry += TimelineGUI.LaneStride;

            TimelineGUI.TailLane(new Rect(rightX, ry, rightW, TimelineGUI.LaneHeight), "캔슬 가능", TimelineGUI.Cancel, _cancel);
            ry += TimelineGUI.LaneStride;

            TimelineGUI.MarkerLane(new Rect(rightX, ry, rightW, TimelineGUI.LaneHeight), "종료", TimelineGUI.Transition, _end);
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
            TimelineGUI.ActiveChip(ref chipCursor, "무적", TimelineGUI.IFrame,
                _playhead >= Mathf.Min(_iframeStart.floatValue, _iframeEnd.floatValue) &&
                _playhead <= Mathf.Max(_iframeStart.floatValue, _iframeEnd.floatValue));
            TimelineGUI.ActiveChip(ref chipCursor, "선입력", TimelineGUI.Combo, _playhead >= _preInput.floatValue);
            TimelineGUI.ActiveChip(ref chipCursor, "캔슬", TimelineGUI.Cancel, _playhead >= _cancel.floatValue);

            EditorGUILayout.Space(4f);

            var colW = (rightW - 8f) / 3f;
            var row = GUILayoutUtility.GetRect(0f, k_FieldH, GUILayout.ExpandWidth(true));
            MiniFloat(new Rect(rightX, row.y, colW, k_FieldH), "무적 시작", _iframeStart);
            MiniFloat(new Rect(rightX + colW + 4f, row.y, colW, k_FieldH), "무적 종료", _iframeEnd);
            MiniFloat(new Rect(rightX + 2f * (colW + 4f), row.y, colW, k_FieldH), "선입력", _preInput);

            row = GUILayoutUtility.GetRect(0f, k_FieldH + 3f, GUILayout.ExpandWidth(true));
            MiniFloat(new Rect(rightX, row.y, colW, k_FieldH), "캔슬", _cancel);
            MiniFloat(new Rect(rightX + colW + 4f, row.y, colW, k_FieldH), "종료", _end);
            MiniFloat(new Rect(rightX + 2f * (colW + 4f), row.y, colW, k_FieldH), "스태미나", _stamina);

            EditorGUILayout.Space(6f);
            DrawWarnings();

            EditorGUILayout.Space(4f);
            DrawRigSettings();

            serializedObject.ApplyModifiedProperties();

            TickPlayback();
        }

        void DrawWarnings()
        {
            var iStart = _iframeStart.floatValue;
            var iEnd = _iframeEnd.floatValue;
            var pre = _preInput.floatValue;
            var can = _cancel.floatValue;
            var end = _end.floatValue;

            if (iStart > iEnd)
                EditorGUILayout.HelpBox("무적 시작(" + iStart.ToString("0.00") + ")이 종료(" + iEnd.ToString("0.00") + ")보다 뒤에 있습니다. 무적이 한 번도 켜지지 않습니다.", MessageType.Error);

            if (OutOfRange(iStart) || OutOfRange(iEnd) || OutOfRange(pre) || OutOfRange(can) || OutOfRange(end))
                EditorGUILayout.HelpBox("0.0 ~ 1.0 범위를 벗어난 값이 있습니다.", MessageType.Error);

            if (can < pre)
                EditorGUILayout.HelpBox("캔슬 시점(" + can.ToString("0.00") + ")이 선입력 시작(" + pre.ToString("0.00") + ")보다 앞에 있습니다. 선입력을 받기 전에 캔슬이 열립니다.", MessageType.Warning);

            if (end < can)
                EditorGUILayout.HelpBox("종료(" + end.ToString("0.00") + ")가 캔슬 시점(" + can.ToString("0.00") + ")보다 앞에 있습니다.", MessageType.Warning);

            if (iEnd > can)
                EditorGUILayout.HelpBox("무적이 캔슬 가능 시점 이후까지 이어집니다. 캔슬 직후에도 무적이 남습니다.", MessageType.Info);
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

        static bool OutOfRange(float v)
        {
            return v < 0f || v > 1f;
        }
    }
}
