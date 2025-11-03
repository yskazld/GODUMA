using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class EtoIconController : MonoBehaviour
{

    //TopLine色の点滅用
    private SpriteRenderer topLineRenderer;
    private Coroutine blinkCoroutine;

    //TopLine ３秒以上触れるとOutにした
    private bool isTouchingTopLine = false;
    private float topLineTouchStartTime = 0f;
    public float topLineWaitSeconds = 2f;

    public int etoLevel = 1;
    public List<GameObject> evolutionPrefabs;
    public GameObject evolutionEffectPrefab;

    private bool hasEvolved = false;
    private bool hasLanded = false;
    private bool alreadyTriggeredGameOver = false;
    private float topLineY;

    void Start()
    {
        // TopLineのY座標を取得（TopLineに "TopLine" タグをつけておくこと）
        GameObject topLine = GameObject.FindWithTag("TopLine");
        if (topLine != null)
        {
            topLineY = topLine.transform.position.y;

            // 🔽 SpriteRendererを取得（点滅用）
            topLineRenderer = topLine.GetComponent<SpriteRenderer>();
            if (topLineRenderer == null)
            {
                Debug.LogWarning("⚠ TopLineにSpriteRendererがついていません。点滅表示は無効になります。");
            }
        }
        else
        {
            Debug.LogError("TopLine オブジェクトが見つかりません。Tag を 'TopLine' に設定してください。");
        }
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        // 着地判定
        if (!hasLanded && (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Eto")))
        {
            hasLanded = true;
        }

        // 進化処理（これまでのままでOK）
        EtoIconController other = collision.gameObject.GetComponent<EtoIconController>();
        if (other != null && other.etoLevel == this.etoLevel && !hasEvolved && !other.hasEvolved)
        {
            if (this.GetInstanceID() > other.GetInstanceID()) return;

            if (etoLevel - 1 < evolutionPrefabs.Count && evolutionPrefabs[etoLevel - 1] != null)
            {
                Vector3 spawnPos = (this.transform.position + other.transform.position) / 2f;
                GameObject evolved = Instantiate(evolutionPrefabs[etoLevel - 1], spawnPos, Quaternion.identity);

                // スコア加算：進化後のレベル × 10点
                int evolvedLevel = etoLevel + 1; // 進化後のレベル（1UP）
                var evolvedController = evolved.GetComponent<EtoIconController>();
                if (evolvedController != null)
                {
                    evolvedController.etoLevel = evolvedLevel; // ← 🔥 ここが大事
                }

                GameManager.Instance?.AddScore(evolvedLevel * 10);

                // ▼ ここを追加
                GameManager.Instance.currentLevel = evolvedLevel;

                // ▼ ついでにセーブする（自動保存）
                //GameManager.Instance.SaveGame();

                if (evolutionEffectPrefab != null)
                {
                    Instantiate(evolutionEffectPrefab, spawnPos, Quaternion.identity);
                }

                // ✅ Congratulation 表示（ここがポイント）
                //GameManager.Instance?.ShowCongratulation();

                Debug.Log($"🌟 進化！evolvedLevel = {evolvedLevel}");

                // 条件確認ログ
                Debug.Log($"IsLevelAlreadyPresent({evolvedLevel}) = {IsLevelAlreadyPresent(evolvedLevel)}");

                GameManager.Instance?.ShowCongratulationIfFirstTime(evolvedLevel);


                Destroy(this.gameObject);
                Destroy(other.gameObject);
            }
        }
    }

    void Update()
    {
        float gameOverThresholdY = topLineY - 0.3f; // ★判定ラインを2下げる

        if (hasLanded && transform.position.y > gameOverThresholdY)
        {
            if (!isTouchingTopLine)
            {
                isTouchingTopLine = true;
                topLineTouchStartTime = Time.time;
                Debug.Log("⚠️ TopLine に触れました → 点滅開始");

                if (topLineRenderer != null && blinkCoroutine == null)
                {
                    blinkCoroutine = StartCoroutine(BlinkTopLine());
                }
            }
            else if (!alreadyTriggeredGameOver && Time.time - topLineTouchStartTime >= topLineWaitSeconds)
            {
                alreadyTriggeredGameOver = true;
                Debug.Log("💀 3秒経過 → GameOver");

                if (blinkCoroutine != null)
                {
                    StopCoroutine(blinkCoroutine);
                    blinkCoroutine = null;
                    topLineRenderer.color = Color.white; // 元に戻す
                }

                GameManager.Instance.GameOver();
            }
        }
        else
        {
            if (isTouchingTopLine)
            {
                Debug.Log("🟢 TopLineから離れた → 点滅停止");

                if (blinkCoroutine != null)
                {
                    StopCoroutine(blinkCoroutine);
                    blinkCoroutine = null;
                    topLineRenderer.color = Color.white; // 元に戻す
                }

                isTouchingTopLine = false;
            }
        }
    }

    private IEnumerator BlinkTopLine()
    {
        bool isRed = false;
        while (true)
        {
            if (topLineRenderer != null)
            {
                topLineRenderer.color = isRed ? Color.white : Color.red;
            }
            isRed = !isRed;
            yield return new WaitForSeconds(0.3f);
        }
    }

    // 現在のシーン上に、同じレベルの干支アイコンがあるかチェック
    bool IsLevelAlreadyPresent(int level)
    {
        EtoIconController[] allIcons = FindObjectsOfType<EtoIconController>();
        foreach (var icon in allIcons)
        {
            if (icon.etoLevel == level)
            {
                return true;
            }
        }
        return false;
    }

    void OnMouseDown()
    {
        if (ItemManager.Instance?.IsInDeleteMode() == true)
        {
            ItemManager.Instance.TryDeleteIcon(this);
        }
    }
public static int GetMaxEtoLevelInScene()
{
    int maxLevel = 1;
    EtoIconController[] allIcons = GameObject.FindObjectsOfType<EtoIconController>();
    foreach (var icon in allIcons)
    {
        if (icon.etoLevel > maxLevel)
        {
            maxLevel = icon.etoLevel;
        }
    }
    return maxLevel;
}


}