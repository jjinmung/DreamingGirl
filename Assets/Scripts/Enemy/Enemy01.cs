
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Enemy01 : EnemyBase
{
    [SerializeField]private DecalProjector AttackRange;
    private Coroutine rangeCoroutine;
    private int enemyID = 1;
    public override void Attack()
    {
        base.Attack();
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
        // 이미 실행 중인 코루틴이 있다면 중지 (중복 실행 방지)
        if (rangeCoroutine != null)
        {
            StopCoroutine(rangeCoroutine);
        }

        if (isAcive)
        {
            AttackRange.gameObject.SetActive(true);
        
            float maxDistance = 70f; 
            RaycastHit hit;
            float targetDistance = maxDistance;

            if (Physics.Raycast(transform.position, transform.forward, out hit, maxDistance, LayerMask.GetMask("Map")))
            {
                targetDistance = hit.distance;
            }

            // 1초 동안 크기를 변경하는 코루틴 시작
            rangeCoroutine = StartCoroutine(AnimateRangeSize(targetDistance * 2f, 1f));
        }
        else
        {
            AttackRange.gameObject.SetActive(false);
        }
    }
    private IEnumerator AnimateRangeSize(float targetY, float duration)
    {
        float elapsedTime = 0f;
        Vector3 initialSize = AttackRange.size;
        
        initialSize.y = 0f; 
    
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;

            // 부드러운 움직임을 위해 Lerp 사용
            Vector3 newSize = AttackRange.size;
            newSize.y = Mathf.Lerp(initialSize.y, targetY, progress);
            AttackRange.size = newSize;

            yield return null;
        }

        // 최종 값 확정
        Vector3 finalSize = AttackRange.size;
        finalSize.y = targetY;
        AttackRange.size = finalSize;
    
        rangeCoroutine = null;
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