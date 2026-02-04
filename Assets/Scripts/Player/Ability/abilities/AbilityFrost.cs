public class AbilityFrost:AbilityEffect
{
    public override void Apply(int stack)
    {
        Managers.Player.Combat.IsIceAttack = true;
        Managers.Player.Combat.IceDamageRatio = 0.5f;
    }

    public override void ApplyStack(int stack)
    {
       
    }

    public override void Remove(int stack)
    {
        Managers.Player.Combat.IsIceAttack = false;
        Managers.Player.Combat.IceDamageRatio = 0;
    }
}