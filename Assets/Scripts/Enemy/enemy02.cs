
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Enemy02 : EnemyBase
{
    private int enemyID = 2;
    private  CircleAttackRange attackRange;
    [SerializeField] private CurveProjectile projectile;
    public override void Attack()
    {
        _animator.SetTrigger("ATTACK");
        IsAttack = true;
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
            var tarpos = _player.transform.position;
            attackRange = Managers.Resource.Instantiate(Address.CircleAttackRange, tarpos+(Vector3.up*0.1f),Quaternion.Euler(90,0,0)).GetComponent<CircleAttackRange>();
            attackRange.Init(projectile.duration,stat.Damage);
        }
        else
        {
            Managers.Resource.Destroy(attackRange.gameObject);
        }
        
    }

    #region 애니메이션 이벤트 함수

    public void Shoot()
    {
        SetAttackArange(true);
        var tarpos = _player.transform.position;
        projectile.Launch(tarpos);
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