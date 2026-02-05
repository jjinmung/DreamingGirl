using UnityEngine;
using TMPro;
using DG.Tweening;

public class UI_FloatingText : MonoBehaviour
{
    private TextMeshProUGUI _text;
    
    [Header("Settings")]
    [SerializeField] float _moveDistance = 1f;

    private void Awake()
    {
        _text = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void Init(Vector3 pos, string message, Color color,float duration,float size,bool isRandom)
    {
        // 1. 초기화 및 기존 트윈 제거 
        _text.DOKill();

        _text.text = message;
        _text.fontSize = size;
        _text.color = color;
        _text.alpha = 1f;
        var randomX = Random.Range(-0.5f, 0.5f);
        var randomYZ = Random.Range(-0.2f, -0.5f);
        
        var textPos =isRandom?pos+new Vector3(randomX,randomYZ,-randomYZ):pos;
        transform.position = textPos;
        // 2. 크리티컬 연출 추가
        if (isRandom)
        {
            // 통통 튀는 펀치 연출 (크기가 1.5배로 커졌다가 돌아옴)
            _text.transform.DOPunchScale(new Vector3(0.5f, 0.5f, 0.5f), 0.3f, 10, 1f);
        }
        // 3. 위로 이동 연출 
        transform.DOLocalMoveY(transform.localPosition.y + _moveDistance, duration)
            .SetEase(Ease.OutBack);

        // 4. 서서히 사라지기 (Ease.InSine 사용)
        _text.DOFade(0, duration)
            .SetEase(Ease.InSine)
            .OnComplete(() =>
            {
                Managers.Resource.Destroy(gameObject);
            });
    }


}