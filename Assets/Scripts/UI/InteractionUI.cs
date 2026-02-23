using DG.Tweening;
using UnityEngine;

public class InteractionUI : MonoBehaviour
{
    [SerializeField] private float animationTime = 0.6f;
    [SerializeField] private float scaleAmount = 1.15f;
    [SerializeField] private float moveAmount = 0.2f;

    void OnEnable()
    {
        transform.localScale = Vector3.one;
        // 1. 크기 애니메이션
        transform.DOScale(scaleAmount, animationTime)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);

        // 2. 위아래로 둥둥 떠다니는 애니메이션 (로컬 좌표 기준)
        transform.DOLocalMoveY(transform.localPosition.y + moveAmount, animationTime)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    // UI가 꺼질 때 애니메이션을 확실히 꺼주는 것이 좋습니다. (메모리 관리)
    void OnDisable()
    {
        transform.DOKill();
    }
}