using TMPro;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;


public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance;
    [Header("削除アイテム設定")]
    public int deleteItemCount; // 所持アイテム数
    public int maxItemCount = 99;
    private bool deleteMode = false;
    public TMP_Text deleteItemCountText;  // ← Textをアタッチする

    [Header("召喚アイテム設定")]
    public int summonItemCount;
    public TMP_Text summonItemCountText;  // ← Textをアタッチする

    [Header("当たり判定延長アイテム設定")]
    public float topLineRaiseRatio = 0.3f;
    public float topLineDuration = 10f;

    public int topLineItemCount = 1;
    public TMP_Text topLineItemCountText;

    private Coroutine topLineCoroutine;

    // === ここを追加 ===
    [Header("アイテム使用確認パネル")]
    public GameObject confirmPanel;
    public TMP_Text confirmMessageText;
    public Button confirmYesButton;
    public Button confirmNoButton;

    private System.Action onConfirm; // 実行対象の処理

    [Header("召喚UIの呼び出し先")]
    public SummonUIManager summonUIManager;  // ← Inspectorでアサイン

    [Header("注意メッセージ表示用")]
    public GameObject warningMessagePanel;
    public TMP_Text warningMessageText;
    public GameObject messagePanel;
    public TMP_Text messageText; // ← 削除用メッセージなどに使用

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("🟢 ItemManager.Instance が初期化されました");
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 🎯 プレイヤーデータを確実に反映
        deleteItemCount = Mathf.Max(PlayerPrefs.GetInt("DeleteItemCount", 1), 0);
        summonItemCount = Mathf.Max(PlayerPrefs.GetInt("SummonItemCount", 1), 0);
        topLineItemCount = Mathf.Max(PlayerPrefs.GetInt("TopLineItemCount", 1), 0);

        Debug.Log($"[初期化後] Delete={deleteItemCount}, Summon={summonItemCount}, TopLine={topLineItemCount}");

        UpdateItemCountUI();
    }




    /// <summary>
    /// 削除ボタンを押したときに呼び出す（UIのOnClickに設定）
    /// </summary>
    public void OnClickDeleteButton()
    {
        Debug.Log($"[OnClickDeleteButton] deleteItemCount: {deleteItemCount}, deleteMode: {deleteMode}");

        if (deleteMode)
        {
            ShowWarningMessage("すでに削除モード中です");
            return;
        }

        ShowConfirm("削除アイテムを使ってよろしいですか？", () =>
        {
            Debug.Log($"[ConfirmYes] deleteItemCount: {deleteItemCount}");
            if (deleteItemCount <= 0)
            {
                ShowWarningMessage("削除アイテムが0のため使用できません");
                return;
            }

            deleteMode = true;
            ShowMessage("削除したいアイコンをクリックしてね");
            Debug.Log("削除モードに入りました。アイコンを1つ選んでください。");
        });
    }

    private Coroutine messageCoroutine;

    private void ShowMessage(string msg)
    {
        Debug.Log($"[ShowMessage] {msg}");

        if (messageText != null)
        {
            messageText.text = msg;
            messagePanel?.SetActive(true);

            // すでにコルーチンが走っていれば停止
            if (messageCoroutine != null)
                StopCoroutine(messageCoroutine);

            messageCoroutine = StartCoroutine(HideMessageAfterDelay(2f)); // ← 2秒で非表示
        }
    }
    private IEnumerator HideMessageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        messagePanel?.SetActive(false);
    }


    public void OnClickSummonButton()
    {
        ShowConfirm("召喚アイテムを使ってよろしいですか？", () =>
        {
            if (summonItemCount <= 0)
            {
                ShowWarningMessage("アイテムの個数が0のため使用できません");
                return;
            }

            summonUIManager?.OpenSummonPanel();
        });
    }
    public void OnClickTopLineButton()
    {
        ShowConfirm("TopLineを一時的に上げますか？", () =>
        {
            if (topLineItemCount <= 0)
            {
                ShowWarningMessage("TopLineUPアイテムが0のため使用できません");
                return;
            }

            UseTopLineItem();
        });
    }





    /// <summary>
    /// 干支アイコンから呼ばれる：削除実行
    /// </summary>
    public void TryDeleteIcon(EtoIconController icon)
    {
        if (deleteMode && deleteItemCount > 0)
        {
            deleteMode = false; // モード終了
            deleteItemCount--;
            PlayerPrefs.SetInt("DeleteItemCount", deleteItemCount);
            UpdateItemCountUI(); // UI更新
            Destroy(icon.gameObject);
            Debug.Log("✅ アイコンを削除しました");
        }
    }

    // ItemManager.cs に以下のように追記（または修正）
    public void AddDeleteItem(int amount)
    {
        deleteItemCount += amount;
        if (deleteItemCount > maxItemCount)
            deleteItemCount = maxItemCount;

        // 保存処理
        PlayerPrefs.SetInt("DeleteItemCount", deleteItemCount);
        PlayerPrefs.Save();

        Debug.Log($"🛒 AddDeleteItemが呼ばれました！ 新しい数: {deleteItemCount}");
        UpdateItemCountUI();
    }

    public int GetDeleteItemCount()
    {
        return deleteItemCount;
    }

    public bool IsInDeleteMode()
    {
        return deleteMode;
    }
    // === 召喚アイテム関連 ===

    public void AddSummonItem(int amount = 1)
    {
        summonItemCount = Mathf.Min(summonItemCount + amount, maxItemCount);
        PlayerPrefs.SetInt("SummonItemCount", summonItemCount);
        UpdateItemCountUI();
    }

    public bool UseSummonItem()
    {
        if (summonItemCount > 0)
        {
            summonItemCount--;
            PlayerPrefs.SetInt("SummonItemCount", summonItemCount);
            UpdateItemCountUI();
            return true;
        }
        return false;
    }

    public int GetSummonItemCount() => summonItemCount;

    // === UI更新 ===

    public void UpdateItemCountUI()
    {
        Debug.Log($"🧮 UI更新: Delete={deleteItemCount}, Summon={summonItemCount}, TopLine={topLineItemCount}");
        if (deleteItemCountText != null)
            deleteItemCountText.text = $"×{deleteItemCount}";

        if (summonItemCountText != null)
            summonItemCountText.text = $"×{summonItemCount}";
        if (topLineItemCountText != null) // ★ これが抜けている！
            topLineItemCountText.text = $"×{topLineItemCount}";
    }


    public void UseTopLineItem()
    {
        if (topLineItemCount <= 0) return;

        GameObject topLine = GameObject.FindWithTag("TopLine");
        GameObject left = GameObject.FindWithTag("LeftLine");

        if (topLine == null || left == null) return;

        topLineItemCount--;
        PlayerPrefs.SetInt("TopLineItemCount", topLineItemCount);
        UpdateTopLineItemCountUI();

        if (topLineCoroutine != null)
        {
            StopCoroutine(topLineCoroutine);
        }

        topLineCoroutine = StartCoroutine(RaiseTopLineTemporarily(topLine, topLineRaiseRatio));
    }


    IEnumerator RaiseTopLineTemporarily(GameObject topLine, float ratio)
    {
        GameObject bottom = GameObject.FindWithTag("BottomLine");
        if (bottom == null)
        {
            Debug.LogError("BottomLine が見つかりません！");
            yield break;
        }

        Vector3 basePos = topLine.transform.position; // 元のTopLine位置
        Vector3 bottomPos = bottom.transform.position;

        float offset = (basePos.y - bottomPos.y) * ratio; // 差分に比率をかける
        float raisedY = basePos.y + offset;

        float elapsed = 0f;
        while (elapsed < topLineDuration)
        {
            topLine.transform.position = new Vector3(basePos.x, raisedY, basePos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }

        topLine.transform.position = basePos; // 元に戻す
    }

    void UpdateTopLineItemCountUI()
    {
        if (topLineItemCountText != null)
            topLineItemCountText.text = $"×{topLineItemCount}";
    }

    public void AddTopLineItem(int amount = 1)
    {
        topLineItemCount = Mathf.Min(topLineItemCount + amount, maxItemCount);
        PlayerPrefs.SetInt("TopLineItemCount", topLineItemCount);
        UpdateTopLineItemCountUI();
    }

    private void ShowConfirm(string message, System.Action onYes)
    {
        onConfirm = onYes;

        if (confirmPanel != null) confirmPanel.SetActive(true);
        if (confirmMessageText != null) confirmMessageText.text = message;

        confirmYesButton.onClick.RemoveAllListeners();
        confirmNoButton.onClick.RemoveAllListeners();

        confirmYesButton.onClick.AddListener(() =>
        {
            confirmPanel.SetActive(false);
            onConfirm?.Invoke();
        });

        confirmNoButton.onClick.AddListener(() =>
        {
            confirmPanel.SetActive(false);
        });
    }

    private void ShowWarningMessage(string msg)
    {
        Debug.Log($"[ShowWarningMessage] 表示メッセージ: {msg}");

        if (warningMessagePanel != null && warningMessageText != null)
        {
            warningMessageText.text = msg;
            warningMessagePanel.SetActive(true);
            StartCoroutine(HideWarningMessageAfterDelay(2f));
        }
    }

    IEnumerator HideWarningMessageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        warningMessagePanel.SetActive(false);
    }

    public void SetNextIconFromSummon(int index)
    {
        Debug.Log($"🧲 召喚アイコン選択: index={index}");

        if (summonItemCount <= 0)
        {
            ShowWarningMessage("召喚アイテムが足りません");
            return;
        }

        UseSummonItem();

        if (EtoDropRadar.Instance != null)
        {
            EtoDropRadar.Instance.SetNextEtoIndex(index);
            EtoDropRadar.Instance.summonCooldownTime = Time.time + 1f; // 1秒ドロップ無効
        }
        else
        {
            Debug.LogWarning("⚠ EtoDropRadar.Instance が見つかりません");
        }

        ShowMessage("次のアイコンに設定されました");
        //summonUIManager?.CloseSummonPanel();
    }



}
