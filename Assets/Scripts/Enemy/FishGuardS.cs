using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FishGuardS : EnemyBase
{
    [SerializeField]private DecalProjector AttackRange;
    
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

    private void Update()
    {
        float maxDistance = 70f; 
        RaycastHit hit;
        Debug.DrawRay(transform.position, transform.forward, Color.red);
        // 레이어 마스크를 사용해 바닥(Ground) 레이어만 감지하는 것이 좋습니다.
        if (Physics.Raycast(transform.position, transform.forward, out hit, maxDistance,LayerMask.GetMask("Map")))
        {
            Vector3 newSize = AttackRange.size;
            newSize.y = hit.distance * 2f; 
            AttackRange.size= newSize;
            Debug.DrawLine(transform.position, hit.point, Color.green);
        }
    }

    protected override void DieHandler()
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
        if (isAcive)
        {
            float maxDistance = 70f; 
            RaycastHit hit;
            
            // 레이어 마스크를 사용해 바닥(Ground) 레이어만 감지하는 것이 좋습니다.
            if (Physics.Raycast(transform.position, transform.forward, out hit, maxDistance,LayerMask.GetMask("Map")))
            {
                Vector3 newSize = AttackRange.size;
                newSize.y = hit.distance; 
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