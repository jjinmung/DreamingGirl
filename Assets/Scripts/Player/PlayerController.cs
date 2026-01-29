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
    public enum PlayerState { Idle, Run,Attack,Dash}
    public PlayerState CurrentState = PlayerState.Idle;
    
    public ParticleSystem LVPParticle;
    public TrailRenderer[] ThunderTrail;

    private bool _isAttackPressed;

    public AbilityID[] ActiveSkills;

    private UI_Ability uiAbility;
    
    //UI맵핑을 위한 이벤트
    public event Action OnGetActiveSKill;
    public  event Action<int, float> OnUseActiveSKill;
    
    private float[] _lastSkillTime = new  float[5]{-999f,-999f,-999f,-999f,-999f};
    private void Awake()
    {
        _movement = GetComponent<PlayerMovement>();
        _combat = GetComponent<PlayerCombat>();
        _interaction= GetComponentInChildren<PlayerInteraction>();
        _animator = GetComponent<Animator>();
        _attackcollider= GetComponentInChildren<SphereCollider>();
        
        _attackcollider.enabled = false;
        
        ActiveSkills = new AbilityID[4]
        {
            AbilityID.None,
            AbilityID.None,
            AbilityID.None,
            AbilityID.None,
        };
    }

    private void Start()
    {
        //기본 공격 이벤트 구독
        Managers.Input.OnAttack -= HandleAttackInput;
        Managers.Input.OnAttack += HandleAttackInput;
        
        //대쉬 이벤트 구독
        Managers.Input.OnDash -= HandleDashInput;
        Managers.Input.OnDash += HandleDashInput;
        
        // 스킬 이벤트 구독
        Managers.Input.OnSkill1 += () => HandleSkillInput(1);
        Managers.Input.OnSkill2 += () => HandleSkillInput(2);
        Managers.Input.OnSkill3 += () => HandleSkillInput(3);
        Managers.Input.OnSkill4 += () => HandleSkillInput(4);
        
    }
    private void FixedUpdate()
    {
        _inputVector = Managers.Input.GetMoveInput();
        
        Vector3 moveDir = CalculateCameraDirection();
        _movement.Move(_inputVector, moveDir);
        
        if (Managers.Input.IsAttackPressed && _combat.CanAttack)
        {
            _combat.AddBuffer("Attack");
        }
        
        _combat.ProcessBuffer();
        
        UpdateAnimation();


    }

    private Vector3 CalculateCameraDirection()
    {
        Vector3 forward = Camera.main.transform.forward;
        Vector3 right = Camera.main.transform.right;
        forward.y = 0; right.y = 0;
        return (forward * _inputVector.y + right * _inputVector.x).normalized;
    }
    

    private void HandleAttackInput()
    {
        // 누른 "순간" 즉시 첫 공격이 나가도록 처리
        if (_combat.CanAttack)
        {
            _combat.AddBuffer("Attack");
        }
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
    private void HandleDashInput()
    {
        float dashCoolDown = Managers.Player.Data.dashCooldown.TotalValue;
        if (!_movement.CanMove||Time.time<_lastSkillTime[0]+dashCoolDown) return;
        _lastSkillTime[0] = Time.time;
        OnUseActiveSKill?.Invoke(0,dashCoolDown);
        _movement.ExecuteDash(CalculateCameraDirection(), () => {
            _combat.ClearBuffer();
            _combat.ResetCombo();
            _animator.SetTrigger("QUICK SHIFT F");
            _movement.CanMove = false;
            _combat.CanAttack = false;
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

    
    //애니메이션 이벤트 함수
    public void CheckCombo()
    {
        _combat.CanAttack = true;
        StopDashPhysics();
        _attackcollider.enabled = false;
    }

    public void PlayAttack(int index)
    {
        _combat.PlayAttackEffect(index);
        _attackcollider.enabled = true;
        
    }
    public void StopDashPhysics() => _movement.StopVelocity();

    public void InputActive(bool isActive)
    {
        _movement.CanMove = isActive; 
        _combat.CanAttack = isActive;
        _interaction.CanInteract = isActive;
    }
}