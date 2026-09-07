using UnityEditor;
using UnityEngine;

namespace SOTimeline.EditorTools
{
    /// <summary>
    /// 클립 포즈 미리보기에 사용할 리그 프리팹을 기억한다.
    /// EditorPrefs 에만 저장하므로 런타임 ScriptableObject 와 .asset 파일은 전혀 건드리지 않는다.
    /// </summary>
    internal static class PreviewRigSettings
    {
        const string k_Prefix = "SOTimeline.PreviewRigGuid.";
        const string k_DefaultQuery = "M_Knight t:Prefab";

        static string Key
        {
            get { return k_Prefix + Application.dataPath.GetHashCode(); }
        }

        public static GameObject Prefab
        {
            get
            {
                var guid = EditorPrefs.GetString(Key, string.Empty);

                if (string.IsNullOrEmpty(guid))
                {
                    guid = FindDefaultGuid();
                    if (!string.IsNullOrEmpty(guid))
                        EditorPrefs.SetString(Key, guid);
                }

                if (string.IsNullOrEmpty(guid))
                    return null;

                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(path))
                    return null;

                return AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }
            set
            {
                if (value == null)
                {
                    EditorPrefs.DeleteKey(Key);
                    return;
                }

                var path = AssetDatabase.GetAssetPath(value);
                if (string.IsNullOrEmpty(path))
                    return;

                EditorPrefs.SetString(Key, AssetDatabase.AssetPathToGUID(path));
            }
        }

        /// <summary>Player.prefab 안에서 쓰는 모델 프리팹을 기본값으로 찾는다.</summary>
        static string FindDefaultGuid()
        {
            var hits = AssetDatabase.FindAssets(k_DefaultQuery);
            return hits != null && hits.Length > 0 ? hits[0] : null;
        }
    }
}
