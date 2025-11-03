using UnityEngine;
using UnityEngine.UI;

public class GameOverPanelResizer : MonoBehaviour
{
    public RectTransform gameOverPanel;
    public RectTransform[] buttons;  // 順番に4つ（左上、右上、左下、右下）


    public RectTransform gameOverImage;
    public RectTransform haneImage;

    [Header("ボタン間の余白")]
    public float paddingX = 40f;
    public float paddingY = 30f;

    void OnEnable()
    {
        ResizeButtons();
    }

    void ResizeButtons()
    {
        if (buttons.Length != 4 || gameOverPanel == null) return;

        float panelWidth = gameOverPanel.rect.width;
        float panelHeight = gameOverPanel.rect.height;

        float buttonWidth = (panelWidth - paddingX * 3f) / 2f;
        float buttonHeight = (panelHeight - paddingY * 3f) / 2f;

        // 配置：左上、右上、左下、右下
        Vector2[] anchors = new Vector2[]
        {
            new Vector2(-0.5f, 0.5f), // 左上
            new Vector2(0.5f, 0.5f),  // 右上
            new Vector2(-0.5f, -0.5f),// 左下
            new Vector2(0.5f, -0.5f)  // 右下
        };

        for (int i = 0; i < buttons.Length; i++)
        {
            RectTransform btn = buttons[i];
            btn.sizeDelta = new Vector2(buttonWidth, buttonHeight);

            float posX = anchors[i].x * (panelWidth - buttonWidth - paddingX);
            float posY = anchors[i].y * (panelHeight - buttonHeight - paddingY);

            btn.anchoredPosition = new Vector2(posX, posY);
        }
        // GameOverImage（タイトル）調整
        if (gameOverImage != null)
        {
            float titleWidth = panelWidth * 0.6f;
            float titleHeight = panelHeight * 0.15f;

            gameOverImage.sizeDelta = new Vector2(titleWidth, titleHeight);
            gameOverImage.anchoredPosition = new Vector2(0f, panelHeight / 2f - titleHeight * 0.7f); // 上寄りに配置
        }

        // HaneImage（羽）調整
        if (haneImage != null)
        {
            float haneSize = panelWidth * 0.25f;
            haneImage.sizeDelta = new Vector2(haneSize, haneSize * 0.4f); // 羽は細長く
            haneImage.anchoredPosition = new Vector2(panelWidth * 0.2f, panelHeight / 2f - haneSize * 0.3f);
            haneImage.localEulerAngles = new Vector3(0, 0, 20f); // 斜めに傾ける
        }

        Debug.Log($"🧩 GameOverボタンリサイズ：{buttonWidth}×{buttonHeight}");
    }

    
}

