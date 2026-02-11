using System;
using UnityEngine;
using static Define;


public class PlayerController : MonoBehaviour
{
    private PlayerMovement _movement;
    private PlayerCombat _combat;
    private PlayerInteraction _interaction;
    private SphereCollider _attackcollider;
    private Animator _animator;
    private Vector2 _inputVector;
    public enum PlayerState {Idle, Run,Attack,Dash}
    public PlayerState CurrentState = PlayerState.Idle;
    [Header("이펙트")]
    public ParticleSystem LVPParticle;
    public ParticleSystem HealParticle;
    public ParticleSystem StatUpParticle;
    public ParticleSystem PactAbyssParticle;
    public TrailRenderer[] ThunderTrail;
    public DynamicParticleRotator DivineOrbs;
    
    [HideInInspector]public MotionTrail motionTrail;
    public AbilityID[] ActiveSkills;

    private UI_Ability uiAbility;
    private bool AttackDelay = true;
    private bool _isAttackPressed;
    //UI맵핑을 위한 이벤트
    public event Action OnGetActiveSKill;
    public  event Action<int, float> OnUseActiveSKill;
    
    private float[] _lastSkillTime = new  float[5]{-999f,-999f,-999f,-999f,-999f};
    private void Awake()
    {
        //컴포넌트 캐싱
        _movement = GetComponent<PlayerMovement>();
        _combat = GetComponent<PlayerCombat>();
        _interaction= GetComponentInChildren<PlayerInteraction>();
        _animator = GetComponent<Animator>();
        _attackcollider= GetComponentInChildren<SphereCollider>();
        DivineOrbs = GetComponentInChildren<DynamicParticleRotator>();
        
        _attackcollider.enabled = false;
        
        //액티브스킬 초기화
        ActiveSkills = new AbilityID[4]
        {
            AbilityID.None,
            AbilityID.None,
            AbilityID.None,
            AbilityID.None,
        };
        
        //이펙트 초기화 
        motionTrail = FindAnyObjectByType<MotionTrail>();
        if (motionTrail != null)
            motionTrail.TargetSkinMeshes = GetComponentsInChildren<SkinnedMeshRenderer>();
        motionTrail.gameObject.SetActive(false);
        DivineOrbs.SetOrbs(0);
    }

    private void Start()
    {
        // 안전하게 기존 구독 해제 후 재등록
        Managers.Input.OnDash -= HandleDashInput;
        Managers.Input.OnDash += HandleDashInput;
    
        // 람다 대신 메서드 참조를 위해 수정 (Action<int>를 지원하지 않는다면 아래처럼)
        Managers.Input.OnSkill1 -= InputSkill1; Managers.Input.OnSkill1 += InputSkill1;
        Managers.Input.OnSkill2 -= InputSkill2; Managers.Input.OnSkill2 += InputSkill2;
        Managers.Input.OnSkill3 -= InputSkill3; Managers.Input.OnSkill3 += InputSkill3;
        Managers.Input.OnSkill4 -= InputSkill4; Managers.Input.OnSkill4 += InputSkill4;
    }

// 델리게이트용 래핑 메서드들
    private void InputSkill1() => HandleSkillInput(1);
    private void InputSkill2() => HandleSkillInput(2);
    private void InputSkill3() => HandleSkillInput(3);
    private void InputSkill4() => HandleSkillInput(4);
    private void FixedUpdate()
    {
        _inputVector = Managers.Input.GetMoveInput();
        
        Vector3 moveDir = CalculateCameraDirection();
        _movement.Move(_inputVector, moveDir);
        
        if (Managers.Input.IsAttackPressed&&AttackDelay)
        {
            _combat.AddBuffer("Attack");
            AttackDelay=false;
            Invoke(nameof(DelayAttackInput),0.5f);
        }
        
        _combat.ProcessBuffer();
            
        
        UpdateAnimation();


    }

    void DelayAttackInput()
    {
        AttackDelay=true;
    }

    private Vector3 CalculateCameraDirection()
    {
        Vector3 forward = Camera.main.transform.forward;
        Vector3 right = Camera.main.transform.right;
        forward.y = 0; right.y = 0;
        return (forward * _inputVector.y + right * _inputVector.x).normalized;
    }
    
