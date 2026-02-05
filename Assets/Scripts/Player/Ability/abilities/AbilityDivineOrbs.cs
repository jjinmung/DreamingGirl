public class AbilityDivineOrbs : AbilityEffect
{
    public override void Apply(int stack)
    {
        Managers.Player.Control.DivineOrbs.SetOrbs(2);
    }

    public override void ApplyStack(int stack)
    {
        switch (stack)
        {
            case 2:
                Managers.Player.Control.DivineOrbs.SetOrbs(3);
                break;
        }
    }

    public override void Remove(int stack)
    {
        Managers.Player.Control.DivineOrbs.SetOrbs(0);
    }
}