using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class ItemShopManager : MonoBehaviour
{
    [Header("参照")]
    public LoginBonusManager coinManager; // コイン管理クラス
    public TMP_Text confirmText;         // メッセージ表示用（任意）
    public GameObject purchaseItemPanel;  // パネル表示用UI
    public GameObject itemButtonPrefab;         // ← ボタンのプレハブ
    public Transform itemListContainer;         // ← ボタンを並べる親オブジェクト


    [Header("アイテムアイコン画像")]
    public Sprite deleteIcon;
    public Sprite summonIcon;
    public Sprite topLineIcon;

    [Header("アイテム価格")]
    public int deleteItemCost = 100;
    public int summonItemCost = 300;
    public int topLineItemCost = 200;

    [Header("確認UI")]
    public GameObject confirmPanel;
    public TMP_Text questionText;

    private List<GameObject> currentButtons = new();
    private string selectedItemType;
    private int selectedItemCost;

    [Header("アイテム説明UI")]
    public GameObject itemInfoPanel;
    public Image itemIconImage;
    public TMP_Text itemDescriptionText;

    [Header("購入メッセージ表示用")]
    public GameObject purchaseMessagePanel;
    public TMP_Text purchaseMessageText;




    /// <summary>
    /// 「アイテム購入」ボタンから呼ばれる：購入パネルを開く
    /// </summary>
    public void OpenPurchasePanel()
    {
        if (purchaseItemPanel != null)
        {
            purchaseItemPanel.SetActive(true);
            PopulateItemList();
        }
    }

    /// <summary>
    /// パネル内の✕ボタンから呼ばれる：購入パネルを閉じる
    /// </summary>
    public void ClosePurchasePanel()
    {
        if (purchaseItemPanel != null)
        {
            purchaseItemPanel.SetActive(false);
        }
        // ボタンをすべて削除
        foreach (var btn in currentButtons)
        {
            Destroy(btn);
        }
        currentButtons.Clear();
    }
    private void PopulateItemList()
    {
        AddItemToList(deleteIcon, deleteItemCost, "delete");
        AddItemToList(summonIcon, summonItemCost, "summon");
        AddItemToList(topLineIcon, topLineItemCost, "topline");
    }

    private void AddItemToList(Sprite icon, int cost, string itemType)
    {
        GameObject buttonObj = Instantiate(itemButtonPrefab, itemListContainer);

        Image iconImage = buttonObj.transform.Find("Vertical/IconImage")?.GetComponent<Image>();
        if (iconImage != null)
            iconImage.sprite = icon;

        TMP_Text coinText = buttonObj.transform.Find("Vertical/CoinText")?.GetComponent<TMP_Text>();
        if (coinText != null)
            coinText.text = $"{cost} コイン";

        Button btn = buttonObj.GetComponent<Button>();
        btn.onClick.AddListener(() =>
        {
            selectedItemType = itemType;
            selectedItemCost = cost;
            ShowConfirmDialog($"本当に購入しますか？");
        });

        currentButtons.Add(buttonObj);
    }

    public void ShowConfirmDialog(string message)
    {
        questionText.text = message;
        confirmPanel.SetActive(true);
    }

    public void OnClickConfirmYes()
    {
        if (coinManager == null || confirmText == null)
        {
            Debug.LogError("coinManager または feedbackText が未設定です！");
            return;
        }

        if (coinManager.SpendCoins(selectedItemCost))
        {


            string purchasedItemName = "";

            switch (selectedItemType)
            {
                case "delete":
                    // データ保持用クラスに加算（GameSceneへの引き継ぎ用）
                    ItemDataHolder.DeleteItemCount += 1;

                    // 保存された合計に加算してPlayerPrefsに保存
                    int currentDelete = PlayerPrefs.GetInt("DeleteItemCount", 0);
                    currentDelete += 1;
                    PlayerPrefs.SetInt("DeleteItemCount", currentDelete);
                    PlayerPrefs.Save();

                    // ゲーム中UIとロジックに反映（もしInstanceがあるなら）
                    ItemManager.Instance?.AddDeleteItem(1);
                    purchasedItemName = "削除アイテム";
                    break;

                case "summon":
                    ItemDataHolder.SummonItemCount += 1;

                    int currentSummon = PlayerPrefs.GetInt("SummonItemCount", 0);
                    currentSummon += 1;
                    PlayerPrefs.SetInt("SummonItemCount", currentSummon);
                    PlayerPrefs.Save();

                    ItemManager.Instance?.AddSummonItem(1);
                    purchasedItemName = "召喚アイテム";
                    break;

                case "topline":
                    ItemDataHolder.TopLineItemCount += 1;

                    int currentTopLine = PlayerPrefs.GetInt("TopLineItemCount", 0);
                    currentTopLine += 1;
                    PlayerPrefs.SetInt("TopLineItemCount", currentTopLine);
                    PlayerPrefs.Save();

                    ItemManager.Instance?.AddTopLineItem(1);
                    purchasedItemName = "TopLineアイテム";
                    break;
            }



            ShowPurchaseMessage($"{purchasedItemName}を購入しました！");


            if (confirmText != null)
            {
                confirmText.text = "購入しました！";
                StartCoroutine(HideFeedbackAfterDelay(1f)); // ← これを確実に呼ぶ
                Debug.Log("✅ confirmText に正常にセットされました");
            }
            else
            {
                Debug.LogError("❌ confirmText が null です！Inspectorで設定されていますか？");
            }
            coinManager.UpdateCoinUI(); // ← LoginBonusManager側にこの関数が必要
            confirmPanel.SetActive(false);
        }
        else
        {
            confirmText.text = "コインが足りません！";

            // 古いコルーチンが動いてたら止めて、新しく開始
            if (feedbackCoroutine != null)
                StopCoroutine(feedbackCoroutine);

            feedbackCoroutine = StartCoroutine(HideFeedbackAfterDelay(1f)); // 1秒後に非表示
        }
    }

    public void OnClickConfirmNo()
    {
        confirmPanel.SetActive(false);
    }

    private Coroutine feedbackCoroutine;

    private IEnumerator HideFeedbackAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (confirmText != null)
            confirmText.text = "";
    }

    private Dictionary<string, string> itemDescriptions = new Dictionary<string, string>
    {
        { "delete", "不要な干支アイコンを1つ削除できます。" },
        { "summon", "指定した干支アイコンを出現させます。" },
        { "topline", "GameOverラインが一時的に上昇します。" }
    };

    public void ShowItemInfo()
    {
        if (itemInfoPanel != null)
        {
            itemInfoPanel.SetActive(true);
        }
    }
    public void CloseItemInfoPanel()
    {
        itemInfoPanel.SetActive(false);
    }

    private Coroutine purchaseMessageCoroutine;

    public void ShowPurchaseMessage(string message)
    {
        if (purchaseMessagePanel != null && purchaseMessageText != null)
        {
            purchaseMessageText.text = message;
            purchaseMessagePanel.SetActive(true);

            if (purchaseMessageCoroutine != null)
                StopCoroutine(purchaseMessageCoroutine);

            purchaseMessageCoroutine = StartCoroutine(HidePurchaseMessageAfterDelay(2f));
        }
        else
        {
            Debug.LogError("❌ purchaseMessagePanel か purchaseMessageText が設定されていません！");
        }
    }


    private IEnumerator HidePurchaseMessageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        purchaseMessagePanel.SetActive(false);
    }
}

