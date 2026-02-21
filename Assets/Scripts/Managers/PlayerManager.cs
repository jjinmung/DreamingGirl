using UnityEngine;
using System;
using System.Collections;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
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
    private PlayerCombat _playerCombat;
    private Animator _playerAnim;
    private PlayerController _playerController;
    private Rigidbody _playerRb;
    private CapsuleCollider _playerCollider;
    private UI_PlayerHPBar _playerHpBar;
    private bool _isdeath;

    // 프로퍼티 (Null 체크 없이 즉시 반환하도록 개선)
    public PlayerUnit Unit=>_playerUnit;
    public PlayerCombat Combat=>_playerCombat;
    public Transform Trans => _playerUnit.transform;
    public Animator Anim => _playerAnim;
    public PlayerController Control => _playerController;
    public bool IsDeath => _isdeath;
    
    
    
    public async UniTask<GameObject> CreatePlayer()
    {
        data = new PlayerData(Managers.Data.PlayerBasicStat[1],Managers.Data.SaveData.player);
        var player = Managers.Resource.Data.Player;
        var playerPrefab = await Managers.Resource.InstantiateAsync(player);
        

        // 생성 시점에 모든 컴포넌트를 한 번만 캐싱
        _playerUnit = playerPrefab.GetComponent<PlayerUnit>();
        _playerCombat = playerPrefab.GetComponent<PlayerCombat>();
        _playerAnim = playerPrefab.GetComponent<Animator>();
        _playerController = playerPrefab.GetComponent<PlayerController>();
        _playerRb = playerPrefab.GetComponent<Rigidbody>();
        _playerCollider = playerPrefab.GetComponent<CapsuleCollider>();
        
        _playerHpBar = _playerUnit.GetComponentInChildren<UI_PlayerHPBar>(true);
        _playerHpBar.Init();
        
        

        _playerAnim.SetFloat("AttackSpeed", data.attackSpeed.TotalValue);

        SubscribeEvent();
        
        PlayerInit();
        
        
        return playerPrefab;
    }

    public void PlayerInit()
    {
        _isdeath = false;
        _playerAnim.SetTrigger("LIVE");
        LevelReset();
        data.currentHp = data.maxHp.TotalValue;
        _playerHpBar.SetMaxHP(data.maxHp.TotalValue,data.currentHp);
        _playerUnit.transform.position = data.position;
        _playerHpBar.gameObject.SetActive(false);
        for(int i=0; i< _playerController.ActiveSkills.Length; i++)
        {
            _playerController.ActiveSkills[i] = AbilityID.None;
        }
        _playerController.PactAbyssParticle.Stop();
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
        if(_isdeath) return;
        data.currentHp = Mathf.Clamp(data.currentHp - damage, 0, data.maxHp.TotalValue);
        TakeDamageAction?.Invoke(damage);
        if (data.currentHp <= 0)
        {
            _isdeath = true;
            Die();
        }
        if (_playerCombat.IsPactAbyss)
        {
            AdjustPactAbyss();
        }
    }

    private void Die()
    {
        Managers.Sound.PlayBgm(Managers.Resource.Data.DeathBGM).Forget();
        _playerAnim.SetTrigger("DIE");
        Control.InputActive(false);
        Control.AllEffectsFinished();
        Control.StopDashPhysics();
        DieAcation?.Invoke();
    } 

    public void AddGold(int amount)
    {
        Managers.Sound.PlayEffect(Managers.Resource.Data.Gold).Forget();
        data.gold += amount;
        OnDataChanged?.Invoke();
        //데이터 세이브
        Managers.Data.SaveData.player.gold += amount;
        Managers.Data.SaveGame();
        
        if(amount>0)
            Managers.UI.ShowFloatingText(Managers.Player.Trans.position, $"+{amount}gold", Color.yellow, 1.5f).Forget();
        else
            Managers.UI.ShowFloatingText(Managers.Player.Trans.position, $"-{amount}gold", Color.brown, 1.5f).Forget();
        
    }
    

    public void AddExp(int amount)
    {
        OnLevelUp?.Invoke(amount);
    }

    public void AdjustPactAbyss()
    {
        if (data.currentHp / data.maxHp.TotalValue <= _playerCombat.ParctAyssStartRatio)
        {
            if(_playerController.PactAbyssParticle.isPlaying) return;
            _playerController.PactAbyssParticle.Play();
            AddPermanentStat(PlayerStat.Attack,_playerCombat.ParctAyssAttackRatio,true);
        }
        else
        {
            if(!_playerController.PactAbyssParticle.isPlaying) return;
            _playerController.PactAbyssParticle.Stop();
            AddPermanentStat(PlayerStat.Attack,-_playerCombat.ParctAyssAttackRatio,true);
        }
    }

    public void LevelUp()
    {
        Managers.Sound.PlayEffect(Managers.Resource.Data.LevelUp).Forget();
        
        AddPermanentStat(PlayerStat.MaxHP, 0.1f, true);
        AddPermanentStat(PlayerStat.Attack, 0.1f, true);
        

        SelectAbilityAsync().Forget();
    }

    public void LevelReset()
    {
        data.level = 1;
        data.currentExp = 0;
        data.nextLevelExp = 100;
        data.maxHp.flatBonus = 0;
        data.maxHp.percentBonus = 0;
        data.damage.flatBonus = 0;
        data.damage.percentBonus = 0;
    }

    private async UniTask SelectAbilityAsync()
    {
        try
        {
            // 1. 레벨업 효과 재생
            _playerController.LVPParticle.Play();
            Managers.UI.ShowFloatingText(Trans.position, "Level UP!", Color.yellow, 1.5f, 60).Forget();

            // 2. 1.5초 대기 
            await UniTask.Delay(TimeSpan.FromSeconds(1.5f));

            // 3. 능력치 선택 팝업 로드 및 생성
            await Managers.UI.ShowPopupUI<UI_Ability>();

            // 4. 시간 정지
            Time.timeScale = 0f;
            
        }
        catch (OperationCanceledException)
        {
            
        }
        catch (Exception e)
        {
            Debug.LogError($"SelectAbility Error: {e.Message}");
        }
    }
    
    
    public void AddPermanentStat(PlayerStat type, float amount, bool isPercent = false)
    {
        var percentHp = data.currentHp / data.maxHp.TotalValue;

        Stat targetStat = GetStat(type);
        if (targetStat == null) return;

        if (isPercent) targetStat.percentBonus += amount;
        else targetStat.flatBonus += amount;

        // 공격 속도일 경우 애니메이터 즉시 갱신
        if (type == PlayerStat.attackSpeed)
            _playerAnim.SetFloat("AttackSpeed", data.attackSpeed.TotalValue);
        if (type == PlayerStat.MaxHP)
        {
            data.currentHp = data.maxHp.TotalValue*percentHp;
            _playerHpBar.SetMaxHP(data.maxHp.TotalValue,data.currentHp);
            if(!isPercent&&amount>0)
                Managers.UI.ShowFloatingText(Trans.position, $"+{amount}", Color.blue,1.5f,60).Forget();
        }

        if (type == PlayerStat.Attack)
        {
            if(!isPercent&&amount>0)
                Managers.UI.ShowFloatingText(Trans.position, $"+{amount}", Color.magenta,1.5f,60).Forget();
        }
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
        if (data.currentHp < data.maxHp.TotalValue)
        {
            _playerController.HealParticle.Play();
            var healamount = data.currentHp+amount <=data.maxHp.TotalValue? amount:data.maxHp.TotalValue- data.currentHp;
            data.currentHp = Mathf.Clamp(data.currentHp + healamount, 0, data.maxHp.TotalValue);
            TakeDamageAction?.Invoke(-healamount); // 기존 로직 유지
            Managers.UI.ShowFloatingText(Trans.position, $"+{Mathf.RoundToInt(healamount)}", Color.green, 1f).Forget();
            
            if (_playerCombat.IsPactAbyss)
            {
                AdjustPactAbyss();
            }
        }

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
    public void SetPlayerActiveState(bool isActive)
    {
        _playerCollider.enabled = isActive;
        _playerRb.useGravity = isActive;
        _playerHpBar.gameObject.SetActive(isActive);
        _playerController.InputActive(isActive);
        _playerController.DivineOrbs.gameObject.SetActive(isActive);
        if (isActive)
        {
            _playerController.gameObject.SetLayerRecursively("Char");
            
        }
        else
        {
            _playerController.StopDashPhysics();
            _playerController.gameObject.SetLayerRecursively("Default");
            _playerController.CurrentState = PlayerController.PlayerState.Idle;
            _playerAnim.SetFloat("MOVE", 0);
        }
    }

    public void BossClearControl(bool isActive)
    {
        _playerController.InputActive(isActive);
        if (!isActive)
        {
            _playerController.StopDashPhysics();
            _playerController.CurrentState = PlayerController.PlayerState.Idle;
            _playerAnim.SetFloat("MOVE", 0);
        }
        else
        {
            PlayerInit();
        }
    }
    public void FadeMoveFloat(float targetValue, float duration = 0.5f)
    {
        UpdateAnimFloatAsync(targetValue, duration).Forget();
    }

    private async UniTask UpdateAnimFloatAsync(float targetValue, float duration)
    {
        float startValue = _playerAnim.GetFloat("MOVE");
        float elapsedTime = 0f;

        // duration이 0일 경우 바로 목표값 설정 후 종료
        if (duration <= 0)
        {
            _playerAnim.SetFloat("MOVE", targetValue);
            return;
        }

        while (elapsedTime < duration)
        {

            elapsedTime += Time.deltaTime;
            float nextValue = Mathf.Lerp(startValue, targetValue, elapsedTime / duration);
    
            _playerAnim.SetFloat("MOVE", nextValue);

            // yield return null 대신 유니티 업데이트 루프의 다음 프레임까지 대기
            await UniTask.Yield(PlayerLoopTiming.Update);
        }

        // 마지막에 정확한 목표값으로 설정
        _playerAnim.SetFloat("MOVE", targetValue);
    }
    
}