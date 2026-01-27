using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using static Define;

public class PlayerManager : MonoBehaviour
{
    [Header("Player Data")]
    [SerializeField]
    private PlayerData data;

    public PlayerData Data => data;
    public event Action OnDataChanged;
    public event Action<int> OnLevelUp;
    public event Action<float> TakeDamageAction;
    public Action<float> OnDamageDealt;
    public event Action DieAcation;

    // 캐싱용 필드
    private PlayerUnit _playerUnit;
    private Animator _playerAnim;
    private PlayerController _playerController;
    private Rigidbody _playerRb;
    private CapsuleCollider _playerCollider;
    private UI_PlayerHPBar _playerHpBar;

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
        
        _playerHpBar = _playerUnit.GetComponentInChildren<UI_PlayerHPBar>(true);
        _playerHpBar.Init();
        _playerHpBar.SetMaxHP(data.maxHp.TotalValue,data.currentHp);
        _playerHpBar.gameObject.SetActive(false);

        _playerAnim.SetFloat("AttackSpeed", data.attackSpeed.TotalValue);

        SubscribeEvent();

        
        
        return playerPrefab;
    }

    void SubscribeEvent()
    {
        // 중복 구독 방지 
        Managers.Stage.ExitRoom -= ExitRoomHandler;
        Managers.Stage.ExitRoom += ExitRoomHandler;

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
    

    public void AddExp(int amount)
    {
        OnLevelUp?.Invoke(amount);
    }
    

    public void LevelUp()
    {
        var percentHp = data.currentHp / data.maxHp.TotalValue;
        AddPermanentStat(PlayerStat.MaxHP, 0.1f, true);
        data.currentHp = data.maxHp.TotalValue*percentHp;
        _playerHpBar.SetMaxHP(data.maxHp.TotalValue,data.currentHp);

        StartCoroutine(SelectAbility());
    }

    IEnumerator SelectAbility()
    {
        _playerController.LVPParticle.Play();
        Managers.UI.ShowFloatingText(PlayerTrans.position, "Level UP!", Color.yellow,false,1.5f);
        yield return new WaitForSeconds(1.5f);
        Managers.UI.ShowPopupUI<UI_Ability>();
        //시간 정지
        Time.timeScale = 0;
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
        _playerHpBar.gameObject.SetActive(isActive);
        
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