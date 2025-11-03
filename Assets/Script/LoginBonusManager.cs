using TMPro;
using System;
using UnityEngine;
using System.Collections;

public class LoginBonusManager : MonoBehaviour
{
    public int dailyCoinBonus = 50; // 毎日のボーナス量
    private const string LAST_LOGIN_KEY = "LastLoginDate";
    private const string COIN_KEY = "Coin";

    public TMP_Text coinText; // ← Inspectorにセット

    //public int currentCoins;

    public int currentCoins = 0; // ← デバッグ用に固定
    public TMP_Text coinGainText; // ← Inspector でアサインする


    void Start()
    {
#if UNITY_EDITOR
        // ★ デバッグ用にコイン700枚に設定
        //currentCoins = 700;
        //PlayerPrefs.SetInt(COIN_KEY, currentCoins);
        //PlayerPrefs.SetInt(COIN_KEY, 0);
        //PlayerPrefs.SetInt("DeleteItemCount", 0);
        //PlayerPrefs.SetInt("SummonItemCount", 0);
        //PlayerPrefs.SetInt("TopLineItemCount", 0);
       
        PlayerPrefs.Save();
        Debug.Log("💰 デバッグ用にコインを700枚に設定しました");
#endif

        // 保存されているコインを読み込む（なければ0）
        currentCoins = PlayerPrefs.GetInt(COIN_KEY, 0);

        CheckLoginBonus();
        UpdateCoinUI(); // 起動時に必ずUI更新
    }

    void CheckLoginBonus()
    {
        string today = DateTime.Now.ToString("yyyyMMdd");

        string lastLogin = PlayerPrefs.GetString(LAST_LOGIN_KEY, "");

        if (lastLogin != today)
        {
            Debug.Log($"🎉 ログインボーナス獲得！+{dailyCoinBonus}コイン");
            int currentCoins = PlayerPrefs.GetInt(COIN_KEY, 0);
            currentCoins += dailyCoinBonus;

            PlayerPrefs.SetInt(COIN_KEY, currentCoins);
            PlayerPrefs.SetString(LAST_LOGIN_KEY, today);
            PlayerPrefs.Save();

            UpdateCoinUI(); // ← ここでUI更新
        }
        else
        {
            Debug.Log("📅 今日はすでにログイン済みです。");
        }
    }

    public int GetCurrentCoin()
    {
        return PlayerPrefs.GetInt(COIN_KEY, 0);
    }

    public void UpdateCoinUI()
    {
        if (coinText != null)
        {
            int coin = PlayerPrefs.GetInt(COIN_KEY, 0);
            coinText.text = $"コイン：{coin}";
        }
        else
        {
            Debug.LogWarning("coinText が設定されていません！");
        }
    }
    public bool SpendCoins(int amount)
    {
        int current = PlayerPrefs.GetInt(COIN_KEY, 0);
        if (current >= amount)
        {
            PlayerPrefs.SetInt(COIN_KEY, current - amount);
            PlayerPrefs.Save();
            UpdateCoinUI();
            return true;
        }
        return false;
    }

    public void ShowCoinGainMessage(int amount)
    {
        if (coinGainText != null)
        {
            coinGainText.text = $"+{amount}コイン！";
            coinGainText.gameObject.SetActive(true);
            StartCoroutine(HideCoinGainTextAfterDelay(1.5f)); // 1.5秒で非表示
        }
    }

    private IEnumerator HideCoinGainTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (coinGainText != null)
        {
            coinGainText.gameObject.SetActive(false);
        }
    }
}
