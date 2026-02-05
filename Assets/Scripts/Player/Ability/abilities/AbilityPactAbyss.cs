public class AbilityPactAbyss : AbilityEffect
{
    public override void Apply(int stack)
    {
        Managers.Player.Combat.IsPactAbyss = true;
        Managers.Player.Combat.ParctAyssAttackRatio = 0.3f;
        Managers.Player.Combat.ParctAyssStartRatio = 0.5f;
        Managers.Player.AdjustPactAbyss();
            
    }

    public override void ApplyStack(int stack)
    {
        switch (stack)
        {
            case 2:
                Managers.Player.Combat.ParctAyssAttackRatio = 0.4f;
                if (Managers.Player.Control.PactAbyssParticle.isPlaying)
                {
                    Managers.Player.AddPermanentStat(Define.PlayerStat.Attack,0.1f,true);
                }
                break;
            case 3:
                Managers.Player.Combat.ParctAyssStartRatio = 0.7f;
                if (Managers.Player.Data.currentHp / Managers.Player.Data.maxHp.TotalValue <= 0.7f)
                {
                    if (Managers.Player.Control.PactAbyssParticle.isPlaying) return;
                    Managers.Player.AdjustPactAbyss();
                    
                }
                    
                break;
            case 4:
                break;
            case 5:
                break;
        }
    }

    public override void Remove(int stack)
    {
        Managers.Player.Combat.IsPactAbyss = false;
        Managers.Player.Combat.ParctAyssAttackRatio = 0f;
        Managers.Player.Combat.ParctAyssStartRatio = 0f;
    }
}