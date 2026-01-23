using UnityEngine;
using System;

public class PlayerManager : MonoBehaviour
{
    
    [Header("Player Data")]
    public PlayerData data;

    // 데이터 변경 시 UI 업데이트 등을 위한 이벤트
    public event Action OnDataChanged;
    public event Action<float> takeDamageAction;
    public event Action dieAcation;
    
    //플레이어 관련 변수 
    private PlayerUnit player;
    public Transform playerTrans => player.transform;
    private Animator _playerAnim;
    public Animator PlayerAnim
    {
        get
        {
            if (_playerAnim == null)
                _playerAnim = player.GetComponent<Animator>();
            return _playerAnim;
        }
    }
    
    private GameObject playerHpBar;
    
    public GameObject CreatePlayer()
    {
        data=new PlayerData(Managers.Data.PlayerBasicStat[1]);
        var playerPrefab = Managers.Resource.Instantiate(Address.Player);
        playerPrefab.transform.position = Managers.Data.SaveData.player.position;
        player= playerPrefab.GetComponent<PlayerUnit>();
        _playerAnim=playerPrefab.GetComponent<Animator>();
        _playerAnim.SetFloat("AttackSpeed",data.attackSpeed.TotalValue);
        player.Init();
        
        
        playerHpBar = player.GetComponentInChildren<UI_PlayerHPBar>().gameObject;
        playerHpBar.SetActive(false);
        
        Managers.Stage.ExitRoom += ExitRoomHandler;
        Managers.Stage.ExitRoom += ExitRoomHandler;
        
        
        return playerPrefab;
    }
    
    // --- 데이터 수정 메소드들 ---

    public void TakeDamage(float damage)
    {
        data.currentHp -= damage;
        data.currentHp = Mathf.Clamp(data.currentHp, 0, data.maxHp.TotalValue);
        takeDamageAction.Invoke(damage);
        if (data.currentHp <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        dieAcation.Invoke();
    }

    public void AddGold(int amount)
    {
        data.gold += amount;
        OnDataChanged?.Invoke();
    }

    public void AddExp(int amount)
    {
        data.currentExp += amount;
        if (data.currentExp >= data.nextLevelExp)
        {
            LevelUp();
        }
        OnDataChanged?.Invoke();
    }

    private void LevelUp()
    {
        data.level++;
        data.currentExp -= data.nextLevelExp;
        data.nextLevelExp = Mathf.RoundToInt(data.nextLevelExp * 1.2f); // 레벨업 필요량 증가
        
        // 레벨업 시 풀피 회복 등 로직
        data.currentHp = data.maxHp.TotalValue;
        
        Debug.Log("Level Up! 현재 레벨: " + data.level);
        
    }
    
    public enum StatType { Attack, MaxHP, MoveSpeed, Critical,DashCooldown,attackSpeed }

    // 스탯을 영구적으로 강화하는 메소드 (레벨업 보상 등)
    public void AddPermanentStat(StatType type, float amount)
    {
        switch (type)
        {
            case StatType.Attack:
                data.damage.addValue += amount;
                break;
            case StatType.MaxHP:
                data.maxHp.addValue += amount;
                break;
            case StatType.MoveSpeed:
                data.moveSpeed.addValue += amount;
                break;
            case StatType.Critical:
                data.criticalChance.addValue += amount;
                break;
            case StatType.DashCooldown:
                data.dashCooldown.addValue += amount;
                break;
            case StatType.attackSpeed:
                data.attackSpeed.addValue += amount;
                _playerAnim.SetFloat("AttackSpeed", data.attackSpeed.TotalValue);
                break;
        }
        Debug.Log($"{type} 스탯이 {amount}만큼 증가했습니다! 현재: {GetStat(type).TotalValue}");
    }

    public Stat GetStat(StatType type)
    {
        return type switch
        {
            StatType.Attack => data.damage,
            StatType.MaxHP => data.maxHp,
            StatType.MoveSpeed => data.moveSpeed,
            StatType.Critical=>data.criticalChance,
            _ => null
        };
    }

    private void ExitRoomHandler()
    {
        player.GetComponent<CapsuleCollider>().enabled = false;
        player.GetComponent<Rigidbody>().useGravity = false;
        var controller = player.GetComponent<PlayerController>();
        controller.StopDashPhysics();
        controller.gameObject.SetLayerRecursively("Default");
        controller.CurrentState = PlayerController.PlayerState.Idle;
        controller.enabled = false;
        
        _playerAnim.SetFloat("MOVE",0);
        playerHpBar.SetActive(false);
    }
    
    public void EnterRoom()
    {
        player.GetComponent<CapsuleCollider>().enabled = true;
        player.GetComponent<Rigidbody>().useGravity = true;
        player.GetComponent<PlayerController>().gameObject.SetLayerRecursively("Char");
        player.GetComponent<PlayerController>().enabled = true;
        playerHpBar.SetActive(true);
    }
    
}