using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Enemy05 : EnemyBase
{
    [SerializeField] private SphereCollider attackAttange;
    
    public override void Attack()
    {
        if(isDead) return;
        IsAttack = true;
        StartCoroutine(FadeLayerWeight(1, 1));
        stat.Speed *= 2f;
        _behavior.SetVariableValue("Speed", stat.Speed);
        attackAttange.enabled = true;
        
    }

    public void AttackFinished()
    {
        IsAttack = false;
        attackAttange.enabled = false;
        stat.Speed /= 2f;
        _behavior.SetVariableValue("Speed", stat.Speed);
        StartCoroutine(FadeLayerWeight(0, 1));
    }
    public IEnumerator FadeLayerWeight(float value, float duration)
    {
        float startWeight = _animator.GetLayerWeight(1);
        float elapsedTime = 0f;
        
        GetLight(value);
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            // 0에서 1까지의 비율을 계산 (0.0 ~ 1.0)
            float newWeight = Mathf.Lerp(startWeight, value, elapsedTime / duration);
            
            _animator.SetLayerWeight(1, newWeight);
            yield return null; // 다음 프레임까지 대기
        }

        // 마지막으로 목표치에 정확히 고정
        _animator.SetLayerWeight(1, value);
    }
    void GetLight(float value)
    {
        string rimlightPower = "_RimLight_Power";
        string highColorPower = "_HighColor_Power";
        string highMode = "_Is_SpecularToHighColor";
        // 현재 값(0)에서 목표 값까지 1초 동안 변화시킴
        DOTween.To(() => _skinnedMesh.material.GetFloat(rimlightPower),
            x => _skinnedMesh.material.SetFloat(rimlightPower, x),
            value, 1f);
        if (value >= 0.5f)
        {
            _skinnedMesh.material.SetFloat(highMode, 1);
            DOTween.To(() => _skinnedMesh.material.GetFloat(highColorPower),
                x => _skinnedMesh.material.SetFloat(highColorPower, x),
                0.6f, 1f);
        }
        else
        {
            _skinnedMesh.material.SetFloat(highMode, 0);
            DOTween.To(() => _skinnedMesh.material.GetFloat(highColorPower),
                x => _skinnedMesh.material.SetFloat(highColorPower, x),
                value, 1f);
        }
        
    }
    
    public override void TakeDamage(float damage, Color color = default, bool isRandom = false)
    {
        if (IsAttack)
        {
            Managers.UI.ShowFloatingText(transform.position,$"무적",Color.gray,1f).Forget();
        }
        else
        {
            base.TakeDamage(damage, color, isRandom);
        }
    }
    #region 이벤트 등록 함수
    protected override void TakeDamageHandler(float damage)
    {
        hitEffect();
        if (!IsAttack)
        {
            _navMeshAgent.isStopped = true;
            transform.LookAt(new Vector3(_player.transform.position.x, 0, _player.transform.position.z));
            Vector3 dashDirection = _player.transform.forward;
            _rigidbody.linearVelocity = dashDirection * 2f;
            _animator.SetTrigger("HIT");
            _animator.SetFloat("moveSpeed", 0);
        }
    }
    
    protected override void DieHandler()
    {
        Managers.Sound.PlayEffect(Managers.Resource.Data.Enemy05Die).Forget();
    }
    #endregion
    
    
    public override void SetAdditionalData(List<GameObject> patrolPoints) 
    {
        _behavior.SetVariableValue("PatrolPoints", patrolPoints);
    }
    
    #region 애니메이션 이벤트 함수
    public void HitFinish()
    {
        _navMeshAgent.isStopped = false;
    }

    public void ShootSound()
    {
        Managers.Sound.PlayEffect(Managers.Resource.Data.Enemy03BallShoot).Forget();
    }
    #endregion
   

    
}