using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class AttackRangeDetector : MonoBehaviour
{
    [Header("Range Settings")]
    public float maxRadius = 6.22f;    // 최대 반지름
    public float minRadius = 2.5f;     // 최소 반지름 (도넛 형태가 아닐 경우 0)
    [Range(0, 360)]
    public float sectorAngle = 92f;    // 부채꼴 중심각

    [Header("Target Settings")]
    public LayerMask targetLayer;      // 감지할 레이어 (예: Enemy 또는 Char)

    public Image bgImage;
    public Image FillArea;

    public GameObject Effect;

    private EnemyBase enemy;
    private float power =2.3f;
    
    // 범위를 확인하기 위한 Gizmos 그리기
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 1, 0, 0.3f); // 투명한 빨간색
        
        Vector3 forward = transform.forward;
        Vector3 origin = transform.position;

        // 부채꼴의 왼쪽 끝과 오른쪽 끝 방향 계산
        Vector3 leftBoundary = Quaternion.Euler(0, -sectorAngle * 0.5f, 0) * forward;
        Vector3 rightBoundary = Quaternion.Euler(0, sectorAngle * 0.5f, 0) * forward;

        // Gizmos로 부채꼴 그리기 (Line)
        Gizmos.DrawLine(origin + leftBoundary * minRadius, origin + leftBoundary * maxRadius);
        Gizmos.DrawLine(origin + rightBoundary * minRadius, origin + rightBoundary * maxRadius);

        // 호(Arc)를 그리기 위한 간단한 루프 (Gizmos에는 Arc 함수가 없으므로 Handles 권장)
        #if UNITY_EDITOR
        UnityEditor.Handles.color = new Color(1, 1, 0, 0.1f);
        UnityEditor.Handles.DrawSolidArc(origin, Vector3.up, leftBoundary, sectorAngle, maxRadius);
        
        if (minRadius > 0)
        {
            UnityEditor.Handles.color = Color.black; // 안쪽 제외 범위 표시용
            UnityEditor.Handles.DrawWireArc(origin, Vector3.up, leftBoundary, sectorAngle, minRadius);
        }
        #endif
    }


    private void OnEnable()
    {
        bgImage.gameObject.SetActive(true);
        // 1. 초기화: 게이지를 0으로 설정
        FillArea.fillAmount = 0;

        // 2. DOTween을 사용하여 2초 동안 1로 변경
        // .SetEase(Ease.Linear)를 추가하면 끊김 없이 일정한 속도로 차오릅니다.
        FillArea.DOFillAmount(1f, 2f)
            .SetEase(Ease.Linear) 
            .OnComplete(() => 
            {
                // 3. 애니메이션이 끝나는 순간 공격 판정 실행
                DetectTargets();
                Effect.SetActive(true);
                bgImage.gameObject.SetActive(false);
                // 4. 다시 초기화
                FillArea.fillAmount = 0;
                DOVirtual.DelayedCall(1f, () => Effect.SetActive(false));
            });
    }

    public void DetectTargets()
    {
        Collider[] targets = Physics.OverlapSphere(transform.position, maxRadius, targetLayer);

        // 거리 비교를 위한 제곱값 미리 계산 (성능 최적화)
        float maxSqr = maxRadius * maxRadius;
        float minSqr = minRadius * minRadius;

        foreach (var target in targets)
        {
            Vector3 directionToTarget = target.transform.position - transform.position;

            // Y축 차이를 무시하여 평면상의 거리/각도만 계산
            directionToTarget.y = 0;

            float sqrDistance = directionToTarget.sqrMagnitude;

            // 1. 거리 조건 체크 (제곱값 비교로 루프 연산 속도 향상)
            if (sqrDistance >= minSqr && sqrDistance <= maxSqr)
            {
                // 2. 각도 조건 체크
                float angle = Vector3.Angle(transform.forward, directionToTarget.normalized);

                if (angle <= sectorAngle * 0.5f)
                {
                    if (target.CompareTag("Player"))
                    {
                        if (enemy == null)
                            enemy = GetComponentInParent<EnemyBase>();
                        target.GetComponent<IDamageable>().TakeDamage(enemy.stat.Damage*power);
                    }
                }
            }
        }
    }
}