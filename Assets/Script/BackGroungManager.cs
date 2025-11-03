using UnityEngine;

public class BackgroundManager : MonoBehaviour
{
    public static BackgroundManager Instance;

    public Sprite selectedBackground;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // シーンを跨いでも破棄されない
        }
        else
        {
            Destroy(gameObject); // 複数インスタンス防止
        }
    }

    public void SetBackground(Sprite bg)
    {
        selectedBackground = bg;
    }

    public Sprite GetBackground()
    {
        return selectedBackground;
    }
}
