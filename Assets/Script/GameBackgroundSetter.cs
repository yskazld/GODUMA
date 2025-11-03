using UnityEngine;
using UnityEngine.UI;

public class GameBackgroundSetter : MonoBehaviour
{
    public Image backgroundImage; // UIの背景Image（またはSpriteRendererでも可）

    void Start()
    {
        if (BackgroundManager.Instance != null && BackgroundManager.Instance.GetBackground() != null)
        {
            backgroundImage.sprite = BackgroundManager.Instance.GetBackground();
        }
    }
}
