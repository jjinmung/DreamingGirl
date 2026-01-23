[System.Serializable]
public class Stat
{
    public float baseValue;        // 기본값
    public float flatBonus;        // 고정 증가 (+)
    public float percentBonus;     // 배율 증가 (%)

    // 최종값
    public float TotalValue
    {
        get
        {
            return (baseValue + flatBonus) * (1f + percentBonus);
        }
    }

    public Stat(float baseValue)
    {
        this.baseValue = baseValue;
        flatBonus = 0f;
        percentBonus = 0f;
    }
}