using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using Action = System.Action;

public abstract class EnemyBase : MonoBehaviour,IDamageable
{
    //lazy cashing
    protected GameObject _player=> _playerCache ??= Managers.Player.Control.gameObject;
    
    protected BehaviorGraphAgent _behavior=> _behaviorCache ??= GetComponent<BehaviorGraphAgent>();
    protected Animator _animator=> _animatorCache ??= GetComponent<Animator>();
    protected NavMeshAgent _navMeshAgent=> _navMeshAgentCache ??= GetComponent<NavMeshAgent>();
    protected SkinnedMeshRenderer _skinnedMesh=> _skinnedMeshCache ??= GetComponentInChildren<SkinnedMeshRenderer>();
    protected Rigidbody _rigidbody=> _rigidbodyCache ??= GetComponent<Rigidbody>();
    public UI_EnemyHPBar EnemyHpBar => _hpbarCashe ??= GetComponentInChildren<UI_EnemyHPBar>();

    private GameObject _playerCache;
    private BehaviorGraphAgent _behaviorCache;
    private Animator _animatorCache;
    private NavMeshAgent _navMeshAgentCache;
    private SkinnedMeshRenderer _skinnedMeshCache;
    private Rigidbody _rigidbodyCache;
    protected UI_EnemyHPBar _hpbarCashe;
    
    public EnemyStat stat; 
    public bool IsAttack=false;
    public bool isDead=false;
    [Header("이펙트")]
    [SerializeField]private ParticleSystem HitParticle;
    [SerializeField]private ParticleSystem fireParticle;
    [SerializeField]private ParticleSystem IceParticle;
    
    //디버프 코루틴
    Coroutine _burnCoroutine;
    Coroutine _freezeCoroutine;
    
    //화상 쿨타임
    private float lastBurn = -999f;
    private float burnCooldown=5f;
    public event Action<float> takeDamageAction; //데미지 받았을 때 실행할 이벤트
    public event Action dieAcation;
    [Header("MAT")]
    [SerializeField]private Material _originalMat;
    [SerializeField]private Material _hitMat;
    [SerializeField]private Material _deathMat;

    
    public virtual void Init(int id)
    {
        // 데이터 로드
        stat = new EnemyStat(Managers.Data.MonsterDict[id]);
        isDead = false;

        // Behavior 트리 설정
        _behavior.Restart();
        _behavior.SetVariableValue("Target", _player);
        _behavior.SetVariableValue("IsDeath", false);
        _behavior.SetVariableValue("AttackDelay", stat.AttackDelay);
        _behavior.SetVariableValue("Speed", stat.Speed);
        _behavior.SetVariableValue("IsAttack", IsAttack);
        // 이벤트 클린업 및 등록
        ResetEvents();

        //이름 변경
        name = stat.Name;
        
        //체력바 비활성화
        if(EnemyHpBar!=null)
            EnemyHpBar.gameObject.SetActive(false);
    }

    private void ResetEvents()
    {
        takeDamageAction = null;
        dieAcation = null;
        takeDamageAction += TakeDamageHandler;
        dieAcation += DieHandler;
    }


    public abstract void Attack();

    public void TakeDamage(float damage, Color color= default,bool isRandom =false)
    {
        if (isDead) return;
        stat.currentHp = Mathf.Clamp(stat.currentHp - damage, 0, stat.MaxHp);
        var col = color == default ? Color.white : color;
        if(color ==default)
            Managers.UI.ShowFloatingText(transform.position,$"-{(int)damage}",col,1f);
        else
            Managers.UI.ShowFloatingText(transform.position,$"-{(int)damage}",col,1f,40f,isRandom);
        
        if (stat.currentHp <= 0)
        {
            Die();
        }
       
        takeDamageAction.Invoke(damage);
    }

    #region 디버프 함수

    public void ApplyBurn(float playerDamage, float ratio, float duration)
    {
        if (isDead) return;
        if (burnCooldown + lastBurn > Time.time) return;
        
        lastBurn = Time.time;
        if(_burnCoroutine!=null)
            StopCoroutine(_burnCoroutine);
        _burnCoroutine =StartCoroutine(BurnRoutine(playerDamage * ratio, duration));
    }

    private IEnumerator BurnRoutine(float damagePerSecond, float duration)
    {
        if (fireParticle != null) fireParticle.Play();
        float elapsed = 0f;

        while (elapsed < duration)
        {
            
            // 초당 데미지 입힘 (Enemy01의 데미지 입는 함수 호출)
            TakeDamage(damagePerSecond,Color.red,true);
            
            yield return new WaitForSeconds(1f); // 1초 간격
            elapsed += 1f;
        }
        
        if (fireParticle != null) fireParticle.Stop();
    }
    
    public void ApplyFreeze(float playerDamage, float ratio, float duration)
    {
        if (isDead) return;
        if(_freezeCoroutine!=null)
            StopCoroutine(_freezeCoroutine);
        _freezeCoroutine =StartCoroutine(FreezeRoutine(playerDamage * ratio, duration));
    }

    private IEnumerator FreezeRoutine(float damagePerSecond, float duration)
    {
        if (fireParticle != null) IceParticle.Play();
        TakeDamage(damagePerSecond,Color.cyan,true);
        stat.Speed *= 0.5f;
        _behavior.SetVariableValue("Speed", stat.Speed);
        yield return new WaitForSeconds(duration); // 3초 
        stat.Speed *= 2f;
        _behavior.SetVariableValue("Speed", stat.Speed);
        if (fireParticle != null) IceParticle.Stop();
    }

    #endregion
    
    

    protected abstract void TakeDamageHandler(float damage);

    protected void Die()
    {
        //변수제어
        isDead = true;
        _behavior.SetVariableValue("IsDeath", true);
        
        if(_burnCoroutine!=null)
            StopCoroutine(_burnCoroutine);
        if(_freezeCoroutine!=null)
            StopCoroutine(_freezeCoroutine);
        //이펙트 종료
        HitParticle.Stop();
        fireParticle.Stop();
        IceParticle.Stop();
        
        //스테이지 관리
        Managers.Stage.CheckClear();
        
        //이벤트 호출
        dieAcation.Invoke();
        
        gameObject.layer = LayerMask.NameToLayer("DeadBody");
        _animator.SetTrigger("DEATH");
        
        
        Managers.Player.AddExp(stat.Exp);
        StartCoroutine(DelayDie());
    }

    protected abstract void DieHandler();

    protected void hitEffect()
    {
        StartCoroutine(DelayHitEffect());
    }

    IEnumerator DelayHitEffect()
    {
        HitParticle.Play();
        _skinnedMesh.material = _hitMat;
        yield return new WaitForSeconds(0.2f);
        _skinnedMesh.material = _originalMat;
    }
    IEnumerator DelayDie()
    {
        string transparentvalue = "_Tweak_transparency";
        yield return new WaitForSeconds(2f);
        _skinnedMesh.material=_deathMat;
        
        // 현재 값(0)에서 목표 값(-1)까지 4초 동안 변화시킴
        DOTween.To(() => _skinnedMesh.material.GetFloat(transparentvalue), 
                x => _skinnedMesh.material.SetFloat(transparentvalue, x), 
                -1f, 2f)
            .OnComplete(() => {
                _skinnedMesh.material.SetFloat(transparentvalue, 0f);
                _skinnedMesh.material=_originalMat;
                Managers.Resource.Destroy(gameObject);
            });
    }
    
    public virtual void SetAdditionalData(List<GameObject> patrolPoints) { }
}