using System;
using System.Collections;
using PixPlays.ElementalVFX;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Enemy03 : EnemyBase
{
    private int attackIndex = 0;
    private int BallAtttackCount = 0;
    private float beamDuration = 5f;
    private Coroutine rangeCoroutine;
    
    private Vector3 _dashTargetPos;
    
    
    [SerializeField]private GameObject Blast;
    [SerializeField]private GameObject DashEffect;
   
    private CinemachineCollisionImpulseSource cam;
    [SerializeField]private DecalProjector attackRange;
    private SphereCollider attackcollider;
    [SerializeField] private BeamVfx beam;
    private bool isBeamAttack;
    private void Awake()
    {
        cam =  GetComponentInChildren<CinemachineCollisionImpulseSource>();
        attackcollider = GetComponentInChildren<SphereCollider>();
    }

    public override void Init(int id)
    {
        base.Init(id);
        var hpBar = Managers.UI.MakeSubItem<UI_EnemyHPBar>(Address.Enemy_HP_BAR);
        hpBar.SetMaxHP(stat.MaxHp);
        hpBar.GetComponentInChildren<HealthBarController>().target = transform;
        takeDamageAction -= hpBar.TakeDamage;
        takeDamageAction += hpBar.TakeDamage;
        dieAcation -= hpBar.Destroy;
        dieAcation += hpBar.Destroy;
        gameObject.SetLayerRecursively("Enemy");
    }

    private void Update()
    {
        if (isBeamAttack)
        {
            ChaseTarget();
        }
    }



    public override void Attack()
    {
        IsAttack = true;
        _behavior.SetVariableValue("IsAttack", IsAttack);
        switch (attackIndex)
        {
            case 0:
                BeamAttack();
                break;
            case 1:
                DashAttack();
                break;
            case 2:
                BallAttack();
                break;
            case 3:
                BlastAttack();
                break;
        }

        attackIndex = (attackIndex + 1) % 4;
    }
    

    protected override void TakeDamageHandler(float damage)
    {
        _navMeshAgent.isStopped = true;
        hitEffect();
    }

    protected override void DieHandler()
    {
        SetAttackArange(false);
    }

    #region 빔공격
    private void BeamAttack()
    {
        SetAttackArange(true, 2.5f, 3f, BeamStart);
    }

    void BeamStart()
    {
        isBeamAttack = true;
        _animator.SetTrigger("BEAMSTART");
        SetAttackArange(false);
        Invoke(nameof(BeamEnd), beamDuration);
    }
    //애니메이션 이벤트함수
    public void DelayBeamAttack()
    {
        beam.Play(beamDuration,stat.Damage*0.2f);
    }
    void BeamEnd()
    {
        _animator.SetTrigger("BEAMEND");
        IsAttack = false;
        isBeamAttack = false;
        _behavior.SetVariableValue("IsAttack", IsAttack);
    }
    
    void ChaseTarget()
    {
        // 1. 타겟으로 향하는 방향 벡터 계산
        Vector3 direction = _player.transform.position - transform.position;
        direction.y = 0; // 높이 차이 무시
        

        // 2. 목표 회전값 계산
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        // 3. 부드러운 회전 적용
        transform.rotation = Quaternion.Slerp(
            transform.rotation, 
            targetRotation, 
            0.5f * Time.deltaTime
        );

        // 4. [핵심] 현재 회전과 목표 회전 사이의 각도 차이 계산
        float angleDiff = Quaternion.Angle(transform.rotation, targetRotation);

        // 각도 차이가 1도 이내라면 완료로 간주
        if (angleDiff < 1.0f)
        {
            // 정확히 목표 방향을 바라보도록 최종 보정
            transform.rotation = targetRotation;
        }
    }

    #endregion
    

    #region 대쉬공격
  public void DashAttack()
    {
        _navMeshAgent.isStopped = true;
        _animator.SetFloat("moveSpeed", 0);
        _animator.SetBool("DASH",true);
        
        TurnToDash();
    }

    private void TurnToDash()
    {
        // 1. 타겟으로 향하는 방향 벡터 계산
        Vector3 direction = _player.transform.position - transform.position;
        direction.y = 0; // 높이 차이 무시
        

        // 2. 목표 회전값 계산
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        // 3. 부드러운 회전 적용
        transform.rotation = Quaternion.Slerp(
            transform.rotation, 
            targetRotation, 
            20f * Time.deltaTime
        );

        // 4. [핵심] 현재 회전과 목표 회전 사이의 각도 차이 계산
        float angleDiff = Quaternion.Angle(transform.rotation, targetRotation);

        // 각도 차이가 1도 이내라면 완료로 간주
        if (angleDiff < 1.0f)
        {
            // 정확히 목표 방향을 바라보도록 최종 보정
            transform.rotation = targetRotation;
            //공경 범위 활성화
            SetAttackArange(true, 5f,2f,Dash);
        }
    }

    private void Dash()
    {
        _navMeshAgent.isStopped = false;
        _navMeshAgent.speed = 40f;
        _animator.SetFloat("moveSpeed", 2f);
        DashEffect.SetActive(true);
        attackcollider.enabled = true;
        // NavMesh 위의 유효한 위치인지 재확인 후 이동
        if (UnityEngine.AI.NavMesh.SamplePosition(_dashTargetPos, out UnityEngine.AI.NavMeshHit hit, 2.0f, UnityEngine.AI.NavMesh.AllAreas))
        {
            _navMeshAgent.SetDestination(hit.position);
        }
        else
        {
            _navMeshAgent.SetDestination(_dashTargetPos);
        }

        // 도착 감시 시작
        StartCoroutine(CheckDashArrival());
    }
    
    private IEnumerator CheckDashArrival()
    {
        // 경로 계산이 시작될 때까지 한 프레임 대기
        yield return null;
        // 경로가 아직 계산 중이거나, 남은 거리가 정지 거리보다 크면 계속 대기

        while (_navMeshAgent.pathPending || _navMeshAgent.remainingDistance > _navMeshAgent.stoppingDistance)
        {
            yield return null;
        }
        OnDashComplete();

    }
    private void OnDashComplete()
    {
        _navMeshAgent.isStopped = true;
        _navMeshAgent.speed = stat.Speed; // 원래 속도로 복구
        _animator.SetBool("DASH", false);
        _animator.SetFloat("moveSpeed", 0);
        DashEffect.SetActive(false);
        attackcollider.enabled = false;
        SetAttackArange(false);
        IsAttack = false;
        _behavior.SetVariableValue("IsAttack", IsAttack);
    }
    #endregion
  
    private void BlastAttack()
    {
        Blast.SetActive(true);
        Invoke((nameof(BlastAnimation)),1f);
        Invoke(nameof(BlastFisnished),2f);
    }

    public void BlastAnimation()
    {
        _animator.SetTrigger("BLAST");
    }
    
    public void BlastFisnished()
    {
        Blast.SetActive(false);
        IsAttack = false;
        _behavior.SetVariableValue("IsAttack", IsAttack);
    }
    private void BallAttack()
    {
        BallAtttackCount = 1;
        SetAttackArange(true, 2f,1f,ShootBall);
    }

    public void CountinueBallAttack()
    {
        transform.LookAt(new Vector3(_player.transform.position.x, 0, _player.transform.position.z));
        if (BallAtttackCount < 5)
        {
            SetAttackArange(true, 2f,1f,ShootBall);
            BallAtttackCount++;
        }
        else
        {
            IsAttack = false;
            _behavior.SetVariableValue("IsAttack", IsAttack);
        }
    }

    private void ShootBall()
    {
        _animator.SetTrigger("BALL");
    }

    public void Rage()
    {
        cam.GenerateImpulse();
        _animator.SetTrigger("RAGE");
    }
    
    public void SetAttackArange(bool isAcive, float width=0f, float duration=0,Action action=null)
    {
        if (rangeCoroutine != null) StopCoroutine(rangeCoroutine);

        if (isAcive)
        {
            attackRange.gameObject.SetActive(true);
            Vector3 newSize = attackRange.size;
            newSize.x = width;
            attackRange.size = newSize;
        
            float maxDistance = 150f; 
            RaycastHit hit;
            float targetDistance = maxDistance;

            // 레이캐스트 지점 저장
            if (Physics.Raycast(transform.position+Vector3.up*0.5f, transform.forward, out hit, maxDistance, LayerMask.GetMask("Map")))
            {
                targetDistance = hit.distance;
                // --- 수정된 부분 ---
                float offset = 1.0f; // 뒤로 물러날 거리 (미터 단위)
                // hit.point에서 레이가 날아온 방향(transform.forward)의 반대 방향으로 offset만큼 이동
                _dashTargetPos = hit.point - (transform.forward * offset);
                Debug.DrawLine(transform.position, hit.point, Color.green, 2f);
                // 부딪힌 지점에 작은 수직선을 그려서 표시
                Debug.DrawRay(transform.position+Vector3.up*0.5f, Vector3.up * 2f, Color.red, 2f);
            }
            else
            {
                // 부딪힌 곳이 없다면 최대 거리 지점을 저장
                _dashTargetPos = transform.position + (transform.forward * maxDistance);
            }

            rangeCoroutine = StartCoroutine(AnimateRangeSize(targetDistance * 2f, duration,action));
        }
        else
        {
            attackRange.gameObject.SetActive(false);
        }
    }
    
    private IEnumerator AnimateRangeSize(float targetY, float duration, Action action)
    {
        float elapsedTime = 0f;
        Vector3 initialSize = attackRange.size;
        initialSize.y = 0f; 

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;
        
            Vector3 newSize = attackRange.size;
            newSize.y = Mathf.Lerp(initialSize.y, targetY, progress);
            attackRange.size = newSize;

            yield return null;
        }

        Vector3 finalSize = attackRange.size;
        finalSize.y = targetY;
        attackRange.size = finalSize;
        rangeCoroutine = null;
    
        // 이펙트 비활성화 및 대쉬 시작
        attackRange.gameObject.SetActive(false); 
        action.Invoke();
    }
}
