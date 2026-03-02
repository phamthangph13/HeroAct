using UnityEngine;
using System.Collections;

/// <summary>
/// Spawn quái vật tại các vị trí chỉ định trên map.
/// Cách dùng:
/// 1. Tạo Empty GameObject → đặt tên "EnemySpawner"
/// 2. Gắn script này
/// 3. Kéo Enemy Prefab vào
/// 4. Tạo các Empty child objects làm spawn points → đặt ở các vị trí mong muốn
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy")]
    public GameObject enemyPrefab; // Kéo prefab zombie vào đây

    [Header("Spawn Settings")]
    public int maxEnemies = 10;        // Số quái tối đa cùng lúc
    public float spawnInterval = 3f;   // Giây giữa mỗi lần spawn
    public bool spawnOnStart = true;   // Spawn ngay khi bắt đầu
    public bool continuousSpawn = true; // Tiếp tục spawn khi quái bị giết

    [Header("Spawn Points (tạo Empty children)")]
    public Transform[] spawnPoints;    // Kéo các spawn point vào đây

    private int currentEnemyCount = 0;

    private void Start()
    {
        // Tự tìm spawn points nếu chưa gán
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            spawnPoints = new Transform[transform.childCount];
            for (int i = 0; i < transform.childCount; i++)
            {
                spawnPoints[i] = transform.GetChild(i);
            }
        }

        if (spawnPoints.Length == 0)
        {
            Debug.LogWarning("EnemySpawner: Không có spawn points! Tạo Empty children làm vị trí spawn.");
            return;
        }

        if (enemyPrefab == null)
        {
            Debug.LogWarning("EnemySpawner: Chưa gán Enemy Prefab!");
            return;
        }

        if (spawnOnStart)
        {
            SpawnAllAtOnce();
        }

        if (continuousSpawn)
        {
            StartCoroutine(SpawnLoop());
        }
    }

    /// <summary>
    /// Spawn quái tại tất cả spawn points cùng lúc
    /// </summary>
    public void SpawnAllAtOnce()
    {
        foreach (Transform point in spawnPoints)
        {
            if (currentEnemyCount >= maxEnemies) break;
            SpawnEnemy(point.position);
        }
    }

    /// <summary>
    /// Spawn 1 quái tại vị trí ngẫu nhiên
    /// </summary>
    public void SpawnRandom()
    {
        if (currentEnemyCount >= maxEnemies) return;

        int randomIndex = Random.Range(0, spawnPoints.Length);
        SpawnEnemy(spawnPoints[randomIndex].position);
    }

    private void SpawnEnemy(Vector3 position)
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning("EnemySpawner: Enemy Prefab bị null! Hãy kéo prefab từ Project (không phải object trong scene).");
            return;
        }

        GameObject enemy = Instantiate(enemyPrefab, position, Quaternion.identity);
        currentEnemyCount++;

        // Khi quái bị hủy → giảm count
        EnemyDeathNotifier notifier = enemy.AddComponent<EnemyDeathNotifier>();
        notifier.spawner = this;
    }

    public void OnEnemyDeath()
    {
        currentEnemyCount--;
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (currentEnemyCount < maxEnemies)
            {
                SpawnRandom();
            }
        }
    }

    // Vẽ spawn points trong Scene view
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        foreach (Transform child in transform)
        {
            Gizmos.DrawWireSphere(child.position, 0.3f);
            Gizmos.DrawIcon(child.position, "d_P4_DeletedLocal", true);
        }
    }
}

/// <summary>
/// Tự động thông báo spawner khi enemy bị Destroy
/// </summary>
public class EnemyDeathNotifier : MonoBehaviour
{
    [HideInInspector] public EnemySpawner spawner;

    private void OnDestroy()
    {
        if (spawner != null)
        {
            spawner.OnEnemyDeath();
        }
    }
}
