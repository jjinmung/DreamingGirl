public class AbilitySwift : AbilityEffect
{
    public override void Apply(int stack)
    {
        Managers.Player.Control.StatUpParticle.Play();
        Managers.Player.AddPermanentStat(Define.PlayerStat.MoveSpeed,0.1f,true);
        Managers.Player.AddPermanentStat(Define.PlayerStat.attackSpeed,0.1f,true);
    }

    public override void ApplyStack(int stack)
    {
        Managers.Player.Control.StatUpParticle.Play();
        Managers.Player.AddPermanentStat(Define.PlayerStat.MoveSpeed,0.1f,true);
        Managers.Player.AddPermanentStat(Define.PlayerStat.attackSpeed,0.1f,true);
    }

    public override void Remove(int stack)
    {
        Managers.Player.AddPermanentStat(Define.PlayerStat.MoveSpeed,-0.1f*stack,true);
        Managers.Player.AddPermanentStat(Define.PlayerStat.attackSpeed,-0.1f*stack,true);
    }
}