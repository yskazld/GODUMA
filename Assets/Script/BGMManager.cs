using UnityEngine;

public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance;

    private AudioSource audioSource;

    public AudioClip titleBGM;
    public AudioClip gameBGM;

    public AudioClip[] bgmClips;  // 複数のBGM
    public string[] bgmNames;     // BGMの名前（UI表示用）

    private int currentIndex = 0; // 現在の選択インデックス


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // シーンをまたいでもBGMを維持
            audioSource = GetComponent<AudioSource>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayTitleBGM()
    {
        PlayBGM(titleBGM);
    }

    public void PlayGameBGM()
    {
        PlayBGM(gameBGM);
    }

    private void PlayBGM(AudioClip clip)
    {
        if (audioSource.clip == clip) return;
        audioSource.clip = clip;
        audioSource.loop = true;
        audioSource.Play();
    }

    public void StopBGM()
    {
        audioSource.Stop();
    }

    public void PlaySelectedBGM(int index)
    {
        if (index < 0 || index >= bgmClips.Length) return;

        audioSource.clip = bgmClips[index];
        audioSource.loop = true;
        audioSource.Play();
    }

}
