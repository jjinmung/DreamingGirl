public class AbilityFire : AbilityEffect
{
    
    public override void Apply(int stack)
    {
        Managers.Player.Combat.IsFireAttack = true;
    }

    public override void ApplyStack(int stack)
    {
       
    }

    public override void Remove(int stack)
    {
        Managers.Player.Combat.IsFireAttack = false;
    }
}