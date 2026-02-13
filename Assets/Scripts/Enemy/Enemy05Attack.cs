using System;
using UnityEngine;

public class Enemy05Attack : MonoBehaviour
{
    private EnemyBase enemy;
    public float knockbackForce = 20f; // 날려버리는 힘의 세기
    public float upwardForce = 10f;   // 위로 살짝 띄우는 힘 (더 멀리 날아가는 느낌을 줌)
    private Rigidbody playerRb;
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject.tag);
        if (other.CompareTag("Player"))
        {
            if (enemy == null)
                enemy = GetComponentInParent<EnemyBase>();

            // 1. 대미지 입히기
            other.GetComponent<IDamageable>()?.TakeDamage(enemy.stat.Damage);

            // 2. 날려버리기 로직
            if (playerRb == null) playerRb = other.GetComponent<Rigidbody>();
            
            if (playerRb != null)
            {
                playerRb.linearVelocity = Vector3.zero; 
                playerRb.angularVelocity = Vector3.zero; 
                
                Managers.Player.Control.InputActive(false);
                Managers.Player.Anim.SetTrigger("STUN");
                CancelInvoke(nameof(PlayerInput)); 
                Invoke(nameof(PlayerInput), 1f);

                // --- 수정된 방향 계산 로직 ---
                
                // 적의 위치에서 플레이어를 바라보는 방향 (수평만)
                Vector3 toPlayer = (other.transform.position - transform.position);
                toPlayer.y = 0;
                
                // 위로 띄우는 힘 합치기
                Vector3 finalForce = (toPlayer + Vector3.up * upwardForce).normalized * knockbackForce;

                playerRb.AddForce(finalForce, ForceMode.Impulse);
            }
            
            GetComponent<SphereCollider>().enabled = false;
        }
    }

    void PlayerInput()
    {
        Managers.Player.Control.InputActive(true);
    }
}