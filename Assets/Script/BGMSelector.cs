using UnityEngine;
using TMPro;

public class BGMSelector : MonoBehaviour
{
    public TMP_Dropdown bgmDropdown;
    public TMP_Text selectedNameText;

    void Start()
    {
        // DropdownにBGM名をセット
        bgmDropdown.ClearOptions();
        bgmDropdown.AddOptions(new System.Collections.Generic.List<string>(BGMManager.Instance.bgmNames));
        bgmDropdown.onValueChanged.AddListener(OnBGMChanged);

        // 初期選択
        bgmDropdown.value = PlayerPrefs.GetInt("SelectedBGMIndex", 0);
        OnBGMChanged(bgmDropdown.value);
    }

    public void OnBGMChanged(int index)
    {
        selectedNameText.text = $"選択中：{BGMManager.Instance.bgmNames[index]}";
        PlayerPrefs.SetInt("SelectedBGMIndex", index);
        PlayerPrefs.Save();

        BGMManager.Instance.PlaySelectedBGM(index);
    }
}