    private void HandleSkillInput(int slotIndex)
    {
        var activeSkill = ActiveSkills[slotIndex-1];
        if (activeSkill!=AbilityID.None)
        {
            var ActiveEffect = Managers.Data.AbilityDict[activeSkill].getActiveEffect();
            
            if(Time.time < _lastSkillTime[slotIndex]+ActiveEffect.Cooldown) return;
            _lastSkillTime[slotIndex] = Time.time;
            // 애니메이션 이름을 포함한 버퍼 추가
            _combat.AddBuffer($"Skill_{ActiveEffect.AnimationName}");
            OnUseActiveSKill?.Invoke(slotIndex,ActiveEffect.Cooldown);
            // 나중에 Execute를 호출하기 위해 현재 실행 중인 스킬 정보를 저장해둘 수 있음
            _combat.CurrentActiveEffect = ActiveEffect; 
        }
    }

    public void GetAciveSkill(AbilityID activeId)
    {
        bool isAdd = false;
        for (int i = 0; i < ActiveSkills.Length; i++)
        {
            if (ActiveSkills[i] == AbilityID.None)
            {
                isAdd = true;
                ActiveSkills[i] = activeId;
                break;
            }
        }

        if (!isAdd)
        {
            Debug.Log("스킬창이 다 찼음!");
            return;
        }
        OnGetActiveSKill.Invoke();

    }
    private async void HandleDashInput()
    {
        float dashCoolDown = Managers.Player.Data.dashCooldown.TotalValue;
        if (!_movement.CanMove||Time.time<_lastSkillTime[0]+dashCoolDown) return;
        await Managers.Sound.PlayEffect(Address.PlayerDash);
        _lastSkillTime[0] = Time.time;
        OnUseActiveSKill?.Invoke(0,dashCoolDown);
        _movement.ExecuteDash(CalculateCameraDirection(), () => {
            _combat.ClearBuffer();
            _combat.ResetCombo();
            _animator.SetTrigger("QUICK SHIFT F");
            _movement.CanMove = false;
            _combat.CanAttack = false;
            CurrentState = PlayerState.Dash;
        });

    }
    private void UpdateAnimation()
    {
        if (!_movement.CanMove) return;
        if (_inputVector.sqrMagnitude > 0.01f && CurrentState != PlayerState.Run)
        {
            CurrentState = PlayerState.Run;
            _animator.SetFloat("MOVE",1f);
        }
        else if (_inputVector.sqrMagnitude <= 0.01f && CurrentState != PlayerState.Idle)
        {
            CurrentState = PlayerState.Idle;
            _animator.SetFloat("MOVE",0f);
        }
    }

    // 애니메이션 이벤트 브릿지
    public void OnAnimationFinished()
    {
        InputActive(true);
        _combat.ResetCombo();
    }
    public void OnAttackFinished()
    {
        if (CurrentState == PlayerState.Attack)
        {
            InputActive(true);
        }
        _combat.ResetCombo();
    }
    
    //애니메이션 이벤트 함수
    public void CheckCombo()
    {
        if (CurrentState == PlayerState.Attack)
        {
            _combat.CanAttack = true;
        }
        StopDashPhysics();
        _attackcollider.enabled = false;
    }
    public void PlayAttack(int index)
    {
        _combat.PlayAttackEffect(index);
        _attackcollider.enabled = true;
        
    }

    public void OnMotionTrail()
    {
        motionTrail.gameObject.SetActive(true);
        _animator.speed = 0;
        Invoke(nameof(StartFlashAttack),0.5f);
    }

    private void StartFlashAttack()
    {
        _animator.speed = 1;
    }
    public void OffMotionTrail()
    {
        motionTrail.EffectOff();
    }
    
    public void StopDashPhysics() => _movement.StopVelocity();

    public void InputActive(bool isActive)
    {
        _movement.CanMove = isActive; 
        _combat.CanAttack = isActive;
        _interaction.CanInteract = isActive;
        
        _combat.ClearBuffer();
        
    }
    private void OnDestroy()
    {
        if (Managers.Instance != null) 
        {
            // 인스턴스가 있을 때만 안전하게 이벤트 해제
            var input = Managers.Input; 
            if (input != null)
            {
                input.OnDash -= HandleDashInput;
                input.OnSkill1 -= InputSkill1;
                input.OnSkill2 -= InputSkill2;
                input.OnSkill3 -= InputSkill3;
                input.OnSkill4 -= InputSkill4;
            }
        }
    }
    
}