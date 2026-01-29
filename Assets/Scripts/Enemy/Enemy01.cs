
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Enemy01 : EnemyBase
{
    [SerializeField]private DecalProjector AttackRange;
    
    private int enemyID = 1;
    public override void Attack()
    {
        _animator.SetTrigger("ATTACK");
    }



    #region 이벤트 등록 함수
    protected override void TakeDamageHandler(float damage)
    {
        _navMeshAgent.isStopped = true;
        hitEffect();
        
        if (!IsAttack)
        {
            transform.LookAt(new Vector3(_player.transform.position.x, 0, _player.transform.position.z));
            Vector3 dashDirection = _player.transform.forward;
            _rigidbody.linearVelocity = dashDirection * 2f;
            _animator.SetTrigger("HIT");
            _animator.SetFloat("moveSpeed", 0);
        }
    }
    
    protected override void DieHandler()
    {
        SetAttackArange(false);
    }
    #endregion
    
    
    public override void SetAdditionalData(List<GameObject> patrolPoints) 
    {
        _behavior.SetVariableValue("PatrolPoints", patrolPoints);
    }

    public void SetAttackArange(bool isAcive)
    {
        if (isAcive)
        {
            float maxDistance = 70f; 
            RaycastHit hit;
            
            
            if (Physics.Raycast(transform.position, transform.forward, out hit, maxDistance,LayerMask.GetMask("Map")))
            {
                Vector3 newSize = AttackRange.size;
                newSize.y = hit.distance*2f; 
                AttackRange.size= newSize;
            }
        }
        AttackRange.gameObject.SetActive(isAcive);
    }
    
    #region 애니메이션 이벤트 함수

    public void OffAttackArrange()
    {
        SetAttackArange(false);
    }

    public void AttackFinish()
    {
        IsAttack = false;
    }
    public void HitFinish()
    {
        _navMeshAgent.isStopped = false;
    }

    #endregion
   

    
}