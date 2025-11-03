using GoogleMobileAds.Api; // ← これが必要！
using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Congratulation画像")]
    public Sprite congratImageA; // Lv10〜18
    public Sprite congratImageB; // Lv19〜27
    public Sprite congratImageC; // Lv28〜32
    public Sprite congratImageD; // Lv33〜36
    public GameObject congratPanel; // パネル本体（Imageがアタッチされている）
    public Image congratImageUI; // ← Hierarchy上の CongratImageUI をアサイン

    //public Image congratImageUI; // UI上のImage（Imageコンポーネント）

    // [Header("Congratulation 画像")]
    // public GameObject congratImageAObj;// Lv10〜18
    // public GameObject congratImageBObj; // Lv19〜27
    // public GameObject congratImageCObj;// Lv28〜32
    // public GameObject congratImageDObj; // Lv33〜36



    [Header("Game Over UI")]
    public GameObject gameOverPanel;

    public int score = 0;
    public int currentLevel = 1; // 初期値はLv1など
    public TMP_Text scoreText; // スコア表示用のTextMeshPro
    public int bestScore = 0;
    public TMP_Text bestScoreText;
    public GameObject saveMessagePanel; // ← UI側でアサイン

    private HashSet<int> shownCongratLevels = new HashSet<int>();


    [Header("アイテム獲得メッセージUI")]
    public GameObject itemMessagePanel;
    public TMP_Text itemMessageText;

    [Header("干支の進化プレハブ")]
    public List<GameObject> evolutionPrefabs;

    private GameObject nextForcedIconPrefab = null;

    [Header("アイテムアイコン")]
    Sprite icon;
    public Sprite deleteIconSprite;
    public Sprite summonIconSprite;
    public Sprite topLineIconSprite;

    // GameManager のフィールドに追加
    [Header("アイテムメッセージ用の画像UI")]
    public Image itemIconImage;

    public bool isGameOver = false;
    public bool isGameStarted = false;

    private Coroutine timerCoroutine;

    private int nextScoreRewardThreshold = 1000;

    [Header("一時メッセージ用UI")]
    public GameObject noticePanel;
    public TMP_Text noticeText;

    [Header("確認パネルUI")]
    public GameObject confirmReturnPanel; // ← パネル
    public TMP_Text confirmReturnText;  // ← TextMeshProの参照



    void Start()
    {
        try
        {
            // AdMob 初期化とバナー読み込みをすべて try-catch に包む
            AdmobLibrary.FirstSetting();

            AdmobLibrary.RequestBanner(
                AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(Screen.width),
                AdPosition.Bottom,
                false
            );

            AdmobLibrary.LoadReward();
        }
        catch (System.Exception e)
        {
            Debug.LogError("🔥 Admob 初期化中にエラー発生: " + e.Message);
        }

        // セーブデータの有無で処理分岐
        if (PlayerPrefs.GetInt("IsContinue", 0) == 1)
        {
            Debug.Log("🔁 IsContinue = 1 → セーブデータから再開");
            LoadSavedGame();
        }
        else
        {
            Debug.Log("🆕 IsContinue = 0 → 新規スタート");
            StartGame();
        }
    }




    /// <summary>
    /// ゲーム開始時に呼ばれる処理
    /// </summary>
    public void StartGame()
    {
        Debug.Log("🎮 StartGame() 実行");

        ResetScore(); // スコアを初期化

        // 🔥 既存の干支アイコンを全て削除
        foreach (var icon in FindObjectsOfType<EtoIconController>())
        {
            Destroy(icon.gameObject);
        }

        // ここにクイズ開始、アイテム生成などの初期化処理を記述
        // 例：QuizManager.Instance.StartQuiz();
        // 例：EtoSpawner.Instance.StartSpawning();
    }
    public void ResumeGame()
    {
        isGameOver = false;
        isGameStarted = true;

        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
        }
        timerCoroutine = StartCoroutine(TimerCountDown());

        // BGMやエフェクトの再開などもあればここで
    }

    private IEnumerator TimerCountDown()
    {
        while (!isGameOver)
        {
            yield return new WaitForSeconds(1f);
            Debug.Log("🕒 タイマー動作中");
        }
    }

    void Awake()
    {
        bestScore = PlayerPrefs.GetInt("BestScore", 0); // 初期値は0
        UpdateScoreUI();
        Debug.Log("GameManager 起動");

        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (gameOverPanel != null)
        {
            Debug.Log("GameOverPanel を非表示にします");
            gameOverPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("gameOverPanel が設定されていません！");
        }
    }

    public void GameOver()
    {
        Debug.Log("Game Over 発生");
        if (score > bestScore)
        {
            bestScore = score;
            PlayerPrefs.SetInt("BestScore", bestScore);
            PlayerPrefs.Save();
            Debug.Log("🎉 ハイスコア更新：" + bestScore);
        }
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true); // 表示ON
        }
    }

    // ボタン用関数（UIから呼ぶ）
    public void Retry()
    {
        int adCounter = PlayerPrefs.GetInt("AdCounter", 0);
        adCounter++;
        PlayerPrefs.SetInt("AdCounter", adCounter);
        Debug.Log($"🔁 Retryボタンが押されました（累計: {adCounter}）");

        if (AdmobLibrary.IsInterstitialReady() && adCounter >= 3)
        {
            AdmobLibrary.OnCloseInterstitial = () =>
            {
                PlayerPrefs.SetInt("AdCounter", 0); // カウントリセット
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            };

            AdmobLibrary.PlayInterstitial();
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }


    public void ReturnToTitle()
    {
        int adCounter = PlayerPrefs.GetInt("AdCounter", 0);
        adCounter++;
        PlayerPrefs.SetInt("AdCounter", adCounter);
        Debug.Log($"🏠 タイトルへ戻るボタンが押されました（累計: {adCounter}）");

        if (AdmobLibrary.IsInterstitialReady() && adCounter >= 3)
        {
            AdmobLibrary.OnCloseInterstitial = () =>
            {
                PlayerPrefs.SetInt("AdCounter", 0);
                SceneManager.LoadScene("TitleScene");
            };

            AdmobLibrary.PlayInterstitial();
        }
        else
        {
            SceneManager.LoadScene("TitleScene");
        }
    }

    public void RetryWithReward()
    {
        Debug.Log("🧪 RetryWithReward() が実行されました");

        if (AdmobLibrary.IsActiveReward())
        {
            Debug.Log("🎬 リワード広告を表示します");

            AdmobLibrary.OnReward = (amount) =>
            {
                Debug.Log("🎁 リワード視聴完了 → アイテム付与＋ゲームを最初から再開");

                GiveRandomItem(); // ★ 共通化

                if (gameOverPanel != null)
                    gameOverPanel.SetActive(false);

                if (StageManager.Instance != null)
                    StageManager.Instance.ResetStage();

                // ✅ アイコン削除（StartGameで実行）
                // ✅ スコア初期化（StartGameで実行）
                StartGame();  // ← Retry() と同様にゲームを最初から開始

                AdmobLibrary.LoadReward();
            };

            AdmobLibrary.ShowReward();
        }
        else
        {
            Debug.LogWarning("⚠️ リワード広告がまだ読み込まれていません");
        }
    }
    /// <summary>
    /// リワードを見て再開する
    /// </summary>
    public void ContinueWithReward()
    {
        Debug.Log("🧪 ContinueWithReward() が実行されました");

        if (AdmobLibrary.IsActiveReward())
        {
            AdmobLibrary.OnReward = (amount) =>
            {
                Debug.Log("🎁 リワード視聴完了 → アイテム付与＋継続再開");

                GiveRandomItem(); // ★ 共通化

                // ▼ 低レベルアイコンを削除する処理を追加
                int currentMaxLevel = EtoIconController.GetMaxEtoLevelInScene();
                int threshold = Mathf.Max(1, currentMaxLevel - 6);
                Debug.Log($"🧹 低レベルアイコン削除処理: Lv{threshold} 未満を除去");
                RemoveLowLevelIcons(threshold);

                // 削除処理のあとに追加
                int removedCount = RemoveLowLevelIcons(threshold);
                ShowNotice($"レベル{threshold}未満のアイコンを{removedCount}個削除しました！", 2f);


                if (gameOverPanel != null)
                    gameOverPanel.SetActive(false);

                // ステージやスコアはリセットしない（継続）
                ResumeGame(); // ここで必要な再開処理を呼ぶ

                AdmobLibrary.LoadReward();
            };

            AdmobLibrary.ShowReward();
        }
        // else
        // {
        //     Debug.LogWarning("⚠️ リワード広告がまだ読み込まれていません");
        // }
        else
        {
            Debug.LogWarning("⚠️ リワード広告がまだ読み込まれていません → 直接継続再開します");

            // ★以下、広告なしでも再開処理を実行
            GiveRandomItem();

            int currentMaxLevel = EtoIconController.GetMaxEtoLevelInScene();
            int threshold = Mathf.Max(1, currentMaxLevel - 6);
            Debug.Log($"🧹 低レベルアイコン削除処理: Lv{threshold} 未満を除去（広告なし）");
            int removedCount = RemoveLowLevelIcons(threshold);
            ShowNotice($"レベル{threshold}未満のアイコンを{removedCount}個削除しました！", 2f);

            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);

            ResumeGame();
        }
    }
    int RemoveLowLevelIcons(int thresholdLevel)
    {
        int count = 0;
        EtoIconController[] allIcons = FindObjectsOfType<EtoIconController>();
        foreach (var icon in allIcons)
        {
            if (icon.etoLevel < thresholdLevel)
            {
                Destroy(icon.gameObject);
                count++;
            }
        }
        return count;
    }

    /// <summary>
    /// リワードを見て再開する時に、アイコンレベルの低いやつを消す表示
    /// </summary>
    /// <param name="message"></param>
    /// <param name="duration"></param>
    public void ShowNotice(string message, float duration = 2f)
    {
        if (noticePanel != null && noticeText != null)
        {
            noticeText.text = message;
            noticePanel.SetActive(true);
            StartCoroutine(HideNoticeAfterSeconds(duration));
        }
    }

    IEnumerator HideNoticeAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        noticePanel.SetActive(false);
    }


    private void GiveRandomItem()
    {
        float rand = UnityEngine.Random.value;
        string message = "";
        Sprite icon = null;

        if (rand < 0.5f)
        {
            ItemManager.Instance.AddDeleteItem(1);
            message = "Deleteアイテムをゲット！";
            icon = deleteIconSprite;
        }
        else if (rand < 0.5f + (1f / 6f))
        {
            ItemManager.Instance.AddSummonItem(1);
            message = "Summonアイテムをゲット！";
            icon = summonIconSprite;
        }
        else
        {
            ItemManager.Instance.topLineItemCount++;
            PlayerPrefs.SetInt("TopLineItemCount", ItemManager.Instance.topLineItemCount);
            PlayerPrefs.Save();
            ItemManager.Instance.UpdateItemCountUI();
            message = "TopLineUPアイテムをゲット！";
            icon = topLineIconSprite;
        }

        ShowItemMessage(message, icon);
    }


    private IEnumerator HideItemMessageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        itemMessagePanel.SetActive(false);
    }

    public void ShowItemMessage(string message, Sprite icon)
    {
        if (itemMessagePanel == null || itemMessageText == null || itemIconImage == null || icon == null)
        {
            Debug.LogWarning("⚠️ ShowItemMessage: UIまたはアイコンが未設定のため、表示をスキップします");
            return;
        }

        itemMessageText.text = message;
        itemIconImage.sprite = icon;
        itemIconImage.gameObject.SetActive(true);

        itemMessagePanel.SetActive(true);
        StartCoroutine(HideItemMessageAfterDelay(2f));
    }


    void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = $"スコア : {score}";

        if (bestScoreText != null)
            bestScoreText.text = $"ベスト : {bestScore}";
    }

    public void ResetScore()
    {
        score = 0;
        UpdateScoreUI();
    }

    public void ShowCongratulation()
    {
        Debug.Log("🎉 ShowCongratulation() が呼ばれました");

        if (congratPanel != null)
        {
            congratPanel.SetActive(true);
            StopCoroutine(nameof(HideCongratulationAfterDelay)); // 念のため停止
            StartCoroutine(HideCongratulationAfterDelay(2f));
        }
        else
        {
            Debug.LogWarning("congratulationPanel が未設定です！");
        }
    }

    private IEnumerator HideCongratulationAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (congratPanel != null)
        {
            Debug.Log("🕒 2秒経過 → Congratulation非表示にします");
            congratPanel.SetActive(false);
        }
    }
   

  

    public void ShowCongratulationIfFirstTime(int level)
    {
        Debug.Log($"🎯 現在の進化レベル: {level}");

        if (level >= 9 && !shownCongratLevels.Contains(level))
        {
            shownCongratLevels.Add(level);
            Debug.Log($"🎉 Congratulation 表示初回: Lv{level}");

            if (congratImageUI != null)
            {
                if (level <= 18)
                {
                    congratImageUI.sprite = congratImageA;
                    Debug.Log("🎨 画像Aを設定");
                }
                else if (level <= 27)
                {
                    congratImageUI.sprite = congratImageB;
                    Debug.Log("🎨 画像Bを設定");
                }
                else if (level <= 32)
                {
                    congratImageUI.sprite = congratImageC;
                    Debug.Log("🎨 画像Cを設定");
                }
                else
                {
                    congratImageUI.sprite = congratImageD;
                    Debug.Log("🎨 画像Dを設定");
                }
            }
            else
            {
                Debug.LogWarning("⚠️ congratImageUI が null です！");
            }

            ShowCongratulation();
        }
    }


    // 保存（例：GameOver時や終了時に呼ぶ）
    public void SaveGame()
    {
        EtoIconController[] icons = FindObjectsOfType<EtoIconController>();
        List<EtoIconData> iconDataList = new List<EtoIconData>();

        foreach (var icon in icons)
        {
            iconDataList.Add(new EtoIconData
            {
                x = icon.transform.position.x,
                y = icon.transform.position.y,
                level = icon.etoLevel
            });
        }

        SaveDataManager.SaveGame(score, currentLevel, iconDataList);

        // ✅「続きから」のフラグを保存
        PlayerPrefs.SetInt("IsContinue", 1);
        PlayerPrefs.Save();

        StartCoroutine(ShowSaveMessage()); // ← ★ここ追加
    }

    private IEnumerator ShowSaveMessage()
    {
        if (saveMessagePanel != null)
        {
            saveMessagePanel.SetActive(true);
            yield return new WaitForSeconds(2f);
            saveMessagePanel.SetActive(false);
        }
    }
  

    public void LoadSavedGame()
    {
        SaveData data = SaveDataManager.LoadGame();
        if (data == null)
        {
            Debug.LogWarning("セーブデータがありません");
            return;
        }

        score = data.score;
        currentLevel = data.currentLevel;
        UpdateScoreUI();

        Debug.Log($"📦 読み込み開始：スコア={score} / レベル={currentLevel} / アイコン数={data.icons.Count}");
        Debug.Log($"📚 evolutionPrefabs の数: {evolutionPrefabs.Count}");

        // 画面上のアイコン削除
        foreach (var icon in FindObjectsOfType<EtoIconController>())
        {
            Destroy(icon.gameObject);
        }

        // アイコンを再生成
        foreach (var iconData in data.icons)
        {
            int index = iconData.level - 1;
            if (index < 0 || index >= evolutionPrefabs.Count)
            {
                Debug.LogError($"❌ 無効なアイコンレベル: level={iconData.level}（index={index}） → スキップ");
                continue;
            }

            GameObject iconPrefab = evolutionPrefabs[iconData.level - 1];
            Vector3 pos = new Vector3(iconData.x, iconData.y, 0f);
            GameObject icon = Instantiate(iconPrefab, pos, Quaternion.identity);

            EtoIconController controller = icon.GetComponent<EtoIconController>();
            controller.etoLevel = iconData.level;
        }

        // ステージ再構築（必要に応じて）
        if (StageManager.Instance != null)
        {
            StageManager.Instance.ResetStage(); // サイズなど初期化
            Debug.Log("🛠 ステージをリセットしました");
        }

        // ゲームを進行状態にする
        isGameStarted = true;
        isGameOver = false;

        if (timerCoroutine != null)
            StopCoroutine(timerCoroutine);
        timerCoroutine = StartCoroutine(TimerCountDown());

        Debug.Log("🔁 セーブデータから再構築完了（ゲーム再開）");
    }


    public void SetNextIcon(GameObject prefab)
    {
        nextForcedIconPrefab = prefab;
    }

    public bool TryConsumeNextForcedIcon(out GameObject prefab)
    {
        if (nextForcedIconPrefab != null)
        {
            Debug.Log($"🎯 強制アイコンを返す: {nextForcedIconPrefab.name}");
            prefab = nextForcedIconPrefab;
            nextForcedIconPrefab = null;
            return true;
        }

        prefab = null;
        return false;
    }

    public GameObject GetNextIcon()
    {
        if (nextForcedIconPrefab != null)
        {
            Debug.Log($"🎯 強制アイコンを返す: {nextForcedIconPrefab.name}");
            var result = nextForcedIconPrefab;
            nextForcedIconPrefab = null; // 1回のみ
            return result;
        }
        Debug.Log("🎲 ランダムアイコンを返す");
        return GetRandomIcon(); // 通常のランダム処理
    }
  
    public GameObject GetRandomIcon()
    {
        int maxIndex = evolutionPrefabs.Count - 1;
        int index = Random.Range(0, maxIndex);

        var prefab = evolutionPrefabs[index];
        if (prefab == null)
        {
            Debug.LogError($"❌ evolutionPrefabs[{index}] が null です！");
        }
        else
        {
            Debug.Log($"✅ ランダム選出 → {prefab.name}");
        }

        return prefab;
    }

    public void AddScore(int value)
    {
        score += value;
        Debug.Log($"スコア加算：+{value} → 合計: {score}");
        UpdateScoreUI();

        // ← 🔧 最初に定義しておく
        LoginBonusManager loginBonusManager = FindObjectOfType<LoginBonusManager>();

        // --- ✅ スコア報酬チェック ---
        if (score >= nextScoreRewardThreshold)
        {
            int rewardsToGive = score / 1000 - (nextScoreRewardThreshold / 1000 - 1);
            int totalCoins = rewardsToGive * 20;

            // コイン付与
            int currentCoins = PlayerPrefs.GetInt("Coin", 0);
            currentCoins += totalCoins;
            PlayerPrefs.SetInt("Coin", currentCoins);
            PlayerPrefs.Save();

            Debug.Log($"💰 スコア報酬！+{totalCoins}コイン付与（スコア:{score}）");

            // UI更新（LoginBonusManagerを探して更新）
            if (loginBonusManager != null)
            {
                loginBonusManager.UpdateCoinUI();
                loginBonusManager.ShowCoinGainMessage(totalCoins); // ← ★ここも中に入れる
            }

            // 次の報酬ライン更新
            nextScoreRewardThreshold = ((score / 1000) + 1) * 1000;
        }

    }
    public int GetCurrentMaxLevel()
    {
        return StageManager.Instance?.GetCurrentMaxLevel() ?? 4;
    }

    public int GetMaxEtoLevel()
    {
        return EtoIconController.GetMaxEtoLevelInScene();
    }

    // タイトルに戻るボタンから呼ぶ
    public void ShowReturnToTitleConfirm()
    {
        if (confirmReturnPanel != null)
        {
            confirmReturnPanel.SetActive(true);

            if (confirmReturnText != null)
            {
                confirmReturnText.text = "本当にタイトルへ戻りますか？\n進行中の内容は保存されません。";
            }
        }
    }

    // Yesボタン用（本当に戻る）
    public void ConfirmReturnToTitle()
    {
        SaveGame(); // ← 事前にSaveGame()を呼ぶ（必要であれば）
        ReturnToTitle(); // 既存の処理を再利用
    }

    // Noボタン用（キャンセル）
    public void CancelReturnToTitle()
    {
        if (confirmReturnPanel != null)
        {
            confirmReturnPanel.SetActive(false);
        }
    }



}
