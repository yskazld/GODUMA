using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemSlotUI : MonoBehaviour
{
    [Header("参照")]
    public Button infoButton; // ← InfoButton をここにドラッグ
    public string itemDescription; // ← Inspectorで個別に設定
    public GameObject infoPanel; // 共通パネル
    public TMP_Text infoText;

    void Start()
    {
        if (infoButton != null)
            infoButton.onClick.AddListener(ShowInfo);
    }

    void ShowInfo()
    {
        if (infoPanel != null && infoText != null)
        {
            infoPanel.SetActive(true);
            infoText.text = itemDescription;
        }
    }
}
