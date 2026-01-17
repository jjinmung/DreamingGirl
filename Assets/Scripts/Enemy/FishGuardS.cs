using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;

public class FishGuardS : EnemyBase
{
    [SerializeField]private GameObject AttackRange;
    
    public override void Attack()
    {
        _animator.SetTrigger("ATTACK");
    }

    public override void Init()
    {
        base.Init();
        
        takeDamageAction -= TakeDamageHandler;
        takeDamageAction += TakeDamageHandler;
        dieAcation -= DieHandler;
        dieAcation += DieHandler;
    }
    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);
        takeDamageAction.Invoke(damage);
    }

   
    
    protected override void Die()
    {
        base.Die();
        dieAcation.Invoke();
    }

    #region 이벤트 등록 함수
    private void TakeDamageHandler(float damage)
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

    private void DieHandler()
    {
        SetAttackArange(false);
    }
    #endregion
    
    
    public void SetVpartrolPoints(List<GameObject> partrolPoints)
    {
        if(_behavior == null)
            _behavior = GetComponent<BehaviorGraphAgent>();
        _behavior.SetVariableValue("PatrolPoints",partrolPoints);
    }

    public void SetAttackArange(bool isAcive)
    {
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