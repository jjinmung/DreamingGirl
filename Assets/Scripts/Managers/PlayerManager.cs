using UnityEngine;
using System;

public class PlayerManager : MonoBehaviour
{
    
    [Header("Player Data")]
    public PlayerData data;

    // 데이터 변경 시 UI 업데이트 등을 위한 이벤트
    public event Action OnDataChanged;
    public event Action OnPlayerDeath;
    

    // --- 데이터 수정 메소드들 ---

    public void TakeDamage(float amount)
    {
        data.currentHp -= amount;
        data.currentHp = Mathf.Clamp(data.currentHp, 0, data.maxHp.TotalValue);
        
        OnDataChanged?.Invoke(); // UI 등에 변경 알림

        if (data.currentHp <= 0)
        {
            OnPlayerDeath?.Invoke();
        }
    }

    public void AddGold(int amount)
    {
        data.gold += amount;
        OnDataChanged?.Invoke();
    }

    public void AddExp(int amount)
    {
        data.currentExp += amount;
        if (data.currentExp >= data.nextLevelExp)
        {
            LevelUp();
        }
        OnDataChanged?.Invoke();
    }

    private void LevelUp()
    {
        data.level++;
        data.currentExp -= data.nextLevelExp;
        data.nextLevelExp = Mathf.RoundToInt(data.nextLevelExp * 1.2f); // 레벨업 필요량 증가
        
        // 레벨업 시 풀피 회복 등 로직
        data.currentHp = data.maxHp.TotalValue;
        
        Debug.Log("Level Up! 현재 레벨: " + data.level);
        
    }
    
    public enum StatType { Attack, MaxHP, MoveSpeed, Critical }

    // 스탯을 영구적으로 강화하는 메소드 (레벨업 보상 등)
    public void AddPermanentStat(StatType type, float amount)
    {
        switch (type)
        {
            case StatType.Attack:
                data.attackPower.addValue += amount;
                break;
            case StatType.MaxHP:
                data.maxHp.addValue += amount;
                break;
            case StatType.MoveSpeed:
                data.moveSpeed.addValue += amount;
                break;
            case StatType.Critical:
                data.criticalChance.addValue += amount;
                break;
        }
        Debug.Log($"{type} 스탯이 {amount}만큼 증가했습니다! 현재: {GetStat(type).TotalValue}");
    }

    public Stat GetStat(StatType type)
    {
        return type switch
        {
            StatType.Attack => data.attackPower,
            StatType.MaxHP => data.maxHp,
            StatType.MoveSpeed => data.moveSpeed,
            StatType.Critical=>data.criticalChance,
            _ => null
        };
    }
    
}