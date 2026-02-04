public class AbilityPactAbyss : AbilityEffect
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
        
    }
}