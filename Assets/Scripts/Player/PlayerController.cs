using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerController : MonoBehaviour
{
    private PlayerMovement _movement;
    private PlayerCombat _combat;
    
    private Animator _animator;
    private Vector2 _inputVector;
    public enum PlayerState { Idle, Run,Attack,Dash}
    public PlayerState CurrentState = PlayerState.Idle;

    private bool _isAttackPressed;
    private void Awake()
    {
        _movement = GetComponent<PlayerMovement>();
        _combat = GetComponent<PlayerCombat>();
        _animator = GetComponent<Animator>();
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
    public void OnAnimationFinished() { 
        _movement.CanMove = true; 
        _combat.CanAttack = true;
        _combat.ResetCombo();
    }
    public void OnAnimationStart() { 
        _movement.CanMove = false; 
        _combat.CanAttack = false;
    }
    
    //애니메이션 이벤트 함수
    public void CheckCombo()
    {
        _combat.CanAttack = true;
        StopDashPhysics();
    }
    public void StopDashPhysics() => _movement.StopVelocity();
    
    public void ChangeLayer(string layer) 
    {
        int targetLayer = LayerMask.NameToLayer(layer);

        // 만약 레이어 설정에 "char"가 없다면 경고를 띄웁니다.
        if (targetLayer == -1)
        {
            Debug.LogWarning("'char' 레이어가 존재하지 않습니다. 레이어 설정을 확인하세요!");
            return;
        }
        
        Transform[] allChildren = GetComponentsInChildren<Transform>(true);

        foreach (Transform child in allChildren)
        {
            if(child.gameObject.CompareTag("Attack")) continue;
            child.gameObject.layer = targetLayer;
        }
    }
    
}