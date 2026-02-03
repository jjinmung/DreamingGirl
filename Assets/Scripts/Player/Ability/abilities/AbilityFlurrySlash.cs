public class AbilityFlurrySlash :ActiveAbilityEffect
{
    public override void Apply(int stack)
    {
        Managers.Player.Control.GetAciveSkill(Define.AbilityID.Flurry_Slash);
    }
    public override void Execute()
    {
        
    }

    
}