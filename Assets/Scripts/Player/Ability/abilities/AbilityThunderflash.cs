public class AbilityThunderflash : ActiveAbilityEffect
{
    public override void Apply(int stack)
    {
        Managers.Player.PlayerControl.GetAciveSkill(Define.AbilityID.Thunderflash);
    }
    public override void Execute()
    {
        
    }
}