using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class InputManager : MonoBehaviour
{
    // Generate된 C# 클래스
    private PlayerAction _playerAction;

    // 이벤트 방식으로 처리할 액션들
    public event Action OnAttack;
    public event Action OnDash;
    public event Action OnChangeCamera;
    public event Action OnInteract;
    public event Action OnSkill1;
    public event Action OnSkill2;
    public event Action OnSkill3;
    public event Action OnSkill4;
    public bool IsAttackPressed { get; private set; }
    private void Awake()
    {
        _playerAction = new PlayerAction();
    }
    private void OnEnable()
    {
        _playerAction.Enable();
        
        _playerAction.Player.Attack.started += ctx => {
            IsAttackPressed = true;
            OnAttack?.Invoke();
        };
        _playerAction.Player.Attack.canceled += ctx => {
            IsAttackPressed = false;
        };
        
        _playerAction.Player.Dash.performed += ctx => OnDash?.Invoke();
        
        _playerAction.Player.ChangeCamera.performed += ctx => OnChangeCamera?.Invoke();
        
        _playerAction.Player.Interact.performed += ctx => OnInteract?.Invoke();
        
        _playerAction.Player.Skill1.performed += ctx => OnSkill1?.Invoke();
        _playerAction.Player.Skill2.performed += ctx => OnSkill2?.Invoke();
        _playerAction.Player.Skill3.performed += ctx => OnSkill3?.Invoke();
        _playerAction.Player.Skill4.performed += ctx => OnSkill4?.Invoke();
        
        
    }

    private void OnDisable()
    {
        _playerAction.Disable();
    }

    // Move 처럼 계속 값을 읽어야 하는 경우 (Getter 방식)
    public Vector2 GetMoveInput()
    {
        return _playerAction.Player.Move.ReadValue<Vector2>();
    }
}