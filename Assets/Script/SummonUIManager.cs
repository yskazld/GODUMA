using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SummonUIManager : MonoBehaviour
{
    [System.Serializable]
    public class SummonableIcon
    {
        public Sprite iconSprite;              // 表示用スプライト（UI向け）
        public GameObject summonPrefab;        // 実際に召喚されるプレハブ
        public int unlockLevel;                // 解放されるレベル（オプション）
    }

    public GameObject summonPanel;
    public Transform iconButtonParent;
    public GameObject iconButtonPrefab;
    public List<SummonableIcon> summonableIcons = new List<SummonableIcon>();

    // public void OpenSummonPanel()
    // {
    //     if (ItemManager.Instance.UseSummonItem())
    //     {
    //         summonPanel.SetActive(true);
    //         SetupButtons();
    //     }
    //     else
    //     {
    //         Debug.Log("📛 召喚アイテムがありません！");
    //     }
    // }

    public void OpenSummonPanel()
    {
        summonPanel.SetActive(true);
        SetupButtons();
    }

    void SetupButtons()
    {
        // 既存ボタン削除
        foreach (Transform child in iconButtonParent)
        {
            Destroy(child.gameObject);
        }

        int currentMaxLevel = EtoIconController.GetMaxEtoLevelInScene();

        foreach (var icon in summonableIcons)
        {
            if (icon.unlockLevel > currentMaxLevel)
            {
                Debug.Log($"❌ 除外: {icon.iconSprite?.name}（UnlockLv{icon.unlockLevel} > MaxEvoLv{currentMaxLevel}）");
                continue;
            }

            Debug.Log($"✅ 表示: {icon.iconSprite?.name} (Lv{icon.unlockLevel})");

            var button = Instantiate(iconButtonPrefab, iconButtonParent);
            Image iconImage = button.transform.Find("Image").GetComponent<Image>();
            iconImage.sprite = icon.iconSprite;

            int selectedIndex = summonableIcons.IndexOf(icon); // インデックス取得

            button.GetComponent<Button>().onClick.AddListener(() =>
            {
                Debug.Log($"🎯 Summonアイコン {icon.iconSprite?.name} が選択されました");

                // ItemManager に委譲（アイテム消費・ドロップ抑制処理）
                ItemManager.Instance?.SetNextIconFromSummon(selectedIndex);

                summonPanel.SetActive(false); // パネルは閉じる
            });
        }
    }



}

