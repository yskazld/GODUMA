using UnityEngine;

public class DeleteIconItem : MonoBehaviour
{
    private bool isSelecting = false;

    void Update()
    {
        if (!isSelecting) return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D hit = Physics2D.OverlapPoint(mousePos);

            if (hit != null && hit.CompareTag("Eto"))
            {
                Destroy(hit.gameObject);
                isSelecting = false;
                Debug.Log("🐾 アイコン削除完了");
            }
        }
    }

    public void ActivateDeleteMode()
    {
        isSelecting = true;
        Debug.Log("🧼 アイコン削除モード開始");
    }
}
