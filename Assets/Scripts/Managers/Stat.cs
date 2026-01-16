[System.Serializable]
public class Stat
{
    public float baseValue; // 캐릭터 고유 기본값
    public float addValue;  // 레벨업, 장비, 버프로 인한 추가값

    // 최종 스탯 반환 (Read-only)
    public float TotalValue => baseValue + addValue;

    public Stat(float baseValue)
    {
        this.baseValue = baseValue;
        this.addValue = 0;
    }
}