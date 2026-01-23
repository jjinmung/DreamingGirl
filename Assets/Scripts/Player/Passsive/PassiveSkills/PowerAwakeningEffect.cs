public class PowerAwakeningEffect : PassiveEffect
{
    public override void Apply() => Managers.Player.AddPermanentStat(Define.PlayerStat.Attack, 0.2f, true);
    public override void Remove() => Managers.Player.AddPermanentStat(Define.PlayerStat.Attack, -0.2f, true);
}