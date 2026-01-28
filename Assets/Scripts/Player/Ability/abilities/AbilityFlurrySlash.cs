public class AbilityFlurrySlash :ActiveAbilityEffect
{
    public override void Apply(int stack)
    {
        Managers.Player.PlayerControl.GetAciveSkill(Define.AbilityID.Flurry_Slash);
    }
    public override void Execute()
    {
        
    }

    
}