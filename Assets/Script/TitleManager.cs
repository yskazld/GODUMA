using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using GoogleMobileAds.Api;

public class TitleScene : MonoBehaviour
{
    public Button continueButton;

    void Start()
    {

        int index = PlayerPrefs.GetInt("SelectedBGMIndex", 0);
        if (BGMManager.Instance != null)
        {
            BGMManager.Instance.PlaySelectedBGM(index);
        }
        else
        {
            Debug.LogWarning("TitleScene: BGMManager.Instance が見つかりませんでした。シーンに BGMManager を配置しているか確認してください。", this);
        }

        
        AdmobLibrary.FirstSetting(); // 初期化（ゲーム全体で一度だけならここはスキップしてもOK）
        AdmobLibrary.RequestBanner(
            AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(Screen.width),
            AdPosition.Bottom,
            false
        );
        // セーブデータがなければ「つづきから」ボタンを非表示にする
        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(SaveDataManager.HasSave());
        }
        else
        {
            Debug.LogWarning("TitleScene: continueButton が未設定です。インスペクタで Button をアサインしてください。", this);
        }
    }

    public void OnContinueButtonPressed()
    {
        PlayerPrefs.SetInt("IsContinue", 1); // フラグで再開モードを記録
        SceneManager.LoadScene("GameScene");
    }

    public void OnStartNewGame()
    {
        SaveDataManager.ClearSave(); // 新規スタート時はセーブデータをクリア
        PlayerPrefs.SetInt("IsContinue", 0);
        SceneManager.LoadScene("GameScene");
    }
}
