public class AbilityStrength : AbilityEffect
{
    public override void Apply(int stack)
    {
        Managers.Player.Control.StatUpParticle.Play();
        Managers.Player.AddPermanentStat(Define.PlayerStat.Attack,10,false);
    }

    public override void ApplyStack(int stack)
    {
        Managers.Player.Control.StatUpParticle.Play();
        Managers.Player.AddPermanentStat(Define.PlayerStat.Attack,10,false);
    }

    public override void Remove(int stack)
    {
        Managers.Player.AddPermanentStat(Define.PlayerStat.Attack,-10*stack,false);
    }
}