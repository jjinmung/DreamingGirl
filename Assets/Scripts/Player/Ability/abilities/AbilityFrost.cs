public class AbilityFrost:AbilityEffect
{
    public override void Apply(int stack)
    {
        Managers.Player.Combat.IsIceAttack = true;
    }

    public override void ApplyStack(int stack)
    {
       
    }

    public override void Remove(int stack)
    {
        Managers.Player.Combat.IsIceAttack = false;
    }
}