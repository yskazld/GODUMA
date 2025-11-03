using UnityEngine;

public class StageManager : MonoBehaviour
{
    [Header("ステージ構成要素")]
    public Transform leftWall;
    public Transform rightWall;
    public Transform topLine;
    public Transform bottom; // ← Bottomを追加！

    [Header("X軸（壁）拡張設定")]
    public float baseWidth = 2f;
    public float sizeIncrementPerLevel = 0.38f;

    [Header("Y軸（TopLine）拡張設定")]
    public float topLineStartY = -2.4f;
    public float topLineMaxY = 5f;
    public float topLineIncrementPerLevel = 0.296f;

    [Header("カメラ拡大設定")]
    public float baseCameraSize = 2.5f;
    public float cameraZoomPerLevel = 0.2f;

    [Header("拡張開始レベル")]
    public int expandStartLevel = 5;

    private int currentMaxLevel = 1;
    private bool isHardMode = false;

    public float CurrentStageMinX => leftWall.position.x;
    public float CurrentStageMaxX => rightWall.position.x;

    public static StageManager Instance;

    private StageMode selectedMode;
    private bool allowStageExpansion = false;

    [Header("Normalモード用 拡張設定")]
    public float normalSizeIncrementPerLevel = 0.25f;
    public float normalTopLineIncrementPerLevel = 0.22f;

    [Header("ステージ拡張の最大幅")]
    public float maxStageWidth = 15.5f;  // Easyモードと同じにする

    public enum StageMode
    {
        Easy = 0,    // 拡張なし
        Normal = 1,  // 中間 → 拡張
        Hard = 2     // 最小 → 拡張
    }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        int saved = PlayerPrefs.GetInt("Mode", -1);
        Debug.Log($"🎮 PlayerPrefsから読み込んだモード: {saved}");

        selectedMode = (StageMode)saved;

        Debug.Log($"[Start] StageManager 起動");

        currentMaxLevel = 1;

        selectedMode = (StageMode)PlayerPrefs.GetInt("Mode", 0);

        // ★ ここでHardモードなら初期サイズを上書き
        if (selectedMode == StageMode.Hard)
        {
            baseWidth = 4.0f;
            topLineStartY = -0.5f;
            Debug.Log($"🛠 神領域モード初期設定 → baseWidth={baseWidth}, topLineStartY={topLineStartY}");
        }
        
