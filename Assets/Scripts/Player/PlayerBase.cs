using UnityEngine;

public class PlayerBase : BaseUnit
{
    
    protected override void Die()
    {
        base.Die();
        Destroy(gameObject,2f);
    }
}
