public class AbilityVitality : AbilityEffect
{
    public override void Apply(int stack)
    {
        Managers.Player.Control.StatUpParticle.Play();
        Managers.Player.AddPermanentStat(Define.PlayerStat.MaxHP,100,false);
        
    }

    public override void ApplyStack(int stack)
    {
        Managers.Player.Control.StatUpParticle.Play();
        Managers.Player.AddPermanentStat(Define.PlayerStat.MaxHP,100,false);
    }

    public override void Remove(int stack)
    {
        Managers.Player.AddPermanentStat(Define.PlayerStat.MaxHP,-100*stack,false);
    }
}