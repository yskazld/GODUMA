using UnityEngine;

public class EtoSpawner : MonoBehaviour
{
    public GameObject[] etoPrefabs;
    public float minSpawnY = 2.0f;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 10f; // Camera からの距離（Camera.main.transform.position.z の絶対値）

            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
            worldPos.z = 0;

            if (worldPos.y >= minSpawnY)
            {
                SpawnRandomEto(worldPos);
            }
        }
    }

    void SpawnRandomEto(Vector3 position)
    {
        if (etoPrefabs.Length == 0) return;

        int index = Random.Range(0, etoPrefabs.Length);
        Instantiate(etoPrefabs[index], position, Quaternion.identity);
    }
}
