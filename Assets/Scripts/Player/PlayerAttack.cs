using System;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    PlayerUnit player;
    private void Awake()
    {
        player = GetComponentInParent<PlayerUnit>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            other.gameObject.GetComponent<IDamageable>().TakeDamage(player.Damage);;
            
            Managers.Player.OnDamageDealt?.Invoke(player.Damage);
            var enemyBase = other.gameObject.GetComponent<EnemyBase>();
            if (Managers.Player.Combat.IsFireAttack)
            {
                if (enemyBase != null)
                    enemyBase.ApplyBurn(player.Damage, Managers.Player.Combat.FireDamageRatio, 3f);
            }
            
            if (Managers.Player.Combat.IsIceAttack)
            {
                if (enemyBase != null)
                    enemyBase.ApplyFreeze(player.Damage, Managers.Player.Combat.IceDamageRatio, 3f);
            }
        }
    }
}
