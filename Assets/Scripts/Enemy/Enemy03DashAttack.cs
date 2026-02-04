using System;
using UnityEngine;

public class Enemy03DashAttack : MonoBehaviour
{
    private EnemyBase enemy;
    public float power = 2f; // 대미지 배율
    public float knockbackForce = 20f; // 날려버리는 힘의 세기
    public float upwardForce = 10f;   // 위로 살짝 띄우는 힘 (더 멀리 날아가는 느낌을 줌)
    private Rigidbody playerRb;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (enemy == null)
                enemy = GetComponentInParent<EnemyBase>();

            // 1. 대미지 입히기
            other.GetComponent<IDamageable>()?.TakeDamage(enemy.stat.Damage * power);

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

                // 적의 로컬 X축(Right) 방향 확인
                // 내 정면(forward) 기준으로 플레이어가 어느 쪽에 있는지 판단
                float side = Vector3.Dot(transform.right, toPlayer);

                Vector3 knockbackDir;
                if (side < 0)
                {
                    // 플레이어가 왼쪽 영역에 있음 -> 적의 왼쪽 방향으로 날림
                    knockbackDir = -transform.right;
                }
                else
                {
                    // 플레이어가 오른쪽 영역에 있음 -> 적의 오른쪽 방향으로 날림
                    knockbackDir = transform.right;
                }

                // 위로 띄우는 힘 합치기
                Vector3 finalForce = (knockbackDir + Vector3.up * upwardForce).normalized * knockbackForce;

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