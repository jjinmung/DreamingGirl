using System.Collections;
using UnityEngine;

public class ThunderflashAttack:MonoBehaviour
{
    private float Damage => Managers.Data.AbilityDict[Define.AbilityID.Thunderflash].data.Damage;
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            StartCoroutine(delay(other.gameObject));
        }
    }

    IEnumerator delay(GameObject obj)
    {
        yield return new WaitForSeconds(0.5f);
        obj.GetComponent<IDamageable>().TakeDamage(Damage);
        Managers.Player.OnDamageDealt?.Invoke(Damage);
    }
    
}