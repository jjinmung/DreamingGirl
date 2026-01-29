using UnityEngine;

public class FlurrySlashAttack : MonoBehaviour
{
    private float Damage => Managers.Data.AbilityDict[Define.AbilityID.Flurry_Slash].data.Damage;
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            other.gameObject.GetComponent<IDamageable>().TakeDamage(Damage);
            Managers.Player.OnDamageDealt?.Invoke(Damage);
        }
    }
}
