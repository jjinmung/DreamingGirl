using UnityEngine;



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

    private bool _isAttackPressed;
    private void Awake()
    {
        _movement = GetComponent<PlayerMovement>();
        _combat = GetComponent<PlayerCombat>();
        _interaction= GetComponentInChildren<PlayerInteraction>();
        _animator = GetComponent<Animator>();
        _attackcollider= GetComponentInChildren<SphereCollider>();
        
        _attackcollider.enabled = false;
    }

    private void Start()
    {
        Managers.Input.OnAttack -= HandleAttackInput;
        Managers.Input.OnAttack += HandleAttackInput;
        
        Managers.Input.OnDash -= HandleDashInput;
        Managers.Input.OnDash += HandleDashInput;
        
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

    private void HandleDashInput()
    {
        if (!_movement.CanMove) return;

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
    public void OnAnimationStart()
    {
        InputActive(false);
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