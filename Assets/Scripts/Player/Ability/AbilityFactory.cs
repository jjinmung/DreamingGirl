public class AbilityFactory
{
    public static AbilityEffect CreateEffect(Define.AbilityID id)
    {
        switch (id)
        {
            case Define.AbilityID.Fire: return new AbilityFire();
            case Define.AbilityID.Frost: return new AbilityFrost();
            case Define.AbilityID.Flurry_Slash : return new AbilityFlurrySlash();
            case Define.AbilityID.Thunderflash : return new AbilityThunderflash();
            case Define.AbilityID.Pact_Abyss : return new AbilityPactAbyss();
            case Define.AbilityID.Chain_Instinct : return new AbilityChain();
            case Define.AbilityID.Divine_Orbs : return new AbilityDivineOrbs();
            case Define.AbilityID.Enhanced_Strength : return new AbilityStrength();
            case Define.AbilityID.Enhanced_Vitality : return new AbilityVitality();
            case Define.AbilityID.Swift_Execution : return new AbilitySwift();
            default: return null;
        }
    }
}