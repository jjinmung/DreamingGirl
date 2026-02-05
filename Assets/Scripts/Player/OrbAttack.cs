using UnityEngine;
using System.Collections.Generic; // Dictionary 사용을 위해 필요

public class OrbAttack : MonoBehaviour
{
    private float Damage => Managers.Data.AbilityDict[Define.AbilityID.Divine_Orbs].data.Damage;
    
    // 적 오브젝트와 마지막 피격 시간을 저장하는 사전
    private Dictionary<GameObject, float> lastAttackTimes = new Dictionary<GameObject, float>();
    
    [SerializeField] private float attackInterval = 0.5f; // 재공격 주기 (0.5초)

    void OnTriggerStay(Collider other) // Enter보다 Stay가 비벼지는 상황에 더 안전합니다
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            if (CanAttack(other.gameObject))
            {
                ApplyDamage(other.gameObject);
            }
        }
    }

    private bool CanAttack(GameObject enemy)
    {
        // 처음 만난 적이거나, 마지막 공격으로부터 attackInterval이 지났는지 확인
        if (!lastAttackTimes.ContainsKey(enemy)) return true;
        
        return Time.time - lastAttackTimes[enemy] >= attackInterval;
    }

    private void ApplyDamage(GameObject enemy)
    {
        var damageable = enemy.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(Damage);
            Managers.Player.OnDamageDealt?.Invoke(Damage);

            // 공격 시간 기록 업데이트
            lastAttackTimes[enemy] = Time.time;
        }
    }

    // 오브젝트가 비활성화될 때 사전 청소 (메모리 관리)
    void OnDisable()
    {
        lastAttackTimes.Clear();
    }
}