using DG.Tweening;
using UnityEngine;

public class CircleAttackRange : MonoBehaviour
{
    public Transform circle;
    private float _damage;
    public LayerMask playerLayer;
    public float radius;

    // 기즈모 색상 설정
    public Color gizmoColor = new Color(1, 0, 0, 0.3f);

    public void Init(float duration, float damage)
    {
        _damage = damage;

        circle.localScale = Vector3.zero;

        circle.DOScale(Vector3.one, duration).SetEase(Ease.Linear).OnComplete(() =>
        {
            CheckDamage();
            Managers.Resource.Destroy(gameObject);
            DOVirtual.DelayedCall(0.1f, () => {
                
            });
        });
    }

    private void CheckDamage()
    {

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius, playerLayer);

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Player") && hitCollider.TryGetComponent(out IDamageable damageable))
            {
                damageable.TakeDamage(_damage);
            }
        }
    }

    // 씬 뷰에서 범위를 그리는 핵심 함수
    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        // 월드 스케일이 반영된 최종 반지름 계산
        float finalRadius = radius;
            
        // 꽉 찬 구체 그리기
        Gizmos.DrawSphere(transform.position, finalRadius);
            
        // 테두리 선 그리기
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, finalRadius);
    }
}