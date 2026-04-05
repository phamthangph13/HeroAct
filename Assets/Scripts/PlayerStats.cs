using System;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }
    public static event Action<PlayerStats> InstanceChanged;

    public event Action StatsChanged;

    [Header("Base Stats")]
    public float baseDamage = 10f;
    public float baseMaxHP = 100f;
    public float baseArmor = 5f;
    public float baseCritRate = 0f;
    public float baseCritDamage = 10f;
    public float baseGoldDropRate = 0f;
    public float baseLifesteal = 0f;

    [Header("Resources")]
    public int statPoints = 15;
    public int totalCoins = 0;

    [Header("Upgrade Levels")]
    public int damageLevel = 0;
    public int hpLevel = 0;
    public int armorLevel = 0;
    public int critRateLevel = 0;
    public int critDamageLevel = 0;
    public int goldDropLevel = 0;
    public int lifestealLevel = 0;

    [Header("Gain Per Level")]
    public float damagePerLevel = 3f;
    public float hpPerLevel = 15f;
    public float armorPerLevel = 2f;
    public float critRatePerLevel = 2f;
    public float critDamagePerLevel = 10f;
    public float goldDropPerLevel = 5f;
    public float lifestealPerLevel = 1f;

    [Header("Upgrade Cost")]
    public int baseCost = 10;
    public float costMultiplier = 1.5f;

    public float Damage => baseDamage + (damageLevel * damagePerLevel);
    public float MaxHP => baseMaxHP + (hpLevel * hpPerLevel);
    public float Armor => baseArmor + (armorLevel * armorPerLevel);
    public float CritRate => baseCritRate + (critRateLevel * critRatePerLevel);
    public float CritDamage => baseCritDamage + (critDamageLevel * critDamagePerLevel);
    public float GoldDropRate => baseGoldDropRate + (goldDropLevel * goldDropPerLevel);
    public float Lifesteal => baseLifesteal + (lifestealLevel * lifestealPerLevel);

    public bool LoadedFromPersistence { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        LoadPersistentProgress();

        Instance = this;
        InstanceChanged?.Invoke(this);
        NotifyStatsChanged(false);
    }

    private void OnEnable()
    {
        if (Instance == null || Instance == this)
        {
            Instance = this;
            InstanceChanged?.Invoke(this);
            NotifyStatsChanged(false);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
            InstanceChanged?.Invoke(null);
        }
    }

    public int GetUpgradeCost(int currentLevel)
    {
        return Mathf.RoundToInt(baseCost * Mathf.Pow(costMultiplier, currentLevel));
    }

    public bool UpgradeStat(string statName)
    {
        int level = GetStatLevel(statName);
        int cost = GetUpgradeCost(level);

        if (totalCoins < cost)
        {
            Debug.Log($"Not enough gold. Need {cost}, have {totalCoins}");
            return false;
        }

        totalCoins -= cost;

        switch (statName)
        {
            case "damage":
                damageLevel++;
                break;
            case "hp":
                hpLevel++;
                break;
            case "armor":
                armorLevel++;
                break;
            case "critRate":
                critRateLevel++;
                break;
            case "critDamage":
                critDamageLevel++;
                break;
            case "goldDrop":
                goldDropLevel++;
                break;
            case "lifesteal":
                lifestealLevel++;
                break;
            default:
                return false;
        }

        if (statName == "hp")
        {
            PlayerHealth health = GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.maxHP = MaxHP;
                health.Heal(hpPerLevel);
            }
        }

        NotifyStatsChanged();
        return true;
    }

    public int GetStatLevel(string statName)
    {
        switch (statName)
        {
            case "damage":
                return damageLevel;
            case "hp":
                return hpLevel;
            case "armor":
                return armorLevel;
            case "critRate":
                return critRateLevel;
            case "critDamage":
                return critDamageLevel;
            case "goldDrop":
                return goldDropLevel;
            case "lifesteal":
                return lifestealLevel;
            default:
                return 0;
        }
    }

    public float GetStatValue(string statName)
    {
        switch (statName)
        {
            case "damage":
                return Damage;
            case "hp":
                return MaxHP;
            case "armor":
                return Armor;
            case "critRate":
                return CritRate;
            case "critDamage":
                return CritDamage;
            case "goldDrop":
                return GoldDropRate;
            case "lifesteal":
                return Lifesteal;
            default:
                return 0f;
        }
    }

    public void AddCoins(int amount)
    {
        if (amount == 0)
        {
            return;
        }

        totalCoins += amount;
        NotifyStatsChanged();
    }

    public float CalculateDamage()
    {
        float damage = Damage;
        if (UnityEngine.Random.Range(0f, 100f) < CritRate)
        {
            damage *= CritDamage / 100f;
        }

        return damage;
    }

    public float CalculateDamageTaken(float rawDamage)
    {
        float reduction = 100f / (100f + Armor);
        return rawDamage * reduction;
    }

    public void SaveProgressNow()
    {
        if (!enabled)
        {
            return;
        }

        PlayerProgressPersistence.Save(this, GetComponent<PlayerHealth>());
    }

    private void LoadPersistentProgress()
    {
        if (!PlayerProgressPersistence.TryLoad(out PlayerProgressData data))
        {
            LoadedFromPersistence = false;
            return;
        }

        PlayerProgressPersistence.Apply(data, this);
        LoadedFromPersistence = true;
    }

    private void NotifyStatsChanged(bool persist = true)
    {
        StatsChanged?.Invoke();

        if (!persist)
        {
            return;
        }

        SaveProgressNow();
    }
}
