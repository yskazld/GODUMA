using UnityEngine;
using UnityEngine.UI;

public class BackgroundSelector : MonoBehaviour
{
    public Sprite[] backgroundOptions; // 背景候補（インスペクタに設定）
    public Image previewImage; // プレビュー用Image

    private int currentIndex = 0;

    void Start()
    {
        UpdatePreview();
    }

    public void NextBackground()
    {
        if (backgroundOptions == null || backgroundOptions.Length == 0)
        {
            Debug.LogWarning("BackgroundSelector: 背景候補が設定されていません。インスペクタで Sprite を指定してください。", this);
            return;
        }

        currentIndex = (currentIndex + 1) % backgroundOptions.Length;
        UpdatePreview();
    }

    public void ApplyBackground()
    {
        if (!TryUpdatePreviewIndex(requirePreviewImage: false))
        {
            return;
        }

        if (BackgroundManager.Instance == null)
        {
            Debug.LogWarning("BackgroundSelector: BackgroundManager.Instance が見つかりませんでした。先に BackgroundManager を初期化してください。", this);
            return;
        }

        BackgroundManager.Instance.SetBackground(backgroundOptions[currentIndex]);
    }

    private void UpdatePreview()
    {
        if (!TryUpdatePreviewIndex(requirePreviewImage: true))
        {
            return;
        }

        previewImage.sprite = backgroundOptions[currentIndex];
    }

    private bool TryUpdatePreviewIndex(bool requirePreviewImage)
    {
        if (backgroundOptions == null || backgroundOptions.Length == 0)
        {
            Debug.LogWarning("BackgroundSelector: 背景候補が設定されていません。インスペクタで Sprite を指定してください。", this);
            return false;
        }

        if (requirePreviewImage && previewImage == null)
        {
            Debug.LogWarning("BackgroundSelector: プレビュー用 Image が設定されていません。", this);
            return false;
        }

        if (currentIndex >= backgroundOptions.Length)
        {
            currentIndex = backgroundOptions.Length - 1;
        }

        if (currentIndex < 0)
        {
            currentIndex = 0;
        }

        return !requirePreviewImage || previewImage != null;
    }
}
