using UnityEngine;
using UnityEditor;

public class EtoScaleEditor
{
    [MenuItem("Jobs/GodIconフォルダ内のPrefabのスケールを調整")]
    public static void AdjustPrefabScales()
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefab/GodIcon" });

        int updatedCount = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            EtoIconController icon = prefab.GetComponent<EtoIconController>();
            if (icon == null)
            {
                Debug.LogWarning($"⚠ {path} に EtoIconController が見つかりませんでした");
                continue;
            }

            int level = Mathf.Clamp(icon.etoLevel, 1, 36);
            float baseScale = 0.75f;
            float increment = 0.05f;
            float scaleValue = baseScale + (level - 1) * increment;

            // Prefabのスケールを変更（インスタンス化してApply）
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.transform.localScale = new Vector3(scaleValue, scaleValue, 1f);

            // Applyして削除
            PrefabUtility.SaveAsPrefabAsset(instance, path);
            GameObject.DestroyImmediate(instance);

            updatedCount++;
        }

        Debug.Log($"✅ GodIcon内のPrefabスケール更新完了：{updatedCount}個のPrefabを調整しました");
    }
}
