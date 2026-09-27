using UnityEditor;
using UnityEngine;

namespace SOTimeline.EditorTools
{
    /// <summary>0.0 ~ 1.0 진행도 타임라인을 그리고 마커 드래그를 처리한다.</summary>
    internal static class TimelineGUI
    {
        public const float LabelWidth = 78f;
        public const float LaneHeight = 18f;
        public const float LaneGap = 3f;
        public const float RulerHeight = 13f;

        public static readonly Color TrackBg = new Color(0.16f, 0.16f, 0.16f, 1f);
        public static readonly Color Hitbox = new Color(0.66f, 0.27f, 0.25f, 1f);
        public static readonly Color Combo = new Color(0.18f, 0.49f, 0.50f, 1f);
        public static readonly Color Transition = new Color(0.85f, 0.68f, 0.41f, 1f);
        public static readonly Color Cancel = new Color(0.30f, 0.36f, 0.45f, 1f);
        public static readonly Color IFrame = new Color(0.26f, 0.44f, 0.25f, 1f);
        public static readonly Color PlayheadColor = new Color(1f, 1f, 1f, 0.9f);

        public static float LaneStride { get { return LaneHeight + LaneGap; } }

        public static Rect TrackOf(Rect row)
        {
            return new Rect(row.x + LabelWidth, row.y, Mathf.Max(1f, row.width - LabelWidth), row.height);
        }

        public static float ToNorm(Rect track, float x)
        {
            return Mathf.Clamp01((x - track.x) / Mathf.Max(1f, track.width));
        }

        public static float ToX(Rect track, float v)
        {
            return track.x + Mathf.Clamp01(v) * track.width;
        }

        static float Snap(float v)
        {
            return Mathf.Round(Mathf.Clamp01(v) * 100f) / 100f;
        }

        // ------------------------------------------------------------- ruler

        public static void Ruler(Rect row)
        {
            var track = TrackOf(row);

            var style = new GUIStyle(EditorStyles.miniLabel);
            style.fontSize = 9;
            style.normal.textColor = new Color(0.5f, 0.5f, 0.5f, 1f);

            for (var i = 0; i <= 4; i++)
            {
                var v = i * 0.25f;
                var x = ToX(track, v);

                if (Event.current.type == EventType.Repaint)
                    EditorGUI.DrawRect(new Rect(x, row.y + row.height - 4f, 1f, 4f), new Color(0.35f, 0.35f, 0.35f, 1f));

                var lr = new Rect(x - 22f, row.y, 44f, row.height - 3f);

                if (i == 0)
                {
                    style.alignment = TextAnchor.UpperLeft;
                    lr.x = x;
                }
                else if (i == 4)
                {
                    style.alignment = TextAnchor.UpperRight;
                    lr.x = x - 44f;
                }
                else
                {
                    style.alignment = TextAnchor.UpperCenter;
                }

                GUI.Label(lr, v.ToString("0.00"), style);
            }
        }

        // -------------------------------------------------------------- lane

        /// <summary>양 끝을 끌 수 있는 구간 레인.</summary>
        public static void BandLane(Rect row, string label, Color color, SerializedProperty from, SerializedProperty to)
        {
            var track = LaneBase(row, label, color);

            var a = Mathf.Clamp01(from.floatValue);
            var b = Mathf.Clamp01(to.floatValue);
            DrawBand(track, Mathf.Min(a, b), Mathf.Max(a, b), color);

            var na = DragHandle(track, a, label + "-a");
            if (!Mathf.Approximately(na, a))
                from.floatValue = na;

            var nb = DragHandle(track, b, label + "-b");
            if (!Mathf.Approximately(nb, b))
                to.floatValue = nb;
        }

        /// <summary>한 지점부터 끝까지 채우는 레인. 시작점만 끌 수 있다.</summary>
        public static void TailLane(Rect row, string label, Color color, SerializedProperty from)
        {
            var track = LaneBase(row, label, color);

            var a = Mathf.Clamp01(from.floatValue);
            DrawBand(track, a, 1f, color);

            var na = DragHandle(track, a, label + "-tail");
            if (!Mathf.Approximately(na, a))
                from.floatValue = na;
        }

        /// <summary>0에서 한 지점까지 채우는 레인. 끝점만 끌 수 있다.</summary>
        public static void HeadLane(Rect row, string label, Color color, SerializedProperty to)
        {
            var track = LaneBase(row, label, color);

            var b = Mathf.Clamp01(to.floatValue);
            DrawBand(track, 0f, b, color);

            var nb = DragHandle(track, b, label + "-head");
            if (!Mathf.Approximately(nb, b))
                to.floatValue = nb;
        }

        /// <summary>끌 수 없는 표시 전용 레인.</summary>
        public static void ReadonlyLane(Rect row, string label, Color color, float from, float to)
        {
            var track = LaneBase(row, label, color);
            DrawBand(track, Mathf.Min(from, to), Mathf.Max(from, to), color);
        }

        /// <summary>단일 시점 마커 레인.</summary>
        public static void MarkerLane(Rect row, string label, Color color, SerializedProperty at)
        {
            var track = LaneBase(row, label, color);

            var v = Mathf.Clamp01(at.floatValue);
            if (Event.current.type == EventType.Repaint)
                EditorGUI.DrawRect(new Rect(ToX(track, v) - 1.5f, track.y, 3f, track.height), color);

            var nv = DragHandle(track, v, label + "-marker");
            if (!Mathf.Approximately(nv, v))
                at.floatValue = nv;
        }

