using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Enemy04 : EnemyBase
{
    public override void Attack()
    {
        if(isDead) return;
        IsAttack = true;
        _animator.SetTrigger("ATTACK");
    }
    


    #region 이벤트 등록 함수
    protected override void TakeDamageHandler(float damage)
    {
        hitEffect();
        if (!IsAttack)
        {
            _navMeshAgent.isStopped = true;
            transform.LookAt(new Vector3(_player.transform.position.x, 0, _player.transform.position.z));
            Vector3 dashDirection = _player.transform.forward;
            _rigidbody.linearVelocity = dashDirection * 2f;
            _animator.SetTrigger("HIT");
            _animator.SetFloat("moveSpeed", 0);
        }
    }
    
    protected override void DieHandler()
    {
        Managers.Sound.PlayEffect(Managers.Resource.Data.Enemy04Die).Forget();
    }
    #endregion
    
    
    public override void SetAdditionalData(List<GameObject> patrolPoints) 
    {
        _behavior.SetVariableValue("PatrolPoints", patrolPoints);
    }
    
    #region 애니메이션 이벤트 함수
    public void HitFinish()
    {
        _navMeshAgent.isStopped = false;
    }

    public void ShootSound()
    {
        Managers.Sound.PlayEffect(Managers.Resource.Data.Enemy03BallShoot).Forget();
    }
    #endregion
   

    
}