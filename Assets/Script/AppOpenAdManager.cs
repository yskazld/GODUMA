using System;
using UnityEngine;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;

/// <summary>
/// アプリ起動時広告（App Open Ad）を管理するシングルトンマネージャークラス
/// ・広告のロード、表示、イベントハンドリングを行います
/// ・AdMob SDK の AppOpenAd API に基づいて動作します
/// </summary>
public class AppOpenAdManager
{
    // シングルトンインスタンス
    private static AppOpenAdManager instance;

    // 読み込んだ広告オブジェクト
    private AppOpenAd appOpenAd;

    // 広告の状態管理
    private bool isAdAvailable = false;
    private bool isShowingAd = false;

    /// <summary>
    /// インスタンスを取得（シングルトン）
    /// </summary>
    public static AppOpenAdManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new AppOpenAdManager();
                Debug.Log("AppOpenAdManager インスタンスが生成されました");
            }
            return instance;
        }
    }

    // 広告イベント通知
    public event Action OnLoaded;
    public event Action OnFailedToLoad;
    public event Action OnAdClosed;

    // 広告ユニットID（プラットフォームに応じて切り替え）
    private string AdUnitId
    {
        get
        {
#if UNITY_ANDROID
            return "ca-app-pub-6073747809973329~4944102237"; // Android用ユニットID（注意：~ではなく/）
#elif UNITY_IOS
            return "ca-app-pub-6073747809973329~4503202815"; // iOS用ユニットID
#else
            return "unexpected_platform";
#endif
        }
    }

    /// <summary>
    /// App Open Ad を読み込む
    /// </summary>
    public void LoadAd()
    {
        if (isAdAvailable)
        {
            Debug.Log("広告はすでに読み込まれています");
            return;
        }

        Debug.Log("App Open Ad の読み込みを開始します");

        // 新しい広告リクエストを作成
        AdRequest request = new AdRequest();

        // AppOpenAd.LoadAd の呼び出し
        AppOpenAd.Load(AdUnitId, request, (appOpenAd, error) =>
        {
            if (error != null)
            {
                Debug.LogError("AppOpenAd ロード失敗: " + error.GetMessage());
                OnFailedToLoad?.Invoke();
                return;
            }

            Debug.Log("AppOpenAd ロード成功");
            this.appOpenAd = appOpenAd;
            isAdAvailable = true;
            RegisterAdEvents();
            OnLoaded?.Invoke();
        });
    }

    /// <summary>
    /// 読み込まれた広告があれば表示する
    /// </summary>
    public void ShowAdIfAvailable()
    {
        if (isAdAvailable && !isShowingAd && appOpenAd != null && appOpenAd.CanShowAd())
        {
            Debug.Log("App Open Ad を表示します");
            appOpenAd.Show();
            isShowingAd = true;
        }
        else
        {
            Debug.Log("広告は利用できないか、既に表示中です");
        }
    }

    /// <summary>
    /// 明示的に広告を表示（条件チェック付き）
    /// </summary>
    public void Show()
    {
        Debug.Log("App Open Ad を表示しようとしています");

        if (appOpenAd != null && appOpenAd.CanShowAd())
        {
            appOpenAd.Show();
            isShowingAd = true;
        }
        else
        {
            Debug.Log("広告が読み込まれていないか、表示条件を満たしていません");
        }
    }

    /// <summary>
    /// AppOpenAd に各種イベントを登録
    /// </summary>
    private void RegisterAdEvents()
    {
        if (appOpenAd == null) return;

        appOpenAd.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("App Open Ad が閉じられました");
            isAdAvailable = false;
            isShowingAd = false;
            OnAdClosed?.Invoke();
        };

        appOpenAd.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("App Open Ad の表示に失敗しました: " + error.GetMessage());
            isAdAvailable = false;
            isShowingAd = false;
        };

        appOpenAd.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("App Open Ad の表示が開始されました");
        };
    }
}
