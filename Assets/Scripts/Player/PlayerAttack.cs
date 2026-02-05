using System;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;


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
            var ratio = Managers.Player.Control.PactAbyssParticle.isPlaying
                ? 1+Managers.Player.Combat.ParctAyssAttackRatio
                : 1;
            float critcal =Random.Range(0, 100);
            if (critcal <= Managers.Player.Data.criticalChance.TotalValue)//크리티컬일 때
            {
                other.gameObject.GetComponent<IDamageable>().TakeDamage(player.Damage*1.5f*ratio,Color.red);
            }
            else
            {
                other.gameObject.GetComponent<IDamageable>().TakeDamage(player.Damage*ratio);   
            }
            
            
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
