using System;
using UnityEngine;

public class Enemy03DashAttack : MonoBehaviour
{
    private EnemyBase enemy;
    private float power = 2f;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (enemy == null)
                enemy = GetComponentInParent<EnemyBase>();
            other.GetComponent<IDamageable>().TakeDamage(enemy.stat.Damage*power);
        }
    }
}
