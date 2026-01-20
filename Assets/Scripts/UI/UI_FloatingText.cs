using UnityEngine;
using TMPro;
using DG.Tweening;

public class UI_FloatingText : MonoBehaviour
{
    private TextMeshProUGUI _text;
    
    [Header("Settings")]
    [SerializeField] float _moveDistance = 120f;
    [SerializeField] float _duration = 1.0f;

    private void Awake()
    {
        _text = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void Init(Vector3 pos, string message, Color color, bool isCritical = false)
    {
        // 1. 초기화 및 기존 트윈 제거 
        _text.DOKill();
        _text.transform.position = pos;

        _text.text = message;
        _text.color = color;
        _text.alpha = 1f;
        _text.transform.localScale = Vector3.one;
        _text.transform.position = pos;
        // 2. 크리티컬 연출 추가
        if (isCritical)
        {
            _text.color = Color.red; // 크리티컬은 강렬한 빨간색
            _text.fontWeight = FontWeight.Bold;
            
            // 통통 튀는 펀치 연출 (크기가 1.5배로 커졌다가 돌아옴)
            _text.transform.DOPunchScale(new Vector3(0.5f, 0.5f, 0.5f), 0.3f, 10, 1f);
        }
        else
        {
            _text.fontWeight = FontWeight.Regular;
        }

        // 3. 위로 이동 연출 (Ease.OutBack을 쓰면 살짝 위로 튀어올랐다 멈추는 느낌이 납니다)
        _text.transform.DOLocalMoveY(_text.transform.localPosition.y + _moveDistance, _duration)
            .SetEase(Ease.OutQuart);

        // 4. 서서히 사라지기 (Ease.InSine 사용)
        _text.DOFade(0, _duration)
            .SetEase(Ease.InSine)
            .OnComplete(() =>
            {
                Managers.Resource.Destroy(gameObject);
            });
    }


}