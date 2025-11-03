using UnityEngine;
using UnityEngine.UI;

public class DifficultySelector : MonoBehaviour
{
    public Toggle easyToggle;
    public Toggle normalToggle;
    public Toggle hardToggle;

    void Start()
    {
        int savedMode = PlayerPrefs.GetInt("Mode", 0);

        easyToggle.isOn = savedMode == 0;
        normalToggle.isOn = savedMode == 1;
        hardToggle.isOn = savedMode == 2;

        easyToggle.onValueChanged.AddListener((on) => { if (on) SetMode(0); });
        normalToggle.onValueChanged.AddListener((on) => { if (on) SetMode(1); });
        hardToggle.onValueChanged.AddListener((on) => { if (on) SetMode(2); });
    }

    void SetMode(int mode)
    {
        PlayerPrefs.SetInt("Mode", mode);
        PlayerPrefs.Save();
        Debug.Log($"🎮 難易度設定: {(StageMode)mode}");
    }

    enum StageMode
    {
        Easy = 0,
        Normal = 1,
        Hard = 2
    }
}

