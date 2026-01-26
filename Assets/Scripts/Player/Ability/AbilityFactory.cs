public class AbilityFactory
{
    public static AbilityEffect CreateEffect(Define.AbilityID id)
    {
        switch (id)
        {
            case Define.AbilityID.Fire: return new AbilityFire();
            case Define.AbilityID.Frost: return new AbilityFrost();
            // ... 나머지 스킬들 매핑
            default: return null;
        }
    }
}