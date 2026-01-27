using UnityEngine;
using System.Collections;
using System.Collections.Generic; // Dictionary 사용을 위해 추가
using DG.Tweening; // DOTween 사용을 위해 추가

public class Spike : MonoBehaviour
{
    public float Damage = 20f;
    public float AttackDelay = 2f;
    private Coroutine damageRoutine;
    void Start()
    {
        // 3초마다 높이 0 -> 3초 대기 -> 높이 -2 반복 로직
        // Sequence를 생성하여 루프 설정
        Sequence spikeSequence = DOTween.Sequence();
        
        spikeSequence.Append(transform.DOMoveY(0f, 0.5f).SetEase(Ease.OutBack)) // 0으로 올라옴 (0.5초 소요)
                     .AppendInterval(3f)                                       // 3초 대기
                     .Append(transform.DOMoveY(-2f, 0.5f).SetEase(Ease.OutBack)) // -2로 내려감 (0.5초 소요)
                     .AppendInterval(3f)                                       // 3초 대기
                     .SetLoops(-1);                                            // 무한 반복
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<IDamageable>();
            if (player != null )
            {
                // 트리거 진입 시 데미지 코루틴 시작 및 저장
                damageRoutine = StartCoroutine(ApplyContinuousDamage(player));
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<IDamageable>();
            if (player != null)
            {
                // 트리거를 나가면 해당 플레이어의 코루틴 중지
                StopCoroutine(damageRoutine);
            }
        }
    }

    // 2초마다 데미지를 주는 코루틴
    private IEnumerator ApplyContinuousDamage(IDamageable target)
    {
        while (true)
        {
            target.TakeDamage(Damage);
            yield return new WaitForSeconds(AttackDelay);
        }
    }
}