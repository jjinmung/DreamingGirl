using System;
using System.Collections;
using DG.Tweening;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using Action = System.Action;

public abstract class EnemyBase : MonoBehaviour,IDamageable
{
    protected GameObject _player;
    protected BehaviorGraphAgent _behavior;
    protected Animator _animator;
    protected NavMeshAgent _navMeshAgent;
    protected SkinnedMeshRenderer _skinnedMesh;
    protected Rigidbody _rigidbody;
    
    protected int ID;
    public EnemyStat stat; 
    public bool IsAttack=false;
    public bool isDead=false;
    public event Action<float> takeDamageAction; //데미지 받았을 때 실행할 이벤트
    public event Action dieAcation;
    
    [SerializeField]private Material _originalMat;
    [SerializeField]private Material _hitMat;
    [SerializeField]private Material _deathMat;
    public virtual void Init()
    {
        stat=new EnemyStat(Managers.Data.MonsterDict[1]);
        _player = GameObject.FindGameObjectWithTag("Player");
        _behavior = GetComponent<BehaviorGraphAgent>();
        _animator = GetComponent<Animator>();
        _navMeshAgent =  GetComponent<NavMeshAgent>();
        _skinnedMesh = GetComponentInChildren<SkinnedMeshRenderer>();
        _rigidbody = GetComponent<Rigidbody>();
        
        gameObject.layer = LayerMask.NameToLayer("Enemy");
        
        _behavior.Restart();
        _behavior.SetVariableValue("Target",_player);
        _behavior.SetVariableValue("IsDeath",false);
        
        takeDamageAction -= TakeDamageHandler;
        takeDamageAction += TakeDamageHandler;
        dieAcation -= DieHandler;
        dieAcation += DieHandler;

        isDead = false;
    }

    private void OnEnable()
    {
        isDead = false;
        stat.currentHp = stat.MaxHp;
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
        isDead = true;
        dieAcation.Invoke();
        gameObject.layer = LayerMask.NameToLayer("DeadBody");
        _animator.SetTrigger("DEATH");
        _behavior.SetVariableValue("IsDeath", true);

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
}