        switch (selectedMode)
        {
            case StageMode.Easy:
                allowStageExpansion = false;
                InitializeStageFixed(); // 拡張なし
                break;

            case StageMode.Normal:
                allowStageExpansion = true;
                InitializeStageMedium(); // 中間サイズ → 拡張あり
                break;

            case StageMode.Hard:
                allowStageExpansion = true;
                InitializeStageSmall(); // 最小サイズ → 拡張あり
                break;
        }
    }



    void Update()
    {
        if (!allowStageExpansion) return;

        int maxLevel = GetMaxEtoLevelInScene();
        if (maxLevel < expandStartLevel) return;
        if (maxLevel <= currentMaxLevel) return;

        switch (selectedMode)
        {
            case StageMode.Normal:
                ResizeStageForNormalMode(maxLevel);
                break;
            case StageMode.Hard:
                ResizeStage(maxLevel);
                break;
        }

        currentMaxLevel = maxLevel;
    }


    public void ResetStage()
    {
        Debug.Log("🔄 StageManager.ResetStage() 実行");

        currentMaxLevel = 1;
        InitializeStageFixed(); // 初期化時もHardMode関係なく固定スタート
    }


    float GetAspectRatioScale()
    {
        float aspect = (float)Screen.width / Screen.height;

        if (aspect >= 2.0f) return 1.2f;   // ワイド → ステージ拡張
        else if (aspect >= 1.6f) return 1.0f; // 通常 → 基準
        else return 0.85f;                // 縦長 → ステージ縮小
    }

    void InitializeStageFixed()
    {
        float scale = GetAspectRatioScale();
        float intendedStageHeight = 8f * scale;  // 目安の高さ

        float margin = 2.0f;  // 画面端からのマージン

        Camera cam = Camera.main;
        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;
        float screenLeft = cam.transform.position.x - halfWidth;
        float screenRight = cam.transform.position.x + halfWidth;
        float screenTopY = cam.transform.position.y + halfHeight;

        // Bottomの位置（固定Y）
        float bottomY = -3.35f;
        bottom.position = new Vector3(0f, bottomY, 0f);

        // TopYの候補
        float topY = bottomY + intendedStageHeight;

        // カメラ上端を超えないよう制限
        if (topY > screenTopY - 0.6f)  // 0.2f は余白
        {
            topY = screenTopY - 0.6f;
        }

        // 最終的なステージ高さ・中央Y
        float stageHeight = topY - bottomY;
        float centerY = bottomY + stageHeight / 2f;

        // 左右壁の位置
        float leftX = screenLeft + margin;
        float rightX = screenRight - margin;
        float stageWidth = rightX - leftX;

        // 壁の配置とスケール
        leftWall.position = new Vector3(leftX, centerY, 0f);
        rightWall.position = new Vector3(rightX, centerY, 0f);
        // leftWall.localScale = new Vector3(0.1f, stageHeight, 1f);
        // rightWall.localScale = new Vector3(0.1f, stageHeight, 1f);

        // 横ラインの配置とスケール
        topLine.position = new Vector3(0f, topY, 0f);
        // topLine.localScale = new Vector3(stageWidth, 0.05f, 1f);
        // bottom.localScale = new Vector3(stageWidth, 0.15f, 1f);

        float wallThickness = 0.25f;      // ← 横の壁の「太さ」
        float lineThickness = 0.3f;       // ← Top/Bottom の「高さ（太さ）」

        leftWall.localScale = new Vector3(wallThickness, stageHeight, 1f);
        rightWall.localScale = new Vector3(wallThickness, stageHeight, 1f);

        topLine.localScale = new Vector3(stageWidth+0.3f, lineThickness/2, 1f);
        bottom.localScale = new Vector3(stageWidth+0.3f, lineThickness, 1f);

        Debug.Log($"📏 Easy拡張（TopLine調整済）→ 幅:{stageWidth:F2}, 高さ:{stageHeight:F2}, TopY:{topY:F2}");
    }


    // void InitializeStageFixed()
    // {
    //     float scale = GetAspectRatioScale();
    //     float stageWidth = 15.5f * scale;
    //     float stageHeight = 8f * scale;

    //     // Bottom位置そのまま
    //     bottom.position = new Vector3(0f, -3.35f, 0f);
    //     bottom.localScale = new Vector3(stageWidth, 0.15f, 1f);

    //     // centerYとTopYを再計算
    //     float centerY = bottom.position.y + stageHeight / 2f;
    //     float topY = bottom.position.y + stageHeight;

    //     // TopLineの位置・スケールを修正
    //     topLine.position = new Vector3(0f, topY, 0f);
    //     topLine.localScale = new Vector3(stageWidth, 0.05f, 1f);

    //     leftWall.position = new Vector3(-stageWidth / 2f, centerY, 0f);
    //     rightWall.position = new Vector3(stageWidth / 2f, centerY, 0f);
    //     leftWall.localScale = new Vector3(0.1f, stageHeight, 1f);
    //     rightWall.localScale = new Vector3(0.1f, stageHeight, 1f);

    //     Debug.Log($"📏 Easy拡張（TopLine調整済み）→ 幅:{stageWidth:F2}, 高さ:{stageHeight:F2}, TopY:{topY:F2}");
    // }



    int GetMaxEtoLevelInScene()
    {
        int max = 1;
        EtoIconController[] icons = FindObjectsOfType<EtoIconController>();
        foreach (var icon in icons)
        {
            if (icon.etoLevel > max)
                max = icon.etoLevel;
        }
        return max;
    }

    // void ResizeStage(int evolvedLevel)
    // {
    //     int deltaLevel = Mathf.Max(0, evolvedLevel - expandStartLevel + 1);

    //     float extraWidth = sizeIncrementPerLevel * deltaLevel;
    //     float stageWidth = baseWidth + extraWidth * 2f;

    //     float extraTopY = deltaLevel * topLineIncrementPerLevel;
    //     float topY = Mathf.Min(topLineStartY + extraTopY, topLineMaxY);
    //     float stageHeight = topY - bottom.position.y;

    //     Vector3 horizontalScale = topLine.localScale;
    //     horizontalScale.x = stageWidth;
    //     topLine.localScale = horizontalScale;
    //     bottom.localScale = horizontalScale;

    //     Vector3 verticalScale = leftWall.localScale;
    //     verticalScale.y = stageHeight;
    //     leftWall.localScale = verticalScale;
    //     rightWall.localScale = verticalScale;

    //     float centerY = bottom.position.y + stageHeight / 2f;
    //     leftWall.position = new Vector3(-stageWidth / 2f, centerY, 0f);
    //     rightWall.position = new Vector3(stageWidth / 2f, centerY, 0f);
    //     topLine.position = new Vector3(0f, topY, 0f);

    //     Debug.Log($"📏 ステージ拡大: Lv{evolvedLevel} → 横幅:{stageWidth}, 高さ:{stageHeight}, TopY:{topY:F2}");
    // }

    

    [Header("Normalモード用 初期拡張レベル")]
    public int normalInitialExpandLevel = 2;

    void InitializeStageMedium()
    {
        int levelOffset = Mathf.Max(0, normalInitialExpandLevel);

        float normalBaseWidth = 6.5f;
        float normalTopStartY = 1.5f;

        float extraTopY = levelOffset * normalTopLineIncrementPerLevel;
        float topY = normalTopStartY + extraTopY;
        float stageHeight = topY - bottom.position.y;

        float extraWidth = normalSizeIncrementPerLevel * levelOffset;
        float stageWidth = normalBaseWidth + extraWidth * 2f;
        float centerY = bottom.position.y + stageHeight / 2f;

        topLine.position = new Vector3(0f, topY, 0f);
        topLine.localScale = new Vector3(stageWidth, 0.05f, 1f);
        bottom.localScale = new Vector3(stageWidth, 0.15f, 1f);

        leftWall.position = new Vector3(-stageWidth / 2f, centerY, 0f);
        rightWall.position = new Vector3(stageWidth / 2f, centerY, 0f);

        Vector3 wallScale = new Vector3(0.1f, stageHeight, 1f);
        leftWall.localScale = wallScale;
        rightWall.localScale = wallScale;

        Debug.Log($"📏 [Normal Init] 中間サイズ開始（幅:{stageWidth:F2}, 高さ:{stageHeight:F2}）");
    }



    void ResizeStageForNormalMode(int evolvedLevel)
    {
        // 初期化時と同じLevelOffsetをスキップする
        int deltaLevel = Mathf.Max(0, evolvedLevel - normalInitialExpandLevel);
        if (deltaLevel == 0) return;  // ← 初期と同じなら拡張しない

        float normalBaseWidth = 6.5f;
        float normalTopStartY = 1.5f;

        float extraWidth = normalSizeIncrementPerLevel * deltaLevel;
        float stageWidth = Mathf.Min(normalBaseWidth + extraWidth * 2f, maxStageWidth); // ← 修正！

        float extraTopY = deltaLevel * normalTopLineIncrementPerLevel;
        float topY = Mathf.Min(normalTopStartY + extraTopY, topLineMaxY);
        float stageHeight = topY - bottom.position.y;
        float centerY = bottom.position.y + stageHeight / 2f;

        topLine.localScale = new Vector3(stageWidth, topLine.localScale.y, 1f);
        bottom.localScale = new Vector3(stageWidth, bottom.localScale.y, 1f);

        Vector3 wallScale = new Vector3(leftWall.localScale.x, stageHeight, 1f);
        leftWall.localScale = wallScale;
        rightWall.localScale = wallScale;

        leftWall.position = new Vector3(-stageWidth / 2f, centerY, 0f);
        rightWall.position = new Vector3(stageWidth / 2f, centerY, 0f);
        topLine.position = new Vector3(0f, topY, 0f);

        Debug.Log($"📏 [Normal Resize] Lv{evolvedLevel} → 横幅:{stageWidth:F2}, 高さ:{stageHeight:F2}, TopY:{topY:F2}");
    }



    void InitializeStageSmall()
    {
        // 変更前：baseWidth = 2f を使っていたが、直接ここでサイズ指定も可能
        float stageWidth = 4.0f; // ← 初期横幅を少し拡大（例：3.0）
        float topY = -0.5f;      // ← topLineStartY = -2.4f より高めに設定

        float stageHeight = topY - bottom.position.y;
        float centerY = bottom.position.y + stageHeight / 2f;

        topLine.position = new Vector3(0f, topY, 0f);
        topLine.localScale = new Vector3(stageWidth, 0.05f, 1f);
        bottom.localScale = new Vector3(stageWidth, 0.15f, 1f);

        leftWall.position = new Vector3(-stageWidth / 2f, centerY, 0f);
        rightWall.position = new Vector3(stageWidth / 2f, centerY, 0f);

        Vector3 wallScale = new Vector3(0.1f, stageHeight, 1f);
        leftWall.localScale = wallScale;
        rightWall.localScale = wallScale;

        Debug.Log($"📏 [Hard Init] 神領域モード初期サイズ → 幅:{stageWidth:F2}, 高さ:{stageHeight:F2}, TopY:{topY:F2}");
    }


    // void InitializeStageSmall()
    // {
    //     float stageHeight = topLineStartY - bottom.position.y;
    //     float stageWidth = baseWidth;
    //     float centerY = bottom.position.y + stageHeight / 2f;

    //     topLine.position = new Vector3(0f, topLineStartY, 0f);
    //     topLine.localScale = new Vector3(stageWidth, 0.05f, 1f);
    //     bottom.localScale = new Vector3(stageWidth, 0.15f, 1f);

    //     leftWall.position = new Vector3(-stageWidth / 2f, centerY, 0f);
    //     rightWall.position = new Vector3(stageWidth / 2f, centerY, 0f);

    //     Vector3 wallScale = new Vector3(0.1f, stageHeight, 1f);
    //     leftWall.localScale = wallScale;
    //     rightWall.localScale = wallScale;

    //     Debug.Log("📏 [Hard Init] 最小サイズから開始 → 拡張あり");
    // }


    void ResizeStage(int evolvedLevel)
    {
        int deltaLevel = Mathf.Max(0, evolvedLevel - expandStartLevel + 1);

        // 横幅（X方向）
        float extraWidth = sizeIncrementPerLevel * deltaLevel;
        float stageWidth = Mathf.Min(baseWidth + extraWidth * 2f, maxStageWidth); // ← 修正！


        // 高さ（Y方向）
        float extraTopY = deltaLevel * topLineIncrementPerLevel;
        float topY = Mathf.Min(topLineStartY + extraTopY, topLineMaxY);
        float stageHeight = topY - bottom.position.y;

        // 横線の長さ（Top/Bottom）
        topLine.localScale = new Vector3(stageWidth, topLine.localScale.y, 1f);
        bottom.localScale = new Vector3(stageWidth, bottom.localScale.y, 1f);

        // 縦線の長さ（Left/Right）
        Vector3 vScale = new Vector3(leftWall.localScale.x, stageHeight, 1f);
        leftWall.localScale = vScale;
        rightWall.localScale = vScale;

        float centerY = bottom.position.y + stageHeight / 2f;
        leftWall.position = new Vector3(-stageWidth / 2f, centerY, 0f);
        rightWall.position = new Vector3(stageWidth / 2f, centerY, 0f);
        topLine.position = new Vector3(0f, topY, 0f);

        Debug.Log($"📏 [ResizeStage] Hardモード：Lv{evolvedLevel} ステージ拡大 → 横幅:{stageWidth}, 高さ:{stageHeight}, TopY:{topY:F2}");
    }

    void AdjustCamera(int level)
    {
        // カメラ拡大処理（未使用ならコメントアウトのままでOK）
    }

    void UpdateCameraPosition()
    {
        float centerX = (leftWall.position.x + rightWall.position.x) / 2f;
        float centerY = (topLine.position.y + bottom.position.y) / 2f;
        Camera.main.transform.position = new Vector3(centerX, centerY, Camera.main.transform.position.z);
    }
    public int GetCurrentMaxLevel()
    {
        return currentMaxLevel;
    }

}
