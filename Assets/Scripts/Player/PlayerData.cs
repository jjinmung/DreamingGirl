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
    public Stat attackPower;
    public Stat moveSpeed;
    public Stat criticalChance;

    // 기본 생성자 (초기값 설정)
    public PlayerData()
    {
        maxHp = new Stat(100f);
        currentHp = 100f;
        level = 1;
        currentExp = 0;
        nextLevelExp = 100;
        gold = 0;
        attackPower = new Stat(10f);
        moveSpeed = new Stat(5f);
        criticalChance = new Stat(0.5f);
    }
}