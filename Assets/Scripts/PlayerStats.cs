using UnityEngine;

/// <summary>
/// Quản lý chỉ số nhân vật. Gắn vào Player.
/// Các script khác (PlayerCombat, PlayerHealth, EnemyHealth) sẽ đọc chỉ số từ đây.
/// </summary>
public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }

    [Header("Chỉ số cơ bản")]
    public float baseDamage = 10f;
    public float baseMaxHP = 100f;
    public float baseArmor = 0f;
    public float baseCritRate = 5f;        // %
    public float baseCritDamage = 150f;     // % (150% = x1.5)
    public float baseGoldDropRate = 0f;     // % bonus
    public float baseLifesteal = 0f;        // %

    [Header("Điểm nâng cấp")]
    public int statPoints = 0;              // Điểm dùng để nâng chỉ số
    public int totalCoins = 0;              // Tổng coin

    [Header("Level nâng cấp")]
    public int damageLevel = 0;
    public int hpLevel = 0;
    public int armorLevel = 0;
    public int critRateLevel = 0;
    public int critDamageLevel = 0;
    public int goldDropLevel = 0;
    public int lifestealLevel = 0;

    [Header("Giá trị mỗi cấp")]
    public float damagePerLevel = 3f;
    public float hpPerLevel = 15f;
    public float armorPerLevel = 2f;
    public float critRatePerLevel = 2f;     // +2% mỗi cấp
    public float critDamagePerLevel = 10f;  // +10% mỗi cấp
    public float goldDropPerLevel = 5f;     // +5% mỗi cấp
    public float lifestealPerLevel = 1f;    // +1% mỗi cấp

    [Header("Chi phí nâng cấp")]
    public int baseCost = 10;               // Coin cần để nâng cấp lv1
    public float costMultiplier = 1.5f;     // x1.5 mỗi level

    // ==== Chỉ số hiện tại (tính toán) ====
    public float Damage => baseDamage + damageLevel * damagePerLevel;
    public float MaxHP => baseMaxHP + hpLevel * hpPerLevel;
    public float Armor => baseArmor + armorLevel * armorPerLevel;
    public float CritRate => baseCritRate + critRateLevel * critRatePerLevel;
    public float CritDamage => baseCritDamage + critDamageLevel * critDamagePerLevel;
    public float GoldDropRate => baseGoldDropRate + goldDropLevel * goldDropPerLevel;
    public float Lifesteal => baseLifesteal + lifestealLevel * lifestealPerLevel;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// Tính chi phí nâng cấp cho level tiếp theo
    /// </summary>
    public int GetUpgradeCost(int currentLevel)
    {
        return Mathf.RoundToInt(baseCost * Mathf.Pow(costMultiplier, currentLevel));
    }

    /// <summary>
    /// Nâng cấp chỉ số. Trả về true nếu thành công.
    /// </summary>
    public bool UpgradeStat(string statName)
    {
        int level = GetStatLevel(statName);
        int cost = GetUpgradeCost(level);

        if (totalCoins < cost)
        {
            Debug.Log($"Không đủ coin! Cần {cost}, có {totalCoins}");
            return false;
        }

        totalCoins -= cost;

        switch (statName)
        {
            case "damage":     damageLevel++;     break;
            case "hp":         hpLevel++;         break;
            case "armor":      armorLevel++;      break;
            case "critRate":   critRateLevel++;   break;
            case "critDamage": critDamageLevel++; break;
            case "goldDrop":   goldDropLevel++;   break;
            case "lifesteal":  lifestealLevel++;  break;
        }

        Debug.Log($"Nâng cấp {statName}! Level {level} → {level + 1}. Coin còn: {totalCoins}");

        // Cập nhật HP nếu nâng max HP
        if (statName == "hp")
        {
            PlayerHealth health = GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.maxHP = MaxHP;
                health.Heal(hpPerLevel); // Hồi lượng HP vừa nâng
            }
        }

        return true;
    }

    public int GetStatLevel(string statName)
    {
        switch (statName)
        {
            case "damage":     return damageLevel;
            case "hp":         return hpLevel;
            case "armor":      return armorLevel;
            case "critRate":   return critRateLevel;
            case "critDamage": return critDamageLevel;
            case "goldDrop":   return goldDropLevel;
            case "lifesteal":  return lifestealLevel;
            default: return 0;
        }
    }

    public float GetStatValue(string statName)
    {
        switch (statName)
        {
            case "damage":     return Damage;
            case "hp":         return MaxHP;
            case "armor":      return Armor;
            case "critRate":   return CritRate;
            case "critDamage": return CritDamage;
            case "goldDrop":   return GoldDropRate;
            case "lifesteal":  return Lifesteal;
            default: return 0;
        }
    }

    /// <summary>
    /// Thêm coin (gọi từ Collectible)
    /// </summary>
    public void AddCoins(int amount)
    {
        totalCoins += amount;
        Debug.Log($"+{amount} Coin! Tổng: {totalCoins}");
    }

    /// <summary>
    /// Tính damage thực tế (có crit)
    /// </summary>
    public float CalculateDamage()
    {
        float dmg = Damage;
        if (Random.Range(0f, 100f) < CritRate)
        {
            dmg *= CritDamage / 100f;
            Debug.Log("CRITICAL HIT!");
        }
        return dmg;
    }

    /// <summary>
    /// Tính damage nhận vào (có giáp)
    /// </summary>
    public float CalculateDamageTaken(float rawDamage)
    {
        // Giáp giảm damage theo công thức: dmg = raw * 100/(100+armor)
        float reduction = 100f / (100f + Armor);
        return rawDamage * reduction;
    }
}
