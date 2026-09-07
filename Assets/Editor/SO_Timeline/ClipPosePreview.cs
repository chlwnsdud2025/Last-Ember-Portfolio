using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SOTimeline.EditorTools
{
    /// <summary>
    /// 리그된 프리팹을 프리뷰 씬에 띄우고, AnimationClip 의 임의 진행도(0~1) 포즈를 찍어 렌더한다.
    /// 바닥과 그리드는 프리뷰 씬 안의 실제 3D 오브젝트라 카메라를 돌리면 같이 돈다.
    /// AnimationMode 는 샘플링~렌더 구간에서만 켜고 바로 끈다.
    /// </summary>
    internal sealed class ClipPosePreview : IDisposable
    {
        static readonly Color k_Sky = new Color(0.145f, 0.152f, 0.168f, 1f);
        static readonly Color k_Ground = new Color(0.105f, 0.110f, 0.122f, 1f);
        static readonly Color k_Grid = new Color(0.34f, 0.36f, 0.40f, 1f);

        PreviewRenderUtility _pru;
        GameObject _instance;
        GameObject _source;
        Bounds _bounds;

        // 루트 모션이 트랜스폼에 실리는지 본에 구워지는지는 클립·아바타마다 다르다.
        // 그래서 위치를 되돌리는 대신, 기준점(엉덩이 본)의 실제 월드 좌표를 재서 그만큼 상쇄한다.
        Animator _animator;
        AnimationClip _refClip;
        Vector3 _refPos;

        GameObject _stage;
        Mesh _gridMesh;
        Mesh _groundMesh;
        Material _gridMat;
        Material _groundMat;

        Vector2 _orbit = new Vector2(150f, 10f);
        float _zoom = 1f;

        public bool HasRig { get { return _instance != null; } }

        /// <summary>Animator 를 못 찾으면 루트 모션을 되돌릴 기준 트랜스폼이 없다.</summary>
        public bool CanStripRootMotion { get { return _animator != null; } }

        // ------------------------------------------------------------ public

        public void Draw(Rect rect, AnimationClip clip, float normalizedTime, GameObject rigPrefab, bool stripRootMotion)
        {
            if (rigPrefab == null)
            {
                DrawNotice(rect, "미리보기 리그 프리팹이 지정되지 않았습니다");
                return;
            }

            if (clip == null)
            {
                DrawNotice(rect, "클립 없음");
                return;
            }

            EnsureUtility();
            EnsureInstance(rigPrefab);

            if (_instance == null)
            {
                DrawNotice(rect, "프리팹을 생성할 수 없습니다");
                return;
            }

            HandleOrbit(rect);

            if (Event.current.type != EventType.Repaint)
                return;

            var tex = RenderPosed(rect, clip, normalizedTime, stripRootMotion);
            if (tex != null)
                GUI.DrawTexture(rect, tex, ScaleMode.StretchToFill, false);
        }

        public void Dispose()
        {
            DestroyInstance();
            DestroyStage();

            if (_pru != null)
            {
                _pru.Cleanup();
                _pru = null;
            }
        }

        // ----------------------------------------------------------- internal

        void EnsureUtility()
        {
            if (_pru != null)
                return;

            _pru = new PreviewRenderUtility();
            _pru.camera.fieldOfView = 28f;
            _pru.camera.nearClipPlane = 0.01f;
            _pru.camera.farClipPlane = 500f;
            _pru.camera.clearFlags = CameraClearFlags.SolidColor;
            _pru.camera.backgroundColor = k_Sky;

            _pru.lights[0].intensity = 1.35f;
            _pru.lights[0].transform.rotation = Quaternion.Euler(38f, 130f, 0f);
            _pru.lights[1].intensity = 0.65f;
            _pru.lights[1].transform.rotation = Quaternion.Euler(-15f, -70f, 0f);
        }

        void EnsureInstance(GameObject prefab)
        {
            if (_instance != null && _source == prefab)
                return;

            DestroyInstance();
            DestroyStage();

            _source = prefab;
            _instance = UnityEngine.Object.Instantiate(prefab);
            _instance.hideFlags = HideFlags.HideAndDontSave;
            _instance.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

            // Animator 는 루트가 아니라 자식(모델 오브젝트)에 있을 수 있다.
            _animator = _instance.GetComponentInChildren<Animator>(true);
            _refClip = null;

            _pru.AddSingleGO(_instance);
            _bounds = CalcBounds(_instance);

            BuildStage();
        }

        void DestroyInstance()
        {
            if (_instance != null)
                UnityEngine.Object.DestroyImmediate(_instance);

            _instance = null;
            _source = null;
            _animator = null;
            _refClip = null;
        }

        /// <summary>
        /// 이동량을 재는 기준점. 휴머노이드면 엉덩이 본을 쓴다.
        /// 루트 모션이 트랜스폼에 실리든 본에 구워지든 결과 좌표는 여기에 나타난다.
        /// </summary>
        Vector3 MeasureAnchor()
        {
            if (_animator != null && _animator.isHuman)
            {
                var hips = _animator.GetBoneTransform(HumanBodyBones.Hips);
                if (hips != null)
                    return hips.position;
            }

            return _animator != null ? _animator.transform.position : _instance.transform.position;
        }

        // -------------------------------------------------------------- stage

        /// <summary>캐릭터 크기에 맞춘 바닥판과 격자를 프리뷰 씬에 세운다.</summary>
        void BuildStage()
        {
            var reach = Mathf.Max(0.5f, _bounds.size.magnitude);
            var extent = reach * 4f;      // 루트 모션으로 이동해도 바닥을 벗어나지 않도록 넉넉히
            var cell = reach * 0.25f;

            _stage = new GameObject("SOTimeline_Stage");
            _stage.hideFlags = HideFlags.HideAndDontSave;

            // 바닥판. CreatePrimitive 는 Collider 가 딸려 와 물리 모듈에 의존하므로 직접 만든다.
            _groundMesh = BuildQuadMesh(extent);
            _groundMat = MakeMaterial(k_Ground);

            var ground = new GameObject("Ground", typeof(MeshFilter), typeof(MeshRenderer));
            ground.hideFlags = HideFlags.HideAndDontSave;
            ground.transform.SetParent(_stage.transform, false);
            ground.transform.localPosition = new Vector3(0f, -0.004f, 0f);
            ground.GetComponent<MeshFilter>().sharedMesh = _groundMesh;

            var gr = ground.GetComponent<MeshRenderer>();
            gr.sharedMaterial = _groundMat;
            gr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            // 격자선
            var divisions = Mathf.Clamp(Mathf.RoundToInt(extent * 2f / Mathf.Max(0.01f, cell)), 8, 80);
            _gridMesh = BuildGridMesh(extent, divisions);
            _gridMat = MakeMaterial(k_Grid);

            var grid = new GameObject("Grid", typeof(MeshFilter), typeof(MeshRenderer));
            grid.hideFlags = HideFlags.HideAndDontSave;
            grid.transform.SetParent(_stage.transform, false);
            grid.GetComponent<MeshFilter>().sharedMesh = _gridMesh;

            var mr = grid.GetComponent<MeshRenderer>();
            mr.sharedMaterial = _gridMat;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;

            _pru.AddSingleGO(_stage);
        }

        void DestroyStage()
        {
            if (_stage != null)
                UnityEngine.Object.DestroyImmediate(_stage);
            _stage = null;

            if (_gridMesh != null)
                UnityEngine.Object.DestroyImmediate(_gridMesh);
            _gridMesh = null;

            if (_groundMesh != null)
                UnityEngine.Object.DestroyImmediate(_groundMesh);
            _groundMesh = null;

            if (_gridMat != null)
                UnityEngine.Object.DestroyImmediate(_gridMat);
            _gridMat = null;

            if (_groundMat != null)
                UnityEngine.Object.DestroyImmediate(_groundMat);
            _groundMat = null;
        }

        static Mesh BuildQuadMesh(float extent)
        {
            var mesh = new Mesh();
            mesh.hideFlags = HideFlags.HideAndDontSave;
            mesh.SetVertices(new List<Vector3>
            {
                new Vector3(-extent, 0f, -extent),
                new Vector3(-extent, 0f,  extent),
                new Vector3( extent, 0f,  extent),
                new Vector3( extent, 0f, -extent)
            });
            mesh.SetNormals(new List<Vector3> { Vector3.up, Vector3.up, Vector3.up, Vector3.up });
            mesh.SetTriangles(new[] { 0, 1, 2, 0, 2, 3 }, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        static Mesh BuildGridMesh(float extent, int divisions)
        {
            var verts = new List<Vector3>();
            var indices = new List<int>();
            var step = extent * 2f / divisions;

            for (var i = 0; i <= divisions; i++)
            {
                var p = -extent + i * step;

                indices.Add(verts.Count); verts.Add(new Vector3(p, 0f, -extent));
                indices.Add(verts.Count); verts.Add(new Vector3(p, 0f, extent));

                indices.Add(verts.Count); verts.Add(new Vector3(-extent, 0f, p));
                indices.Add(verts.Count); verts.Add(new Vector3(extent, 0f, p));
            }

            var mesh = new Mesh();
            mesh.hideFlags = HideFlags.HideAndDontSave;
            mesh.indexFormat = verts.Count > 65000
                ? UnityEngine.Rendering.IndexFormat.UInt32
                : UnityEngine.Rendering.IndexFormat.UInt16;
            mesh.SetVertices(verts);
            mesh.SetIndices(indices.ToArray(), MeshTopology.Lines, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>URP / 빌트인 어느 쪽이든 렌더되는 단색 머티리얼을 만든다.</summary>
        static Material MakeMaterial(Color color)
        {
            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            if (shader == null) shader = Shader.Find("Hidden/Internal-Colored");

            var mat = new Material(shader);
            mat.hideFlags = HideFlags.HideAndDontSave;

            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);

            return mat;
        }

        // ------------------------------------------------------------- render

        Texture RenderPosed(Rect rect, AnimationClip clip, float normalizedTime, bool stripRootMotion)
        {
            var time = Mathf.Clamp01(normalizedTime) * clip.length;

            // 다른 도구(애니메이션 창 등)가 이미 AnimationMode 를 쓰고 있으면 소유권을 빼앗지 않는다.
            var owned = !AnimationMode.InAnimationMode();

            Texture result = null;

            try
            {
                if (owned)
                    AnimationMode.StartAnimationMode();

                // 상쇄 기준: 이 클립의 0 프레임에서 기준점이 어디였는지를 한 번만 재 둔다.
                if (stripRootMotion && _refClip != clip)
                {
                    _instance.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

                    AnimationMode.BeginSampling();
                    AnimationMode.SampleAnimationClip(_instance, clip, 0f);
                    AnimationMode.EndSampling();

                    _refPos = MeasureAnchor();
                    _refClip = clip;
                }

                if (stripRootMotion)
                    _instance.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

                AnimationMode.BeginSampling();
                AnimationMode.SampleAnimationClip(_instance, clip, time);
                AnimationMode.EndSampling();

                var anchor = MeasureAnchor();

                if (stripRootMotion)
                {
                    // 0 프레임 대비 수평 이동량만큼 인스턴스 전체를 반대로 민다.
                    // 트랜스폼이든 본이든 어디에 실렸는지와 무관하게 상쇄된다. 수직 움직임은 남긴다.
                    var drift = anchor - _refPos;
                    drift.y = 0f;
                    _instance.transform.position -= drift;
                    anchor -= drift;
                }

                // 포즈가 살아 있는 동안 렌더해야 한다. StopAnimationMode 는 값을 원래대로 되돌린다.
                PlaceCamera(stripRootMotion ? Vector3.zero : anchor - _bounds.center);

                _pru.BeginPreview(rect, GUIStyle.none);
                _pru.camera.Render();
                result = _pru.EndPreview();
            }
            finally
            {
                if (owned && AnimationMode.InAnimationMode())
                    AnimationMode.StopAnimationMode();
            }

            return result;
        }

        void PlaceCamera(Vector3 followOffset)
        {
            var radius = Mathf.Max(0.05f, _bounds.extents.magnitude);
            var fov = _pru.camera.fieldOfView * 0.5f * Mathf.Deg2Rad;
            var dist = radius / Mathf.Max(0.01f, Mathf.Sin(fov)) * _zoom;

            // 루트 모션을 남겨 둔 경우 캐릭터를 따라가서 화면 밖으로 나가지 않게 한다.
            var target = _bounds.center + followOffset;

            var rot = Quaternion.Euler(_orbit.y, _orbit.x, 0f);
            _pru.camera.transform.rotation = rot;
            _pru.camera.transform.position = target - rot * Vector3.forward * dist;
        }

        void HandleOrbit(Rect rect)
        {
            var id = GUIUtility.GetControlID(FocusType.Passive);
            var e = Event.current;

            switch (e.GetTypeForControl(id))
            {
                case EventType.MouseDown:
                    if (rect.Contains(e.mousePosition) && e.button == 0)
                    {
                        GUIUtility.hotControl = id;
                        e.Use();
                    }
                    break;

                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == id)
                    {
                        _orbit.x += e.delta.x * 0.6f;
                        _orbit.y = Mathf.Clamp(_orbit.y + e.delta.y * 0.4f, -20f, 70f);
                        e.Use();
                        GUI.changed = true;
                    }
                    break;

                case EventType.MouseUp:
                    if (GUIUtility.hotControl == id)
                    {
                        GUIUtility.hotControl = 0;
                        e.Use();
                    }
                    break;

                case EventType.ScrollWheel:
                    if (rect.Contains(e.mousePosition))
                    {
                        _zoom = Mathf.Clamp(_zoom + e.delta.y * 0.03f, 0.35f, 3f);
                        e.Use();
                        GUI.changed = true;
                    }
                    break;
            }

            if (rect.Contains(e.mousePosition))
                EditorGUIUtility.AddCursorRect(rect, MouseCursor.Orbit);
        }

        static Bounds CalcBounds(GameObject go)
        {
            var renderers = go.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
                return new Bounds(Vector3.up, Vector3.one * 2f);

            var b = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++)
                b.Encapsulate(renderers[i].bounds);

            // 공격 모션은 바인드 포즈보다 크게 벌어지므로 여유를 준다.
            b.Expand(b.size.magnitude * 0.35f);
            return b;
        }

        static void DrawNotice(Rect rect, string message)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            EditorGUI.DrawRect(rect, k_Sky);

            var style = new GUIStyle(EditorStyles.miniLabel);
            style.alignment = TextAnchor.MiddleCenter;
            style.wordWrap = true;
            style.normal.textColor = new Color(0.55f, 0.55f, 0.55f, 1f);
            GUI.Label(rect, message, style);
        }
    }
}
