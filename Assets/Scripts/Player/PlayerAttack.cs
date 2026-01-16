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
            other.gameObject.GetComponent<BaseUnit>().TakeDamage(player.damage);
        }
    }
}
