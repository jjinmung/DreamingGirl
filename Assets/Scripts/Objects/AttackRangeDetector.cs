using System;
using Cysharp.Threading.Tasks;
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
        FillArea.fillAmount = 0;

        // 시퀀스 생성
        Sequence seq = DOTween.Sequence();

        // 1. 게이지 채우기 (0초 지점부터 시작해서 2초 동안)
        seq.Append(FillArea.DOFillAmount(1f, 2f).SetEase(Ease.Linear));

        // 2. 1.5초 지점에 사운드 재생 삽입 (전체 타임라인 기준)
        seq.InsertCallback(1.5f, () => 
        {
            Managers.Sound.PlayEffect(Managers.Resource.Data.Enemy03Blast).Forget();
        });

        // 3. 전체 완료 후 실행될 로직
        seq.OnComplete(() => 
        {
            DetectTargets();
            Effect.SetActive(true);
            bgImage.gameObject.SetActive(false);
        
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