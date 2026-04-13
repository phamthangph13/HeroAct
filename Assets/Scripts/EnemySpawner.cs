using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Spawns enemies at child spawn points in the current scene.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    public event Action<GameObject> EnemySpawned;

    [Header("Enemy")]
    public GameObject enemyPrefab;

    [Header("Spawn Settings")]
    public int maxEnemies = 10;
    public float spawnInterval = 3f;
    public bool spawnOnStart = true;
    public bool continuousSpawn = true;

    [Header("Spawn Points")]
    public Transform[] spawnPoints;

    private int currentEnemyCount;
    private Coroutine spawnLoopCoroutine;

    public GameObject LastSpawnedEnemy { get; private set; }

    private void Start()
    {
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
            Debug.LogWarning("EnemySpawner: No spawn points assigned.");
            return;
        }

        if (enemyPrefab == null)
        {
            Debug.LogWarning("EnemySpawner: Missing enemy prefab.");
            return;
        }

        if (spawnOnStart)
        {
            SpawnAllAtOnce();
        }

        if (continuousSpawn)
        {
            spawnLoopCoroutine = StartCoroutine(SpawnLoop());
        }
    }

    public void SpawnAllAtOnce()
    {
        foreach (Transform point in spawnPoints)
        {
            if (currentEnemyCount >= maxEnemies)
            {
                break;
            }

            SpawnEnemy(point.position);
        }
    }

    public void SpawnRandom()
    {
        if (currentEnemyCount >= maxEnemies)
        {
            return;
        }

        int randomIndex = UnityEngine.Random.Range(0, spawnPoints.Length);
        SpawnEnemy(spawnPoints[randomIndex].position);
    }

    public void StopSpawning()
    {
        continuousSpawn = false;

        if (spawnLoopCoroutine != null)
        {
            StopCoroutine(spawnLoopCoroutine);
            spawnLoopCoroutine = null;
        }
    }

    public void OnEnemyDeath()
    {
        currentEnemyCount = Mathf.Max(0, currentEnemyCount - 1);

        if (currentEnemyCount == 0)
        {
            LastSpawnedEnemy = null;
        }
    }

    private void SpawnEnemy(Vector3 position)
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning("EnemySpawner: Missing enemy prefab.");
            return;
        }

        GameObject enemy = Instantiate(enemyPrefab, position, Quaternion.identity);
        currentEnemyCount++;
        LastSpawnedEnemy = enemy;

        EnemyDeathNotifier notifier = enemy.AddComponent<EnemyDeathNotifier>();
        notifier.spawner = this;

        EnemySpawned?.Invoke(enemy);
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
/// Notifies the source spawner when the spawned enemy is destroyed.
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
