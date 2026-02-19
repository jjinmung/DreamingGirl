public class AbilityChain : AbilityEffect
{
    public override void Apply(int stack)
    {
        Managers.Player.Combat.IsChain = true;
    }

    public override void ApplyStack(int stack)
    {
       
    }

    public override void Remove(int stack)
    {
        
    }
}