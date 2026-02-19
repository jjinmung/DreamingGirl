using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_PlayerHPBar : UI_Base
{
    private float _currentHp;
    private float _maxHp;
    
    // 비동기 애니메이션 제어를 위한 토큰 소스
    private CancellationTokenSource _animCts;
    
    enum Texts
    {
        HPText
    }

    enum Sliders
    {
        MainBar,
        SubBar,
    }

    enum GameObjects
    {
        HPLines
    }

    public override void Init()
    {
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<Slider>(typeof(Sliders));
        Bind<GameObject>(typeof(GameObjects));
        
        // 이벤트 중복 구독 방지 및 등록
        Managers.Player.TakeDamageAction -= TakeDamage;
        Managers.Player.TakeDamageAction += TakeDamage;
        Managers.Player.DieAcation -= Destroy;
        Managers.Player.DieAcation += Destroy;
    }

    public void SetMaxHP(float maxHp, float currentHp)
    {
        _maxHp = maxHp;
        _currentHp = currentHp;
        
        float ratio = _currentHp / _maxHp;
        Get<Slider>((int)Sliders.MainBar).value = ratio;
        Get<Slider>((int)Sliders.SubBar).value = ratio;
        GetText((int)Texts.HPText).text = $"{Mathf.RoundToInt(_currentHp)}";

        // 체력바 가이드 라인 스케일 설정
        float scaleX = 1000f / _maxHp;
        var hpLinesTransform = Get<GameObject>((int)GameObjects.HPLines).transform;
        
        foreach (Transform child in hpLinesTransform)
        {
            child.localScale = new Vector3(scaleX, 1, 1);
        }

        // Layout UI 강제 갱신을 위한 Trick (필요 시)
        Get<GameObject>((int)GameObjects.HPLines).SetActive(false);
        Get<GameObject>((int)GameObjects.HPLines).SetActive(true);
    }
    
    public void TakeDamage(float damage)
    {
        _currentHp = Mathf.Max(0, _currentHp - damage);
        
        // 메인 바와 텍스트는 즉시 업데이트 (반응성)
        Get<Slider>((int)Sliders.MainBar).value = _currentHp / _maxHp;
        GetText((int)Texts.HPText).text = $"{Mathf.RoundToInt(_currentHp)}";

        // 1. 기존 애니메이션 취소 및 새 소스 생성
        if (_animCts != null)
        {
            _animCts.Cancel();
            _animCts.Dispose();
            _animCts = null; 
        }
        _animCts = new CancellationTokenSource();

        // 2. 비동기 잔상 애니메이션 실행
        if (_currentHp > 0)
        {
            SubBarAnimAsync(_animCts.Token).Forget();
        }
    }
    
    private async UniTaskVoid SubBarAnimAsync(CancellationToken token)
    {
        try
        {
            // 0.3초 대기 (데미지 체감 후 잔상이 따라오도록)
            await UniTask.Delay(TimeSpan.FromSeconds(0.3f), cancellationToken: token);

            Slider subSlider = Get<Slider>((int)Sliders.SubBar);
            float targetValue = _currentHp / _maxHp;
            
            await subSlider.DOValue(targetValue, 0.3f)
                .SetEase(Ease.OutQuad) // 서서히 느려지는 부드러운 효과
                .ToUniTask(cancellationToken: token);
        }
        catch (OperationCanceledException)
        {
            // 연속 데미지로 인한 취소 시 자연스럽게 종료
        }
        catch (Exception e)
        {
            Debug.LogError($"PlayerHP SubBar Error: {e.Message}");
        }
    }

    public void Destroy()
    {
        // 비동기 작업 중단
        _animCts?.Cancel();
        _animCts?.Dispose();
        _animCts = null;

        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        // Destroy()가 명시적으로 불리지 않았을 때를 대비한 2중 안전장치
        _animCts?.Cancel();
        _animCts?.Dispose();
    }
}