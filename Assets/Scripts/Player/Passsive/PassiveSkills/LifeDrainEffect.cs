public class LifeDrainEffect : PassiveEffect
{
    public override void Apply()
    {
        Managers.Player.OnDamageDealt += HandleLifeDrain;
    }

    private void HandleLifeDrain(float damage)
    {
        Managers.Player.Heal(damage);
    }

    public override void Remove() =>  Managers.Player.OnDamageDealt -= HandleLifeDrain;
}