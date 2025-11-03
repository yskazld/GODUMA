// using System.Collections.Generic;
// using UnityEngine;

// [System.Serializable]
// public class EtoIconData
// {
//     public float x;
//     public float y;
//     public int level;
// }

// [System.Serializable]
// public class SaveData
// {
//     public int score;
//     public int currentLevel;
//     public List<EtoIconData> icons = new List<EtoIconData>();
// }

// public static class SaveDataManager
// {
//     private const string SaveKey = "GameSaveData";
//     private static string SavePath => Path.Combine(Application.persistentDataPath, "GameSaveData.json");

//     public static void SaveGame(int score, int level, List<EtoIconData> icons)
//     {
//         SaveData data = new SaveData();
//         data.score = score;
//         data.currentLevel = level;
//         data.icons = icons;

//         string json = JsonUtility.ToJson(data);
//         PlayerPrefs.SetString(SaveKey, json);
//         PlayerPrefs.SetInt("HasSaveData", 1); // フラグ
//         PlayerPrefs.Save();

//         Debug.Log("💾 ゲーム状態を保存しました:\n" + json);
//     }

//     public static SaveData LoadGame()
//     {
//         if (!PlayerPrefs.HasKey(SaveKey))
//         {
//             Debug.LogWarning("セーブデータが存在しません。");
//             return null;
//         }

//         string json = PlayerPrefs.GetString(SaveKey);
//         SaveData data = JsonUtility.FromJson<SaveData>(json);
//         return data;
//     }

//     public static bool HasSave()
//     {
//         return PlayerPrefs.GetInt("HasSaveData", 0) == 1;
//     }

//     public static void ClearSave()
//     {
//         PlayerPrefs.DeleteKey(SaveKey);
//         PlayerPrefs.DeleteKey("HasSaveData");
//         PlayerPrefs.Save();
//     }
// }
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class EtoIconData
{
    public float x;
    public float y;
    public int level;
}

[System.Serializable]
public class SaveData
{
    public int score;
    public int currentLevel;
    public List<EtoIconData> icons = new List<EtoIconData>();
}

public static class SaveDataManager
{
    private const string SaveKey = "GameSaveData";
    private static string SavePath => Path.Combine(Application.persistentDataPath, "GameSaveData.json");

    public static void SaveGame(int score, int level, List<EtoIconData> icons)
    {
        SaveData data = new SaveData
        {
            score = score,
            currentLevel = level,
            icons = icons
        };

        string json = JsonUtility.ToJson(data);

#if UNITY_ANDROID && !UNITY_EDITOR
        try
    {
        File.WriteAllText(SavePath, json);
        Debug.Log($"💾 [Android] JSONファイルに保存しました（{json.Length}文字）");
        Debug.Log($"📂 保存先パス: {SavePath}");
    }
    catch (System.Exception e)
    {
        Debug.LogError($"❌ [Android] File.WriteAllText() で例外発生: {e.Message}");
    }
#else
        // それ以外 → PlayerPrefs
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.SetInt("HasSaveData", 1); // フラグ
        PlayerPrefs.Save();
        Debug.Log("💾 [PlayerPrefs] に保存しました:\n" + json);
#endif
    }

    public static SaveData LoadGame()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (!File.Exists(SavePath))
        {
            Debug.LogWarning("📂 [Android] セーブファイルが存在しません");
            return null;
        }

            try
    {
        string json = File.ReadAllText(SavePath);
        Debug.Log("📥 [Android] JSONファイルから読み込み:\n" + json);
        return JsonUtility.FromJson<SaveData>(json);
    }
    catch (System.Exception e)
    {
        Debug.LogError($"❌ [Android] Load時に例外発生: {e.Message}");
        return null;
    }
#else
        if (!PlayerPrefs.HasKey(SaveKey))
        {
            Debug.LogWarning("📥 [PlayerPrefs] セーブデータが存在しません");
            return null;
        }

        string json = PlayerPrefs.GetString(SaveKey);
        Debug.Log("📥 [PlayerPrefs] 読み込み:\n" + json);
        return JsonUtility.FromJson<SaveData>(json);
#endif
    }

    public static bool HasSave()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        return File.Exists(SavePath);
#else
        return PlayerPrefs.GetInt("HasSaveData", 0) == 1;
#endif
    }

    public static void ClearSave()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
            Debug.Log("🗑️ [Android] セーブファイルを削除しました");
        }
#else
        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.DeleteKey("HasSaveData");
        PlayerPrefs.Save();
        Debug.Log("🗑️ [PlayerPrefs] セーブデータを削除しました");
#endif
    }
}
