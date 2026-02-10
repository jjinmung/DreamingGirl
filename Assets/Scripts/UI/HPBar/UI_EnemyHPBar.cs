using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening; // DOTween 네임스페이스 추가
using UnityEngine;
using UnityEngine.UI;

public class UI_EnemyHPBar : UI_Base
{
    private float _hp;
    private float _hpMax;
    private CancellationTokenSource _animCts;
    
    enum Sliders
    {
        MainBar,
        SubBar,
    }

    private void Awake()
    {
        Init();
    }

    public override void Init()
    {
        Bind<Slider>(typeof(Sliders));
    }
    
    public void SetMaxHP(float maxHp)
    {
        _hpMax = maxHp;
        _hp = maxHp;
        
        // 초기화 시에는 즉시 반영
        Get<Slider>((int)Sliders.MainBar).value = 1f;
        Get<Slider>((int)Sliders.SubBar).value = 1f;
    }
    
    public void TakeDamage(float damage)
    {
        _hp = Mathf.Max(0, _hp - damage);
        float targetRatio = _hp / _hpMax;
        
        // 메인 바는 즉시 갱신
        Get<Slider>((int)Sliders.MainBar).value = targetRatio;

        // 1. 기존 애니메이션(지연 및 트윈) 즉시 취소
        _animCts?.Cancel();
        _animCts?.Dispose();
        _animCts = new CancellationTokenSource();

        // 2. 체력이 남은 경우 잔상 애니메이션 실행
        if (_hp > 0)
        {
            SubBarAnimAsync(targetRatio, _animCts.Token).Forget();
        }
    }
    
    private async UniTaskVoid SubBarAnimAsync(float targetValue, CancellationToken token)
    {
        try
        {
            // 0.3초 대기 (데미지 직후 바로 깎이지 않고 약간 머무르는 연출)
            await UniTask.Delay(TimeSpan.FromSeconds(0.3f), cancellationToken: token);

            Slider subSlider = Get<Slider>((int)Sliders.SubBar);

            // 3. DOTween을 이용한 슬라이더 애니메이션
            await subSlider.DOValue(targetValue, 0.3f)
                .SetEase(Ease.OutQuad) // 서서히 느려지는 부드러운 효과
                .ToUniTask(cancellationToken: token);
        }
        catch (OperationCanceledException)
        {
            // 새로운 데미지가 들어와서 취소된 경우 아무것도 하지 않음
        }
        catch (Exception e)
        {
            Debug.LogError($"SubBarAnim Error: {e.Message}");
        }
    }

    private void OnDestroy()
    {
        _animCts?.Cancel();
        _animCts?.Dispose();
        _animCts = null;
    }

    public void Destroy()
    {
        Managers.Resource.Destroy(gameObject);
    }

    public void SetFalse()
    {
        gameObject.SetActive(false);
    }
}