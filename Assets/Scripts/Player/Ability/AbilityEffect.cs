public abstract class AbilityEffect
{
    public virtual void Apply(int stack) { }
    public virtual void Remove(int stack) { }
    public virtual void ApplyStack(int stack) { }
}