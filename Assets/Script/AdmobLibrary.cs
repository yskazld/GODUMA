using GoogleMobileAds.Api;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// adMobを使用するためのクラス
/// </summary>
public class AdmobLibrary
{
    private static BannerView _bannerView;
    private static InterstitialAd _interstitialAd;
    private static RewardedAd _rewardedAd;

    public static Action<double> OnReward;

    public static Action OnLoadedInterstitial;

    public static Action OnCloseInterstitial;

    /// <summary>
    /// ゲーム起動　初回に一度だけ呼ぶ
    /// </summary>
    public static void FirstSetting()
    {
        //13歳以下を対象と「する」場合はtrue
        RequestConfiguration request = new RequestConfiguration
        {
            TagForChildDirectedTreatment = TagForChildDirectedTreatment.False,
            TestDeviceIds = new List<string> { "あなたのテスト端末ID" } // ★ここ追加！
        };

        MobileAds.SetRequestConfiguration(request);

        MobileAds.Initialize((InitializationStatus initStatus) =>
        {
            // This callback is called once the MobileAds SDK is initialized.
            InitInterstitial();

        });
    }


    /// <summary>
    /// バナー広告を生成
    /// </summary>
    /// <param name="size"></param>
    /// <param name="position"></param>
    /// <summary>
    /// バナー広告を生成して表示する（重複防止・エラーハンドリング付き）
    /// </summary>
    /// <param name="size">AdSize（使用しない場合もあり）</param>
    /// <param name="position">バナーの表示位置（基本はBottom）</param>
    /// <param name="collapsible">折りたたみバナーとして表示するか</param>
    public static void RequestBanner(AdSize size, AdPosition position, bool collapsible)
    {
#if UNITY_ANDROID
        //自分のID
        string adUnitId = "ca-app-pub-6073747809973329/5160261059"; // ← ご自身のユニットID

        //テストプレイ用ID
        //string adUnitId = "ca-app-pub-3940256099942544/6300978111";
#elif UNITY_IPHONE
        //自分のID
        string adUnitId = "ca-app-pub-6073747809973329/8465065332"; 
        // ← テストID
        //string adUnitId = "ca-app-pub-3940256099942544/6300978111";
#else
        string adUnitId = "unexpected_platform";
#endif

        // 既存のバナーを破棄してから新規生成
        if (_bannerView != null)
        {
            _bannerView.Destroy();
            _bannerView = null;
            Debug.Log("既存のバナー広告を破棄しました");
        }

        try
        {
            // アダプティブサイズを使用
            AdSize adaptiveSize = AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);
            _bannerView = new BannerView(adUnitId, adaptiveSize, position);

            // 広告リクエスト作成
            AdRequest adRequest = new AdRequest();
            if (collapsible)
            {
                adRequest.Extras.Add("collapsible", "bottom");
            }

            // ロードイベント設定（成功・失敗）
            _bannerView.OnBannerAdLoaded += () =>
            {
                Debug.Log("バナー広告の読み込みが完了しました");
            };

            _bannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
            {
                Debug.LogError("バナー広告の読み込みに失敗しました: " + error.GetMessage());
            };

            // 広告ロード
            _bannerView.LoadAd(adRequest);
            Debug.Log("バナー広告の読み込みリクエストを送信しました");

        }
        catch (Exception e)
        {
            Debug.LogError("バナー広告の初期化中にエラーが発生しました: " + e.Message);
        }
    }

    /// <summary>
    /// バナー広告を削除（破棄）
    /// </summary>
    public static void DestroyBanner()
    {
        if (_bannerView != null)
        {
            _bannerView.Destroy();
            _bannerView = null;
            Debug.Log("バナー広告を破棄しました");
        }
        else
        {
            Debug.Log("バナー広告は既に存在していません");
        }
    }


    /// <summary>
    /// インタースティシャル読み込み
    /// </summary>
    private static void InitInterstitial()
    {
#if UNITY_ANDROID
        //自分のID
        string adUnitId = "ca-app-pub-6073747809973329/3935627537";

        //テストプレイ用のID
        //string adUnitId = "ca-app-pub-3940256099942544/1033173712";
#elif UNITY_IPHONE
        //自分のID
        string adUnitId = "ca-app-pub-6073747809973329/8888345972";
        //テストプレイ用のID
        //string adUnitId = "ca-app-pub-3940256099942544/1033173712";
#else
        string adUnitId = "unexpected_platform";
#endif
        // Initialize an InterstitialAd.

        var adRequest = new AdRequest();
        if (_interstitialAd != null)
        {
            _interstitialAd.Destroy();
            _interstitialAd = null;
        }

        Debug.Log("InitInterstitial");
        // send the request to load the ad.
        InterstitialAd.Load(adUnitId, adRequest,
            (InterstitialAd ad, LoadAdError error) =>
            {
                // if error is not null, the load request failed.
                if (error != null || ad == null)
                {
                    Debug.LogError("interstitial ad failed to load an ad " +
                                   "with error : " + error);
                    return;
                }

                Debug.Log("Interstitial ad loaded with response : "
                          + ad.GetResponseInfo());

                // Raised when the ad is estimated to have earned money.
                ad.OnAdPaid += (AdValue adValue) =>
                {
                    Debug.Log(String.Format("Interstitial ad paid {0} {1}.",
                        adValue.Value,
                        adValue.CurrencyCode));
                };
                // Raised when an impression is recorded for an ad.
                ad.OnAdImpressionRecorded += () => { Debug.Log("Interstitial ad recorded an impression."); };
                // Raised when a click is recorded for an ad.
                ad.OnAdClicked += () => { Debug.Log("Interstitial ad was clicked."); };
                // Raised when an ad opened full screen content.
                ad.OnAdFullScreenContentOpened += () => { Debug.Log("Interstitial ad full screen content opened."); };
                // Raised when the ad closed full screen content.
                ad.OnAdFullScreenContentClosed += () =>
                {
                    Debug.Log("インタースティシャル広告が閉じられました。次の広告をロードします");
                    _interstitialAd.Destroy();
                    _interstitialAd = null;
                    InitInterstitial(); // ← ここ追加！

                    // 🔽追加：広告閉じたら呼ぶ
                    OnCloseInterstitial?.Invoke();
                    OnCloseInterstitial = null; // 念のためクリア
                };
                // Raised when the ad failed to open full screen content.
                ad.OnAdFullScreenContentFailed += (AdError error) =>
                {
                    Debug.LogError("Interstitial ad failed to open full screen content " +
                                   "with error : " + error);
                };
                _interstitialAd = ad;
                OnLoadedInterstitial?.Invoke();
            });
    }

    /// <summary>
    /// インタースティシャルを出す
    /// </summary>
    public static void PlayInterstitial()
    {
        Debug.Log("PlayInterstitial");
        if (_interstitialAd != null && _interstitialAd.CanShowAd())
        {
            Debug.Log("Showing interstitial ad.");
            _interstitialAd.Show();
        }
        else
        {
            Debug.LogError("Interstitial ad is not ready yet.");
        }
    }
    public static bool IsInterstitialReady()
    {
        return _interstitialAd != null && _interstitialAd.CanShowAd();
    }

    /// <summary>
    /// インタースティシャル削除
    /// </summary>
    public static void DestroyInterstitial()
    {
        if (_interstitialAd != null)
        {
            Debug.Log("DestroyInterstitial");
            _interstitialAd.Destroy();
        }
    }

    /// <summary>
    /// リワード広告
    /// </summary>
    public static void LoadReward()
    {
        string adUnitId;
#if UNITY_ANDROID
        //自分のID
        adUnitId = "ca-app-pub-6073747809973329/7915009808";

        //テストプレイ用のID
        //adUnitId = "ca-app-pub-3940256099942544/5224354917";
#elif UNITY_IPHONE
        //自分のID
        adUnitId = "ca-app-pub-6073747809973329/1385260476";
        //テストプレイ用のID
        //adUnitId = "ca-app-pub-3940256099942544/1712485313";
#else
        adUnitId = "unexpected_platform";
#endif
        var adRequest = new AdRequest();
        _rewardedAd = null;
        RewardedAd.Load(adUnitId, adRequest,
            (RewardedAd ad, LoadAdError error) =>
            {
                // if error is not null, the load request failed.
                if (error != null || ad == null)
                {
                    Debug.LogError("rewarded ad failed to load an ad " +
                                   "with error : " + error);
                    return;
                }

                Debug.Log("Rewarded ad loaded with response : "
                          + ad.GetResponseInfo());
                // Raised when the ad is estimated to have earned money.
                ad.OnAdPaid += (AdValue adValue) =>
                {
                    Debug.Log(String.Format("Rewarded ad paid {0} {1}.",
                        adValue.Value,
                        adValue.CurrencyCode));
                };
                // Raised when an impression is recorded for an ad.
                ad.OnAdImpressionRecorded += () => { Debug.Log("Rewarded ad recorded an impression."); };
                // Raised when a click is recorded for an ad.
                ad.OnAdClicked += () => { Debug.Log("Rewarded ad was clicked."); };
                // Raised when an ad opened full screen content.
                ad.OnAdFullScreenContentOpened += () => { Debug.Log("Rewarded ad full screen content opened."); };
                // Raised when the ad closed full screen content.
                ad.OnAdFullScreenContentClosed += () =>
                {
                    Debug.Log("Interstitial ad full screen content closed.");

                    // 表示が終わったら破棄
                    _interstitialAd.Destroy();
                    _interstitialAd = null;

                    // 次の広告を準備しておく
                    InitInterstitial();
                };


                // Raised when the ad failed to open full screen content.
                ad.OnAdFullScreenContentFailed += (AdError error) =>
                {
                    Debug.LogError("Rewarded ad failed to open full screen content " +
                                   "with error : " + error);
                };
                _rewardedAd = ad;
            });
    }

    /// <summary>
    /// リワード広告を作成
    /// </summary>
    public static void ShowReward()
    {
        if (_rewardedAd != null && _rewardedAd.CanShowAd())
        {
            _rewardedAd.Show((Reward reward) =>
            {
                Debug.Log($"🎥 Reward watched: {reward.Type}, {reward.Amount}");

                // 順序を変更：先に破棄 → 読み込み → 最後にゲーム処理
                _rewardedAd.Destroy();
                LoadReward();

                OnReward?.Invoke(reward.Amount);
            });
        }
    }


    /// <summary>
    /// リワード削除
    /// </summary>
    public static void DestroyReward()
    {
        if (_rewardedAd != null)
        {
            _rewardedAd.Destroy();
        }
    }

    /// <summary>
    /// リワード
    /// </summary>
    /// <returns></returns>
    public static bool IsActiveReward()
    {
        return _rewardedAd != null;
    }
}