using UnityEngine;

public class BossFinalVictoryTrigger : MonoBehaviour
{
    [SerializeField] private EnemySpawner bossSpawner;
    [SerializeField] private BossVictoryPanelController victoryPanel;

    private EnemyHealth trackedBossHealth;
    private bool hasTriggeredWin;

    private void Reset()
    {
        if (bossSpawner == null)
        {
            bossSpawner = GetComponent<EnemySpawner>();
        }
    }

    private void OnEnable()
    {
        if (bossSpawner == null)
        {
            bossSpawner = GetComponent<EnemySpawner>();
        }

        if (bossSpawner == null)
        {
            return;
        }

        bossSpawner.EnemySpawned -= HandleEnemySpawned;
        bossSpawner.EnemySpawned += HandleEnemySpawned;
    }

    private void Start()
    {
        if (bossSpawner != null && bossSpawner.LastSpawnedEnemy != null)
        {
            AttachToBoss(bossSpawner.LastSpawnedEnemy);
        }
    }

    private void OnDisable()
    {
        if (bossSpawner != null)
        {
            bossSpawner.EnemySpawned -= HandleEnemySpawned;
        }

        DetachTrackedBoss();
    }

    public void Configure(EnemySpawner spawnerReference, BossVictoryPanelController panelReference)
    {
        bossSpawner = spawnerReference;
        victoryPanel = panelReference;
    }

    private void HandleEnemySpawned(GameObject enemy)
    {
        if (hasTriggeredWin)
        {
            return;
        }

        AttachToBoss(enemy);
    }

    private void AttachToBoss(GameObject enemy)
    {
        if (enemy == null)
        {
            return;
        }

        EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
        if (enemyHealth == null || enemyHealth == trackedBossHealth)
        {
            return;
        }

        DetachTrackedBoss();
        trackedBossHealth = enemyHealth;
        trackedBossHealth.Died += HandleBossDied;
    }

    private void DetachTrackedBoss()
    {
        if (trackedBossHealth == null)
        {
            return;
        }

        trackedBossHealth.Died -= HandleBossDied;
        trackedBossHealth = null;
    }

    private void HandleBossDied(EnemyHealth bossHealth)
    {
        if (hasTriggeredWin)
        {
            return;
        }

        hasTriggeredWin = true;
        DetachTrackedBoss();

        if (bossSpawner != null)
        {
            bossSpawner.StopSpawning();
        }

        if (victoryPanel != null)
        {
            victoryPanel.Show();
        }
        else
        {
            Debug.LogWarning("BossFinalVictoryTrigger: Missing victory panel reference.");
        }
    }
}
