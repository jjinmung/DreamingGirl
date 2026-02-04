public class AbilityFire : AbilityEffect
{
    
    public override void Apply(int stack)
    {
        Managers.Player.Combat.IsFireAttack = true;
        Managers.Player.Combat.FireDamageRatio = 0.15f;
    }

    public override void ApplyStack(int stack)
    {
        switch (stack)
        {
            case 2:
                Managers.Player.Combat.FireDamageRatio *= 1.1f;
                break;
            case 3:
                Managers.Player.Combat.FireDamageRatio *= 1.2f;
                break;
            case 4:
                Managers.Player.Combat.FireDamageRatio *= 1.3f;
                break;
            case 5:
                Managers.Player.Combat.FireDamageRatio *= 1.5f;
                break;
        }
    }

    public override void Remove(int stack)
    {
        Managers.Player.Combat.IsFireAttack = false;
        Managers.Player.Combat.FireDamageRatio = 0;
    }
}