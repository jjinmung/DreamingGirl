using UnityEngine;
using System;
using System.Collections.Generic;
using static Define;

public class PlayerManager : MonoBehaviour
{
    [Header("Player Data")]
    private PlayerData data;

    public event Action OnDataChanged;
    public event Action<float> TakeDamageAction;
    public Action<float> OnDamageDealt;
    public event Action DieAcation;

    // 캐싱용 필드
    private PlayerUnit _playerUnit;
    private Animator _playerAnim;
    private PlayerController _playerController;
    private Rigidbody _playerRb;
    private CapsuleCollider _playerCollider;
    private GameObject _playerHpBar;

    // 프로퍼티 (Null 체크 없이 즉시 반환하도록 개선)
    public Transform PlayerTrans => _playerUnit.transform;
    public Animator PlayerAnim => _playerAnim;
    public PlayerController PlayerControl => _playerController;

    
    
    public GameObject CreatePlayer()
    {
        data = new PlayerData(Managers.Data.PlayerBasicStat[1],Managers.Data.SaveData.player);
        var playerPrefab = Managers.Resource.Instantiate(Address.Player);
        playerPrefab.transform.position = data.position;

        // 생성 시점에 모든 컴포넌트를 한 번만 캐싱
        _playerUnit = playerPrefab.GetComponent<PlayerUnit>();
        _playerAnim = playerPrefab.GetComponent<Animator>();
        _playerController = playerPrefab.GetComponent<PlayerController>();
        _playerRb = playerPrefab.GetComponent<Rigidbody>();
        _playerCollider = playerPrefab.GetComponent<CapsuleCollider>();
        
        _playerHpBar = _playerUnit.GetComponentInChildren<UI_PlayerHPBar>(true).gameObject;
        

        _playerAnim.SetFloat("AttackSpeed", data.attackSpeed.TotalValue);
        _playerUnit.Init();

        // 중복 구독 방지 (이미 등록되어 있을 수 있으므로 -= 후 +=)
        Managers.Stage.ExitRoom -= ExitRoomHandler;
        Managers.Stage.ExitRoom += ExitRoomHandler;

        return playerPrefab;
    }

    // --- 데이터 수정 메소드들 ---
    public void TakeDamage(float damage)
    {
        data.currentHp = Mathf.Clamp(data.currentHp - damage, 0, data.maxHp.TotalValue);
        TakeDamageAction?.Invoke(damage);
        if (data.currentHp <= 0) Die();
    }

    public void Die() => DieAcation?.Invoke();

    public void AddGold(int amount)
    {
        data.gold += amount;
        OnDataChanged?.Invoke();
    }
    
    public int GetGold()
    {
        return data != null ? data.gold : 0;
    }

    public void AddExp(int amount)
    {
        data.currentExp += amount;
        while (data.currentExp >= data.nextLevelExp) // if 대신 while 사용 (연속 레벨업 대응)
        {
            LevelUp();
        }
        OnDataChanged?.Invoke();
    }

    private void LevelUp()
    {
        data.level++;
        data.currentExp -= data.nextLevelExp;
        data.nextLevelExp = Mathf.RoundToInt(data.nextLevelExp * 1.2f);
        data.currentHp = data.maxHp.TotalValue;
        Debug.Log($"Level Up! 현재 레벨: {data.level}");
    }

    public void AddPermanentStat(PlayerStat type, float amount, bool isPercent = false)
    {
        Stat targetStat = GetStat(type);
        if (targetStat == null) return;

        if (isPercent) targetStat.percentBonus += amount;
        else targetStat.flatBonus += amount;

        // 공격 속도일 경우 애니메이터 즉시 갱신
        if (type == PlayerStat.attackSpeed)
            _playerAnim.SetFloat("AttackSpeed", data.attackSpeed.TotalValue);
    }

    public Stat GetStat(PlayerStat type)
    {
        return type switch
        {
            PlayerStat.Attack => data.damage,
            PlayerStat.MaxHP => data.maxHp,
            PlayerStat.MoveSpeed => data.moveSpeed,
            PlayerStat.Critical => data.criticalChance,
            PlayerStat.DashCooldown => data.dashCooldown,
            PlayerStat.attackSpeed => data.attackSpeed,
            _ => null
        };
    }

    public void Heal(float amount)
    {
        data.currentHp = Mathf.Clamp(data.currentHp + amount, 0, data.maxHp.TotalValue);
        TakeDamageAction?.Invoke(-amount); // 기존 로직 유지
        Managers.UI.ShowFloatingText(PlayerTrans.position, $"+{amount}", Color.green, false);
    }

    // --- 상태 제어 메소드 (클린 코드) ---

    private void ExitRoomHandler()
    {
        SetPlayerActiveState(false);
    }

    public void EnterRoom()
    {
        SetPlayerActiveState(true);
    }

    /// <summary>
    /// 플레이어의 물리 및 컨트롤러 상태를 일괄 제어
    /// </summary>
    private void SetPlayerActiveState(bool isActive)
    {
        _playerCollider.enabled = isActive;
        _playerRb.useGravity = isActive;
        _playerHpBar.SetActive(isActive);
        
        if (isActive)
        {
            _playerController.gameObject.SetLayerRecursively("Char");
            _playerController.InputActive(true);
        }
        else
        {
            _playerController.StopDashPhysics();
            _playerController.gameObject.SetLayerRecursively("Default");
            _playerController.CurrentState = PlayerController.PlayerState.Idle;
            _playerAnim.SetFloat("MOVE", 0);
            _playerController.InputActive(false);
        }
    }
    
}