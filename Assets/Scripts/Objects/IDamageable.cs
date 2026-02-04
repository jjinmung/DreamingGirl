using UnityEngine;

public interface IDamageable
{
      public void TakeDamage(float damage, Color color=default, bool isRandom =false);  
}