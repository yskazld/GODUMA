using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ListEvolution : MonoBehaviour
{
    public GameObject evolutionPanel;               // パネル本体（ON/OFF用）
    public GameObject evoPanelParent;               // GridLayoutの親
    public GameObject evoIconItemPrefab;            // アイコンプレハブ
    public List<Sprite> evolutionIcons;             // 進化アイコン（36個）
    public Sprite questionMarkIcon;                 // 「？」用アイコン
    public List<string> iconNames;                  // 各アイコン名（36個）

    private int maxReachedLevel = 1;
    private bool isInitialized = false;
    private bool isVisible = false;

    void Start()
    {
        // PlayerPrefsから最大到達レベルを取得（デフォルト1）
        maxReachedLevel = PlayerPrefs.GetInt("MaxReachedLevel", 1);

        // 最初はパネルを非表示にしておく
        evolutionPanel.SetActive(false);
    }

    // public void EvoPanelShow()
    // {
    //     Debug.Log("▶ EvoPanelShow called");
    //     maxReachedLevel = EtoIconController.GetMaxEtoLevelInScene(); // ★常に更新！

    //     if (!isInitialized)
    //     {
    //         Debug.Log("🔄 Generating panel items...");
    //         GeneratePanelItems();
    //         isInitialized = true;
    //     }

    //     isVisible = !isVisible;
    //     evolutionPanel.SetActive(isVisible);  // 表示／非表示切り替え
    // }

    public void EvoPanelShow()
    {
        Debug.Log("▶ EvoPanelShow called");

        // 毎回再描画するように変更！
        maxReachedLevel = EtoIconController.GetMaxEtoLevelInScene();
        ClearOldItems();  // ★既存アイテムを削除
        GeneratePanelItems();

        isVisible = !isVisible;
        evolutionPanel.SetActive(isVisible);
    }

    void ClearOldItems()
    {
        foreach (Transform child in evoPanelParent.transform)
        {
            Destroy(child.gameObject);
        }
    }


    // ←★これを追加
    public void CloseEvoPanel()
    {
        Debug.Log("❎ CloseEvoPanel called");

        isVisible = false;
        evolutionPanel.SetActive(false);
    }


    void GeneratePanelItems()
    {
        //AdjustGridLayout(); // ← 最初に追加
        int maxReachedLevel = EtoIconController.GetMaxEtoLevelInScene(); // ← これに統一！

        for (int i = 0; i < evolutionIcons.Count; i++)
        {
            GameObject item = Instantiate(evoIconItemPrefab, evoPanelParent.transform);

            var iconImage = item.transform.Find("IconImage").GetComponent<Image>();
            var nameText = item.transform.Find("NameText").GetComponent<TextMeshProUGUI>();

            bool isReached = (i + 1) <= maxReachedLevel;

            iconImage.sprite = isReached ? evolutionIcons[i] : questionMarkIcon;
            nameText.text = isReached ? iconNames[i] : "？？？？";
        }
    }

    // void AdjustGridLayout()
    // {
    //     GridLayoutGroup grid = evoPanelParent.GetComponent<GridLayoutGroup>();
    //     RectTransform panelRect = evolutionPanel.GetComponent<RectTransform>();

    //     if (grid == null || panelRect == null) return;

    //     float panelWidth = panelRect.rect.width;
    //     float panelHeight = panelRect.rect.height;

    //     int columnCount = 4; // 列数（例：4列）
    //     int rowCount = Mathf.CeilToInt((float)evolutionIcons.Count / columnCount);

    //     float spacing = panelWidth * 0.02f; // 2%をSpacingとPaddingに使用
    //     float iconWidth = (panelWidth - spacing * (columnCount + 1)) / columnCount;
    //     float iconHeight = (panelHeight - spacing * (rowCount + 1)) / rowCount;

    //     grid.cellSize = new Vector2(iconWidth, iconHeight);
    //     grid.spacing = new Vector2(spacing, spacing);
    //     grid.padding = new RectOffset(
    //         Mathf.RoundToInt(spacing),
    //         Mathf.RoundToInt(spacing),
    //         Mathf.RoundToInt(spacing),
    //         Mathf.RoundToInt(spacing)
    //     );

    //     Debug.Log($"✅ GridLayout 調整完了：cellSize={iconWidth:F0}x{iconHeight:F0}, spacing={spacing:F0}");
    // }

    


}
