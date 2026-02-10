using System;
using System.Collections;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using PixPlays.ElementalVFX;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Enemy03 : EnemyBase
{
    private int attackIndex = 0;

    private float beamDuration = 5f;
    private float beamRotaion = 0.7f;
    
    private Vector3 _dashTargetPos;
    
    [Header("Effects")]
    [SerializeField]private GameObject Blast;
    [SerializeField]private ParticleSystem DashEffect;
    [SerializeField]private DecalProjector[] attackRanges;
    [SerializeField] private BeamVfx beam;
    
    private CinemachineCollisionImpulseSource cam;
    private SphereCollider attackcollider;
    [SerializeField]private GameObject ProjectilePos;
    private bool isBeamAttack;
    private AudioSource _loopSource;
    
    private void Awake()
    {
        cam =  GetComponentInChildren<CinemachineCollisionImpulseSource>();
        attackcollider = GetComponentInChildren<SphereCollider>();
        attackRanges = GetComponentsInChildren<DecalProjector>(true);
    }

    public override async UniTask Init(int id)
    {
        await base.Init(id);
        var hpBar = await Managers.UI.MakeSubItem<UI_EnemyHPBar>(Address.Boss_HP_BAR);
        //위치 초기화
        RectTransform rect = hpBar.GetComponent<RectTransform>();
        if (rect != null)
        {
            // 1. 앵커를 중앙으로 설정 (부모의 중앙 기준)
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);

            // 2. 위치 좌표를 0,0으로 (중앙 정렬)
            rect.anchoredPosition = Vector2.zero;
        }
        
        hpBar.SetMaxHP(stat.MaxHp);
        
        takeDamageAction -= hpBar.TakeDamage;
        takeDamageAction += hpBar.TakeDamage;
        dieAcation -= hpBar.Destroy;
        dieAcation += hpBar.Destroy;
        gameObject.SetLayerRecursively("Enemy");
        attackIndex = 0;
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
        if(isDead) return;
        IsAttack = true;
        _behavior.SetVariableValue("IsAttack", IsAttack);
        switch (attackIndex)
        {
            case 0:
                BallAttack();
                break;
            case 1:
                DashAttack();
                break;
            case 2:
                BeamAttack();
                break;
            case 3:
                BlastAttack();
                break;
        }

        attackIndex = (attackIndex + 1) % 4;
    }
    

    protected override void TakeDamageHandler(float damage)
    {
        hitEffect();
    }

    protected override void DieHandler()
    {
        for(int i=0; i<attackRanges.Length; i++)
            SetAttackArange(false,i);
        attackcollider.enabled = false;
        beam.Stop();
        DashEffect.gameObject.SetActive(false);
        OnDashComplete();
        
        Managers.Sound.PlayEffect(Address.Enemy03Roar).Forget();
    }

    #region 빔공격
    private void BeamAttack()
    {
        SetAttackArange(true, 0,2.5f, 2f, BeamStart);
    }

    async void BeamStart()
    {
        //사운드 시작
        _loopSource = await Managers.Sound.PlayEffectLoop(Address.Enemy03Beam);
        
        isBeamAttack = true;
        _animator.SetTrigger("BEAMSTART");
        SetAttackArange(false,0);
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
        
        //사운드 종료
        Managers.Sound.StopLoop(_loopSource, 1.0f);
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
            beamRotaion * Time.deltaTime
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
  public async void DashAttack()
    {
        _navMeshAgent.isStopped = true;
        _animator.SetFloat("moveSpeed", 0);
        _animator.SetBool("DASH",true);
        _rigidbody.mass = 1f;
        
        DashEffect.gameObject.SetActive(true);
        DashEffect.Play();
         //사운드 시작
        _loopSource = await Managers.Sound.PlayEffectLoop(Address.Enemy03Dash);
        SetAttackArange(true, 0,5f,2f,Dash);
      
    }



    private async void Dash()
    {
        _navMeshAgent.isStopped = false;
        _navMeshAgent.speed = 15f;
        _animator.SetFloat("MoveAnim", 2);
        FadeMoveFloat(2f);
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
        FadeMoveFloat(0);
        _animator.SetFloat("MoveAnim", 1);
        DashEffect.Stop();
        _rigidbody.mass = 2000f;
        DashEffect.gameObject.SetActive(false);
        attackcollider.enabled = false;
        SetAttackArange(false,0);
        IsAttack = false;
        _behavior.SetVariableValue("IsAttack", IsAttack);
        
        //사운드 종료
        Managers.Sound.StopLoop(_loopSource, 1.0f);
    }
    
    public void FadeMoveFloat(float targetValue, float duration = 0.5f)
    {
        StartCoroutine(CoUpdateAnimFloat(targetValue, duration));
    }

    private IEnumerator CoUpdateAnimFloat(float targetValue, float duration)
    {
        float startValue = _animator.GetFloat("moveSpeed");
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            // 0에서 1 사이의 진행률 계산
            float nextValue = Mathf.Lerp(startValue, targetValue, elapsedTime / duration);
        
            _animator.SetFloat("moveSpeed", nextValue);
            yield return null;
        }

        // 마지막에 정확한 목표값으로 설정
        _animator.SetFloat("moveSpeed", targetValue);
    }
    #endregion

    #region 블래스터 공격
    private void BlastAttack()
    {
        Blast.SetActive(true);
        Invoke((nameof(BlastAnimation)),1.05f);
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
    

    #endregion

    #region 볼 공격
    private void BallAttack()
    {
        _animator.SetTrigger("BALLREADY");
        StartCoroutine(BallAttackStart());
        
    }



    IEnumerator BallAttackStart()
    {
        float angleStep = 15f;
        // 몬스터가 바라보는 현재 방향을 기준으로 설정
        Vector3 horizontalDir = _player.transform.position - transform.position;
        horizontalDir.y = 0;
        Quaternion centerRotation = Quaternion.LookRotation(horizontalDir);

        for (int i = 0; i < attackRanges.Length; i++)
        {
            float offsetAngle = (i - (attackRanges.Length - 1) / 2f) * angleStep;
        
            // 몬스터 정면에서 offsetAngle만큼 Y축으로 회전한 방향 계산
            Quaternion finalRotation = centerRotation * Quaternion.Euler(0, offsetAngle, 0);
        

            // 여기서는 회전값만 설정하고, 레이캐스트는 SetAttackArange 내부에서 처리
            attackRanges[i].transform.rotation = finalRotation * Quaternion.Euler(90, 0, 0);

            // 2f, 2f는 각각 폭과 지속시간
            SetAttackArange(true, i, 2f, 2f, ShooBall);
            yield return new WaitForSeconds(0.2f);
        }

        IsAttack = false;
        _behavior.SetVariableValue("IsAttack", IsAttack);
        
    }

    void ShooBall()
    {
        _animator.SetTrigger("BALLSTART");
        Managers.Sound.PlayEffect(Address.Enemy03BallShoot).Forget();
        
    }

    #endregion
   

    public void Rage()
    {
        cam.GenerateImpulse();
        _animator.SetTrigger("RAGE");
        Managers.Sound.PlayEffect(Address.Enemy03Roar).Forget();
    }
    
    public void SetAttackArange(bool isAcive, int index,float width=0f, float duration=0,Action action=null)
    {

        if (isAcive)
        {
            attackRanges[index].gameObject.SetActive(true);
            Vector3 newSize = attackRanges[index].size;
            newSize.x = width;
            attackRanges[index].size = newSize;
        
            float maxDistance = 150f; 
            RaycastHit hit;
            float targetDistance = maxDistance;
            
            Vector3 rayDir;
            
            rayDir = attackRanges[index].transform.up; 
            
            rayDir.y = 0; // 수평 레이캐스트 보장
            rayDir.Normalize();

            // 레이캐스트 지점 저장
            if (Physics.Raycast(transform.position+Vector3.up*0.5f, rayDir, out hit, maxDistance, LayerMask.GetMask("Map")))
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

            StartCoroutine(AnimateRangeSize(index,rayDir, targetDistance * 2f, duration,action));
        }
        else
        {
            attackRanges[index].transform.localEulerAngles = new Vector3(90f, 0f, 0f);
            attackRanges[index].gameObject.SetActive(false);
        }
    }
    
    private IEnumerator AnimateRangeSize(int index,Vector3 dir,float targetY, float duration, Action action)
    {
        float elapsedTime = 0f;
        Vector3 initialSize = attackRanges[index].size;
        initialSize.y = 0f; 

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;
        
            Vector3 newSize = attackRanges[index].size;
            newSize.y = Mathf.Lerp(initialSize.y, targetY, progress);
            attackRanges[index].size = newSize;

            yield return null;
        }

        Vector3 finalSize = attackRanges[index].size;
        finalSize.y = targetY;
        attackRanges[index].size = finalSize;
        
    
        // 이펙트 비활성화 및 액션 시작
        ProjectilePos.transform.rotation = Quaternion.LookRotation(dir);
        
        attackRanges[index].transform.localEulerAngles = new Vector3(90f, 0f, 0f);
        attackRanges[index].gameObject.SetActive(false);
        
        action.Invoke();
    }
    
    
}
