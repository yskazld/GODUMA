using UnityEditor;
using UnityEngine;

public class EtoPhysicsSetter : EditorWindow
{
    [MenuItem("Tools/Eto Physics Mass Setter (Prefab対応)")]
    public static void ShowWindow()
    {
        GetWindow<EtoPhysicsSetter>("Eto Physics Setter");
    }

    private void OnGUI()
    {
        GUILayout.Label("Assets/Prefab/GodIcon にあるPrefabを一括調整", EditorStyles.boldLabel);

        if (GUILayout.Button("Prefabに一括設定（Mass, Drag）"))
        {
            ApplyToPrefabs();
        }
    }

    private void ApplyToPrefabs()
    {
        string prefabFolderPath = "Assets/Prefab/GodIcon";
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { prefabFolderPath });

        int successCount = 0;
        int skipCount = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab != null)
            {
                var controller = prefab.GetComponent<EtoIconController>();
                var rb = prefab.GetComponent<Rigidbody2D>();

                if (controller != null && rb != null)
                {
                    int level = Mathf.Clamp(controller.etoLevel, 1, 36);
                    float mass = (36 - level) * 0.2f + 1.0f;

                    rb.mass = mass;
                    rb.linearDamping = 1.0f;
                    rb.angularDamping = 1.0f;

                    EditorUtility.SetDirty(prefab);
                    PrefabUtility.SavePrefabAsset(prefab);
                    successCount++;
                }
                else
                {
                    skipCount++;
                }
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"✅ Prefab設定完了: {successCount} 件、スキップ: {skipCount} 件");
    }
}
