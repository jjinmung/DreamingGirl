public class CriticalSense : PassiveEffect
{
    public override void Apply() => Managers.Player.AddPermanentStat(Define.PlayerStat.Critical, 30);
    public override void Remove() => Managers.Player.AddPermanentStat(Define.PlayerStat.Critical, -30);
}