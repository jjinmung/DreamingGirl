using System.Collections;
using UnityEngine;

public class ThunderflashAttack:MonoBehaviour
{
    private float Damage => Managers.Data.AbilityDict[Define.AbilityID.Thunderflash].data.Damage;
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            other.gameObject.GetComponent<IDamageable>().TakeDamage(Damage);
            Managers.Player.OnDamageDealt?.Invoke(Damage);
        }
    }
    
}