        static Rect LaneBase(Rect row, string label, Color color)
        {
            var track = TrackOf(row);

            if (Event.current.type == EventType.Repaint)
            {
                var style = new GUIStyle(EditorStyles.miniLabel);
                style.normal.textColor = Color.Lerp(color, Color.white, 0.45f);
                GUI.Label(new Rect(row.x, row.y, LabelWidth - 4f, row.height), label, style);
                EditorGUI.DrawRect(track, TrackBg);
            }

            return track;
        }

        static void DrawBand(Rect track, float from, float to, Color color)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            var x0 = ToX(track, from);
            var x1 = ToX(track, to);
            EditorGUI.DrawRect(new Rect(x0, track.y, Mathf.Max(1f, x1 - x0), track.height), color);
        }

        // ------------------------------------------------------------ handle

        static float DragHandle(Rect track, float value, string hint)
        {
            var id = GUIUtility.GetControlID(hint.GetHashCode(), FocusType.Passive);
            var x = ToX(track, value);
            var hit = new Rect(x - 7f, track.y - 2f, 14f, track.height + 4f);

            EditorGUIUtility.AddCursorRect(hit, MouseCursor.ResizeHorizontal);

            var e = Event.current;
            switch (e.GetTypeForControl(id))
            {
                case EventType.MouseDown:
                    if (e.button == 0 && hit.Contains(e.mousePosition))
                    {
                        GUIUtility.hotControl = id;
                        e.Use();
                    }
                    break;

                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == id)
                    {
                        value = Snap(ToNorm(track, e.mousePosition.x));
                        GUI.changed = true;
                        e.Use();
                    }
                    break;

                case EventType.MouseUp:
                    if (GUIUtility.hotControl == id)
                    {
                        GUIUtility.hotControl = 0;
                        e.Use();
                    }
                    break;

                case EventType.Repaint:
                    var hot = GUIUtility.hotControl == id;
                    EditorGUI.DrawRect(new Rect(x - 1.5f, track.y - 2f, 3f, track.height + 4f),
                        new Color(0.92f, 0.92f, 0.92f, hot ? 1f : 0.7f));
                    // 잡을 수 있다는 걸 알리는 위아래 그립
                    EditorGUI.DrawRect(new Rect(x - 3.5f, track.y - 3f, 7f, 3f), new Color(0.92f, 0.92f, 0.92f, hot ? 1f : 0.8f));
                    EditorGUI.DrawRect(new Rect(x - 3.5f, track.yMax, 7f, 3f), new Color(0.92f, 0.92f, 0.92f, hot ? 1f : 0.8f));
                    break;
            }

            return value;
        }

        // ---------------------------------------------------------- playhead

        /// <summary>레인 묶음 위에 재생 헤드를 그리고 드래그를 받는다.</summary>
        public static float PlayheadOverlay(Rect lanesArea, float value)
        {
            var track = TrackOf(lanesArea);
            var id = GUIUtility.GetControlID("timeline-playhead".GetHashCode(), FocusType.Passive);

            EditorGUIUtility.AddCursorRect(track, MouseCursor.ResizeHorizontal);

            var e = Event.current;
            switch (e.GetTypeForControl(id))
            {
                case EventType.MouseDown:
                    // 트랙 아무 곳이나 누르면 재생 헤드가 그 지점으로 온다.
                    // 구간 핸들이 먼저 이벤트를 가져가므로 핸들 조작과 충돌하지 않는다.
                    if (e.button == 0 && track.Contains(e.mousePosition))
                    {
                        GUIUtility.hotControl = id;
                        value = Snap(ToNorm(track, e.mousePosition.x));
                        GUI.changed = true;
                        e.Use();
                    }
                    break;

                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == id)
                    {
                        value = Snap(ToNorm(track, e.mousePosition.x));
                        GUI.changed = true;
                        e.Use();
                    }
                    break;

                case EventType.MouseUp:
                    if (GUIUtility.hotControl == id)
                    {
                        GUIUtility.hotControl = 0;
                        e.Use();
                    }
                    break;

                case EventType.Repaint:
                    EditorGUI.DrawRect(new Rect(ToX(track, value) - 1f, lanesArea.y - 2f, 2f, lanesArea.height + 4f), PlayheadColor);
                    break;
            }

            return value;
        }

        // ------------------------------------------------------------- chips

        public static void ActiveChip(ref Rect cursor, string label, Color color, bool on)
        {
            var w = EditorStyles.miniLabel.CalcSize(new GUIContent(label)).x + 14f;
            var r = new Rect(cursor.x, cursor.y, w, cursor.height);

            if (Event.current.type == EventType.Repaint)
            {
                EditorGUI.DrawRect(r, on ? color : new Color(color.r, color.g, color.b, 0.18f));

                var style = new GUIStyle(EditorStyles.miniLabel);
                style.alignment = TextAnchor.MiddleCenter;
                style.normal.textColor = on ? Color.white : new Color(0.55f, 0.55f, 0.55f, 1f);
                GUI.Label(r, label, style);
            }

            cursor.x += w + 4f;
        }
    }
}
