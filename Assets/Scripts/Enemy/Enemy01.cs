
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Enemy01 : EnemyBase
{
    public override void Attack()
    {
        if(isDead) return;
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
        Managers.Sound.PlayEffect(Address.Enemy01Die).Forget();
    }
    #endregion
    
    
    public override void SetAdditionalData(List<GameObject> patrolPoints) 
    {
        _behavior.SetVariableValue("PatrolPoints", patrolPoints);
    }
    
    #region 애니메이션 이벤트 함수
    
    public void AttackFinish()
    {
        IsAttack = false;
    }
    public void HitFinish()
    {
        _navMeshAgent.isStopped = false;
    }

    public void ShootSound()
    {
        Managers.Sound.PlayEffect(Address.Enemy01Shoot).Forget();
    }
    #endregion
   

    
}