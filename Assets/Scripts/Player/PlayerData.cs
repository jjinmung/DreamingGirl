using Data;

[System.Serializable]
public class PlayerData
{
    public float currentHp;
    
    // 성장 및 재화
    public int level;
    public int currentExp;
    public int nextLevelExp;
    public int gold;

    // 전투 스탯 (스킬 선택에 의해 변동됨)
    public Stat maxHp;
    public Stat damage;
    public Stat moveSpeed;
    public Stat dashCooldown;
    public Stat criticalChance;
    public Stat attackSpeed;

    // 기본 생성자 (초기값 설정)
    public PlayerData(PlayerBasicStat basicStat)
    {
        maxHp = new Stat(basicStat.MaxHp);
        currentHp = maxHp.TotalValue;
        level = 1;
        currentExp = 0;
        nextLevelExp = 100;
        gold = 0;
        damage = new Stat(basicStat.Damage);
        moveSpeed = new Stat(basicStat.Speed);
        dashCooldown= new Stat(basicStat.DashCooldown);
        criticalChance = new Stat(basicStat.CritChance);
        attackSpeed = new  Stat(basicStat.AttackSpeed);
    }
}