using System;
using UnityEngine;

[Serializable]
public sealed class PlayerProgressData
{
    public int version = 1;

    public float baseDamage = 10f;
    public float baseMaxHP = 100f;
    public float baseArmor = 5f;
    public float baseCritRate = 0f;
    public float baseCritDamage = 10f;
    public float baseGoldDropRate = 0f;
    public float baseLifesteal = 0f;

    public int statPoints = 15;
    public int totalCoins = 0;

    public int damageLevel = 0;
    public int hpLevel = 0;
    public int armorLevel = 0;
    public int critRateLevel = 0;
    public int critDamageLevel = 0;
    public int goldDropLevel = 0;
    public int lifestealLevel = 0;

    public float damagePerLevel = 3f;
    public float hpPerLevel = 15f;
    public float armorPerLevel = 2f;
    public float critRatePerLevel = 2f;
    public float critDamagePerLevel = 10f;
    public float goldDropPerLevel = 5f;
    public float lifestealPerLevel = 1f;

    public int baseCost = 10;
    public float costMultiplier = 1.5f;

    public float currentHP = -1f;
}

public static class PlayerProgressPersistence
{
    private const string SaveKey = "HeroAct.PlayerProgress.v1";

    public static bool TryLoad(out PlayerProgressData data)
    {
        if (!PlayerPrefs.HasKey(SaveKey))
        {
            data = null;
            return false;
        }

        string json = PlayerPrefs.GetString(SaveKey, string.Empty);
        if (string.IsNullOrWhiteSpace(json))
        {
            data = null;
            return false;
        }

        try
        {
            data = JsonUtility.FromJson<PlayerProgressData>(json);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Failed to parse player progress save: {ex.Message}");
            data = null;
        }

        return data != null;
    }

    public static void Save(PlayerStats stats, PlayerHealth health)
    {
        if (stats == null)
        {
            return;
        }

        PlayerProgressData previous = TryLoad(out PlayerProgressData loaded) ? loaded : null;
        PlayerProgressData data = Capture(stats, health, previous);
        SaveRaw(data);
    }

    public static void SaveRespawnState(PlayerStats stats)
    {
        if (stats == null)
        {
            return;
        }

        PlayerProgressData previous = TryLoad(out PlayerProgressData loaded) ? loaded : null;
        PlayerProgressData data = Capture(stats, null, previous);
        data.currentHP = stats.MaxHP;
        SaveRaw(data);
    }

    public static void Apply(PlayerProgressData data, PlayerStats stats)
    {
        if (data == null || stats == null)
        {
            return;
        }

        stats.baseDamage = data.baseDamage;
        stats.baseMaxHP = data.baseMaxHP;
        stats.baseArmor = data.baseArmor;
        stats.baseCritRate = data.baseCritRate;
        stats.baseCritDamage = data.baseCritDamage;
        stats.baseGoldDropRate = data.baseGoldDropRate;
        stats.baseLifesteal = data.baseLifesteal;

        stats.statPoints = data.statPoints;
        stats.totalCoins = data.totalCoins;

        stats.damageLevel = data.damageLevel;
        stats.hpLevel = data.hpLevel;
        stats.armorLevel = data.armorLevel;
        stats.critRateLevel = data.critRateLevel;
        stats.critDamageLevel = data.critDamageLevel;
        stats.goldDropLevel = data.goldDropLevel;
        stats.lifestealLevel = data.lifestealLevel;

        stats.damagePerLevel = data.damagePerLevel;
        stats.hpPerLevel = data.hpPerLevel;
        stats.armorPerLevel = data.armorPerLevel;
        stats.critRatePerLevel = data.critRatePerLevel;
        stats.critDamagePerLevel = data.critDamagePerLevel;
        stats.goldDropPerLevel = data.goldDropPerLevel;
        stats.lifestealPerLevel = data.lifestealPerLevel;

        stats.baseCost = data.baseCost;
        stats.costMultiplier = data.costMultiplier;
    }

    public static float ResolveCurrentHP(float fallbackMaxHP)
    {
        if (!TryLoad(out PlayerProgressData data))
        {
            return fallbackMaxHP;
        }

        if (data.currentHP <= 0f)
        {
            return fallbackMaxHP;
        }

        return Mathf.Clamp(data.currentHP, 0f, fallbackMaxHP);
    }

    private static PlayerProgressData Capture(PlayerStats stats, PlayerHealth health, PlayerProgressData previous)
    {
        PlayerProgressData data = previous ?? new PlayerProgressData();

        data.baseDamage = stats.baseDamage;
        data.baseMaxHP = stats.baseMaxHP;
        data.baseArmor = stats.baseArmor;
        data.baseCritRate = stats.baseCritRate;
        data.baseCritDamage = stats.baseCritDamage;
        data.baseGoldDropRate = stats.baseGoldDropRate;
        data.baseLifesteal = stats.baseLifesteal;

        data.statPoints = stats.statPoints;
        data.totalCoins = stats.totalCoins;

        data.damageLevel = stats.damageLevel;
        data.hpLevel = stats.hpLevel;
        data.armorLevel = stats.armorLevel;
        data.critRateLevel = stats.critRateLevel;
        data.critDamageLevel = stats.critDamageLevel;
        data.goldDropLevel = stats.goldDropLevel;
        data.lifestealLevel = stats.lifestealLevel;

        data.damagePerLevel = stats.damagePerLevel;
        data.hpPerLevel = stats.hpPerLevel;
        data.armorPerLevel = stats.armorPerLevel;
        data.critRatePerLevel = stats.critRatePerLevel;
        data.critDamagePerLevel = stats.critDamagePerLevel;
        data.goldDropPerLevel = stats.goldDropPerLevel;
        data.lifestealPerLevel = stats.lifestealPerLevel;

        data.baseCost = stats.baseCost;
        data.costMultiplier = stats.costMultiplier;

        if (health != null && health.HasInitializedState)
        {
            data.currentHP = health.currentHP;
        }

        return data;
    }

    private static void SaveRaw(PlayerProgressData data)
    {
        if (data == null)
        {
            return;
        }

        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }
}
