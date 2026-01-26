public class AbilityInstance
{
    public AbilityData data;
    public AbilityEffect effect;
    public int stack;

    public AbilityInstance(AbilityData data)
    {
        this.data = data;
        this.effect = data.GetEffect();
        stack = 0;
    }

    public void AddStack()
    {
        if (stack >= data.maxStack)
            return;

        stack++;

        if (stack == 1)
            effect.Apply(stack);      // 최초 획득
        else
            effect.ApplyStack(stack); // 중첩 증가
    }

    public void RemoveAll()
    {
        if (stack <= 0)
            return;

        effect.Remove(stack);
        stack = 0;
    }
}