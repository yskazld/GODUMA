using UnityEngine;
using UnityEngine.UI;

public class EtoNextPreview : MonoBehaviour
{
    public Image nextEtoImage; // UIのImage（右上に表示）
    public GameObject[] etoPrefabs; // 干支Prefab（ここからSpriteを取得）

    private int currentIndex;

    void Start()
    {
        currentIndex = Random.Range(0, etoPrefabs.Length);
        SetNextEto(currentIndex);
    }

    public void SetNextEto(int index)
    {
        if (index >= 0 && index < etoPrefabs.Length)
        {
            var spriteRenderer = etoPrefabs[index].GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && nextEtoImage != null)
            {
                nextEtoImage.sprite = spriteRenderer.sprite;
                nextEtoImage.color = new Color(0, 0, 0, 0.5f); // シルエット（黒＋半透明）
            }
        }
    }

    public int GetCurrentIndex()
    {
        return currentIndex;
    }
}
