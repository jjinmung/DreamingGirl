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
    protected GameObject _player=> _playerCache ??= Managers.Player.PlayerControl.gameObject;
    
    protected BehaviorGraphAgent _behavior=> _behaviorCache ??= GetComponent<BehaviorGraphAgent>();
    protected Animator _animator=> _animatorCache ??= GetComponent<Animator>();
    protected NavMeshAgent _navMeshAgent=> _navMeshAgentCache ??= GetComponent<NavMeshAgent>();
    protected SkinnedMeshRenderer _skinnedMesh=> _skinnedMeshCache ??= GetComponentInChildren<SkinnedMeshRenderer>();
    protected Rigidbody _rigidbody=> _rigidbodyCache ??= GetComponent<Rigidbody>();

    private GameObject _playerCache;
    private BehaviorGraphAgent _behaviorCache;
    private Animator _animatorCache;
    private NavMeshAgent _navMeshAgentCache;
    private SkinnedMeshRenderer _skinnedMeshCache;
    private Rigidbody _rigidbodyCache;
    
    public EnemyStat stat; 
    public bool IsAttack=false;
    public bool isDead=false;
    public event Action<float> takeDamageAction; //데미지 받았을 때 실행할 이벤트
    public event Action dieAcation;
    
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
    }

    private void ResetEvents()
    {
        takeDamageAction = null;
        dieAcation = null;
        takeDamageAction += TakeDamageHandler;
        dieAcation += DieHandler;
    }
    

    public abstract void Attack();

    public void TakeDamage(float damage)
    {
        if (isDead) return;
        Managers.UI.ShowFloatingText(transform.position,$"-{damage}",Color.white,false);
        stat.currentHp -= damage;
        
        if (stat.currentHp <= 0)
        {
            Die();
        }
       
        takeDamageAction.Invoke(damage);
    }

    protected abstract void TakeDamageHandler(float damage);

    protected void Die()
    {
        //변수제어
        isDead = true;
        _behavior.SetVariableValue("IsDeath", true);
        
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