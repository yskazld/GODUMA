using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TitleSettingManager : MonoBehaviour
{
    [Header("Configパネル関連")]
    public GameObject configPanel;         // ← ConfigPanelをここにアサイン
    public Button configButton;            // ← これを追加（Inspectorでアサイン）

    [Header("Hardモード")]
    public Toggle hardModeToggle;

    [Header("ゲーム説明パネル")]
    public GameObject explanationPanel;

    [Header("説明表示ボタン")]
    public Button showExplanationButton;

    [Header("閉じるボタン")]
    public Button closeExplanationButton;

    void Start()
    {
        // PlayerPrefsからHardモード設定を読み込み
        hardModeToggle.isOn = PlayerPrefs.GetInt("HardMode", 0) == 1;

        // Toggle変更で保存
        hardModeToggle.onValueChanged.AddListener(delegate
        {
            PlayerPrefs.SetInt("HardMode", hardModeToggle.isOn ? 1 : 0);
            PlayerPrefs.Save();
        });

        // Configパネル非表示で開始
        if (configPanel != null) configPanel.SetActive(false);

        // Configボタン押したらパネルを開く
        if (configButton != null)
        {
            configButton.onClick.AddListener(OpenConfigPanel);
        }

        explanationPanel.SetActive(false); // 最初は非表示

        showExplanationButton.onClick.AddListener(() =>
        {
            explanationPanel.SetActive(true);
        });

        closeExplanationButton.onClick.AddListener(() =>
        {
            explanationPanel.SetActive(false);
        });
    }

    public void OpenConfigPanel()
    {
        if (configPanel != null) configPanel.SetActive(true);
    }

    public void CloseConfigPanel()
    {
        if (configPanel != null) configPanel.SetActive(false);
    }
}
