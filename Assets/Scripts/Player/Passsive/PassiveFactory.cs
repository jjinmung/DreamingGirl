public static class PassiveFactory
{
    public static PassiveEffect CreateEffect(Define.PassiveSkillID id)
    {
        switch (id)
        {
            case Define.PassiveSkillID.PowerAwakening: return new PowerAwakeningEffect();
            case Define.PassiveSkillID.LifeDrain: return new LifeDrainEffect();
            // ... 나머지 스킬들 매핑
            default: return null;
        }
    }
}