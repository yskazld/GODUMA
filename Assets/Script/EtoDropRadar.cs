using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EtoDropRadar : MonoBehaviour
{

    public float summonCooldownTime = 0f; // ← クールタイム終了時刻

    public GameObject backToTitlePanel;      // ← 追加
    public GameObject saveMessagePanel;      // ← 追加
    public GameObject ItemUseConfirmPanel;
    public GameObject evolutionPanel;
    public GameObject summonPanel;
    public GameObject gameOverPanel;

    public GameObject[] etoPrefabs;            // 干支の実体プレハブ
    public Camera mainCamera;
    private GameObject currentPreview;
    private int nextIndex=0;

    public int spawnRange = 4;
    private float lastSpawnTime = -999f; // 最後にアイコンを出した時間
    public float spawnCooldown = 1.0f;   // クールタイム（秒）

    // private float topLineY;
    // public float minX = -7.5f; // 画面左端
    // public float maxX =  7.5f; // 画面右端

    public float topLineY = 2f; // 例：TopLineのY座標（またはStageManagerから取得）
    public float minX = -1f;    // 左端X座標（左壁）
    public float maxX = 1f;     // 右端X座標（右壁）

    [Header("右上の次アイコン表示用UI")]
    public Image nextIconImage;  // ← InspectorでImageをアサインする

    private StageManager stageManager;

    [Header("左右マージン設定")]
    public float sideMargin = 0.3f;

    [Header("照準ライン（LineRenderer）")]
    public LineRenderer dropLine;


    public static EtoDropRadar Instance;

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        StartCoroutine(InitAfterDelay());
    }


    
    private bool IsAnyUIPanelOpen()
    {
        return
            (ItemUseConfirmPanel != null && ItemUseConfirmPanel.activeSelf) ||
            (evolutionPanel != null && evolutionPanel.activeSelf) ||
            (summonPanel != null && summonPanel.activeSelf) ||
            (gameOverPanel != null && gameOverPanel.activeSelf) ||
            (backToTitlePanel != null && backToTitlePanel.activeSelf) || // ← OK
            (saveMessagePanel != null && saveMessagePanel.activeSelf);   // ← OK
    }


    void Start()
    {
        if (dropLine != null)
        {
            dropLine.positionCount = 2;
            dropLine.startWidth = 0.05f;
            dropLine.endWidth = 0.05f;
            dropLine.startColor = Color.red;
            dropLine.endColor = Color.red;
            dropLine.material = new Material(Shader.Find("Sprites/Default"));
            dropLine.gameObject.SetActive(false);
        }
    }


    IEnumerator InitAfterDelay()
    {
        yield return null; // 1フレーム待つ

        mainCamera = Camera.main;
        PrepareNextEto();

        stageManager = FindObjectOfType<StageManager>();

        if (stageManager != null)
        {
            minX = stageManager.CurrentStageMinX;
            maxX = stageManager.CurrentStageMaxX;
        }

        GameObject topLine = GameObject.FindWithTag("TopLine");
        if (topLine != null)
        {
            topLineY = topLine.transform.position.y;
        }
        else
        {
            Debug.LogError("TopLineが見つかりません。Tag 'TopLine' を設定してください");
        }
    }


    void Update()
    {
        // 毎フレーム、TopLineのY座標 + マージンを取得
        if (StageManager.Instance != null && StageManager.Instance.topLine != null)
        {
            topLineY = StageManager.Instance.topLine.position.y + 0.3f;
        }
        // マウス位置をワールド座標に変換
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Mathf.Abs(mainCamera.transform.position.z);
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(mousePos);
        mouseWorldPos.z = 0f;

        // ステージマネージャが存在していれば、常に現在値を取得
        if (stageManager != null)
        {
            minX = stageManager.CurrentStageMinX;
            maxX = stageManager.CurrentStageMaxX;
        }

        // 新しいドロップ範囲（マージン考慮）
        float safeMinX = minX + sideMargin;
        float safeMaxX = maxX - sideMargin;

        // 範囲チェック
        // bool isWithinY = mouseWorldPos.y >= topLineY;
        // bool isWithinX = mouseWorldPos.x >= safeMinX && mouseWorldPos.x <= safeMaxX;
        // bool isValidPosition = isWithinX && isWithinY;
        bool isValidPosition = IsValidDropPosition(mouseWorldPos);

        Debug.DrawLine(new Vector3(minX + sideMargin, topLineY, 0), new Vector3(minX + sideMargin, topLineY - 2, 0), Color.red);
        Debug.DrawLine(new Vector3(maxX - sideMargin, topLineY, 0), new Vector3(maxX - sideMargin, topLineY - 2, 0), Color.red);




        // === 長押し中 ===
        if (Input.GetMouseButton(0))
        {
            // 🔒 UIパネルが開いている場合、Preview表示禁止
            if (IsAnyUIPanelOpen())
            {
                if (currentPreview != null) currentPreview.SetActive(false);
                if (dropLine != null) dropLine.gameObject.SetActive(false);
                return;
            }

            // 正常な範囲でのみ生成
            if (IsValidDropPosition(mouseWorldPos))
            {
                if (currentPreview == null && !EventSystem.current.IsPointerOverGameObject())
                {
                    if (Time.time - lastSpawnTime >= spawnCooldown)
                    {
                        currentPreview = Instantiate(etoPrefabs[nextIndex]);
                        SetAlpha(currentPreview, 0.5f); // 半透明化
                    }
                }

                float previewX = Mathf.Clamp(mouseWorldPos.x, minX + sideMargin, maxX - sideMargin);
                float previewY = topLineY + 0.5f;

                if (currentPreview != null)
                {

                    currentPreview.transform.position = new Vector3(previewX, previewY, 0f);
                    currentPreview.SetActive(true);
                }

                // 照準ラインを表示
                if (dropLine != null)
                {
                    dropLine.gameObject.SetActive(true);
                    Vector3 start = new Vector3(previewX, previewY, 0f);
                    Vector3 end = new Vector3(previewX, -5f, 0f);
                    dropLine.SetPosition(0, start);
                    dropLine.SetPosition(1, end);
                }

            }
            else
            {
                // 範囲外では Preview を非表示
                if (currentPreview != null)
                {
                    currentPreview.SetActive(false);
                }
                if (dropLine != null) dropLine.gameObject.SetActive(false);
            }
        }

        // === ドロップ時 ===
        else if (Input.GetMouseButtonUp(0))
        {

            // UIパネルが開いている場合はドロップ処理を中止
            if (IsAnyUIPanelOpen() || Time.time < summonCooldownTime)
            {
                Debug.Log("🛑 UIパネルが開いているためドロップをキャンセル");
                return;
            }

            if (currentPreview != null)
            {
                Destroy(currentPreview);

                if (IsValidDropPosition(mouseWorldPos))
                {
                    float dropX = Mathf.Clamp(mouseWorldPos.x, minX + sideMargin, maxX - sideMargin);
                    float dropY = topLineY + 0.5f;
                    Vector3 dropPos = new Vector3(dropX, dropY, 0f);

                    Instantiate(etoPrefabs[nextIndex], dropPos, Quaternion.identity);

                    lastSpawnTime = Time.time;
                    GameManager.Instance?.AddScore(10);

                    Debug.Log($"📥 アイコンを {dropPos} にドロップしました");
                }

                PrepareNextEto();
            }

            if (dropLine != null) dropLine.gameObject.SetActive(false);
        }
    }


    void PrepareNextEto()
    {
        GameObject forcedPrefab = null;
        bool hasForcedIcon = GameManager.Instance != null &&
                             GameManager.Instance.TryConsumeNextForcedIcon(out forcedPrefab);

        // 🎲 通常ランダム処理：Scene上の最大レベルに応じてドロップ範囲を変化
        int currentMax = EtoIconController.GetMaxEtoLevelInScene();
        Debug.Log($"[DEBUG] 現在のScene上の最大進化レベル: {currentMax}");

        int minLevel;
        int maxLevel;

        // レベル帯に応じて範囲を変化させる（例：3段階に分岐）
        if (currentMax <= 6)
        {
            minLevel = 1;
            maxLevel = 4;
        }
        else if (currentMax <= 15)
        {
            minLevel = Mathf.Max(1, currentMax - 5);
            maxLevel = Mathf.Min(currentMax - 2, etoPrefabs.Length);
        }
        else
        {
            minLevel = Mathf.Max(1, currentMax - 5);
            maxLevel = Mathf.Min(currentMax - 2, etoPrefabs.Length);
        }

        Debug.Log($"[DEBUG] ドロップレベル範囲: {minLevel}〜{maxLevel}");

        if (hasForcedIcon && forcedPrefab != null)
        {
            var iconController = forcedPrefab.GetComponent<EtoIconController>();
            if (iconController == null)
            {
                Debug.LogWarning($"⚠ 強制召喚アイコンに EtoIconController がありません: {forcedPrefab.name}");
            }
            else
            {
                int forcedLevel = iconController.etoLevel;
                if (forcedLevel < minLevel || forcedLevel > maxLevel)
                {
                    Debug.LogWarning($"❌ 召喚アイコン（Lv{forcedLevel}）は現在のドロップ範囲 {minLevel}〜{maxLevel} に含まれていません！");
                }
                else
                {
                    int matchedIndex = FindPrefabIndexByName(forcedPrefab.name);
                    if (matchedIndex < 0)
                    {
                        matchedIndex = FindPrefabIndexByLevel(forcedLevel);
                    }

                    if (matchedIndex >= 0)
                    {
                        nextIndex = matchedIndex;
                        Debug.Log($"🧲 強制召喚アイコンを設定: {forcedPrefab.name} (Lv{forcedLevel})");
                        UpdateNextIconUI(etoPrefabs[nextIndex]);
                        return;
                    }

                    Debug.LogWarning($"⚠ 強制召喚アイコン {forcedPrefab.name} に一致するプレハブが見つかりませんでした。レベル {forcedLevel} から最適な候補を検索します。");
                }
            }
        }


        // 実際のレベルとindexを決定
        int level = Random.Range(minLevel, maxLevel + 1);
        nextIndex = Mathf.Clamp(level - 1, 0, etoPrefabs.Length - 1);
        Debug.Log($"🎲 通常ランダム選出 → index={nextIndex}, level={level}");

        // UI更新
        UpdateNextIconUI(etoPrefabs[nextIndex]);
    }



    void SetAlpha(GameObject obj, float alpha)
    {
        SpriteRenderer[] renderers = obj.GetComponentsInChildren<SpriteRenderer>();
        foreach (var sr in renderers)
        {
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }
    }

    void UpdateNextIconUI(GameObject prefab)
    {
        Debug.Log("🧪 UpdateNextIconUI() が呼ばれました"); // ★ 最初に入れる
        if (nextIconImage == null || prefab == null)
        {
            Debug.LogWarning("❌ nextIconImage または prefab が null");
            return;
        }

        var sr = prefab.GetComponentInChildren<SpriteRenderer>();
        if (sr == null)
        {
            Debug.LogWarning($"⚠ SpriteRenderer not found in prefab: {prefab.name}");
            return;
        }

        nextIconImage.sprite = sr.sprite;
        nextIconImage.color = Color.white;
        nextIconImage.gameObject.SetActive(true);
        Debug.Log($"🖼️ NextIconImage に {sr.sprite.name} を設定しました");
    }


    bool IsValidDropPosition(Vector3 worldPos)
    {
        float safeMinX = minX + sideMargin;
        float safeMaxX = maxX - sideMargin;

        bool isWithinX = worldPos.x >= safeMinX && worldPos.x <= safeMaxX;
        //bool isWithinY = worldPos.y >= topLineY;
        return isWithinX; //&& isWithinY;
    }

    int FindPrefabIndexByName(string prefabName)
    {
        for (int i = 0; i < etoPrefabs.Length; i++)
        {
            if (etoPrefabs[i] != null && etoPrefabs[i].name == prefabName)
            {
                return i;
            }
        }

        return -1;
    }

    int FindPrefabIndexByLevel(int level)
    {
        for (int i = 0; i < etoPrefabs.Length; i++)
        {
            var controller = etoPrefabs[i]?.GetComponent<EtoIconController>();
            if (controller != null && controller.etoLevel == level)
            {
                return i;
            }
        }

        return -1;
    }

    public void SetNextEtoIndex(int index)
    {
        nextIndex = index;
        Debug.Log($"🎯 次に落とすアイコンのインデックスを {index} に設定しました");
        UpdateNextIconUI(etoPrefabs[nextIndex]); // Next表示更新（必要なら）
    }


}
