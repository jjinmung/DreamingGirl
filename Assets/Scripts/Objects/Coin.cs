using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class Coin : MonoBehaviour
{
    private Rigidbody rb;
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        Jump();
    }

    public void Jump()
    {
        rb.isKinematic = false;
        transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
        Vector3 jumpDirection = new Vector3(0, 1f, 0).normalized;

        // 2. 위로 튀어오르는 힘 (원하는 세기로 조절)
        float jumpForce = 8f;
        rb.AddForce(jumpDirection * jumpForce, ForceMode.Impulse);

        // 3. 데굴데굴 구르게 만드는 토크(회전력) 추가
        Vector3 randomTorque = new Vector3(8f, 8f, 8f);
        rb.AddTorque(randomTorque, ForceMode.Impulse);
    }

    public void Clear()
    {
        // 1. 물리 엔진 비활성화 (플레이어에게 직선으로 날아가기 위함)
        rb.isKinematic = true;

        // 2. 타겟 설정 (플레이어)
        Transform target = Managers.Player.Trans;
        if (target == null) return;

        // 3. 연출 시작 (DOTween 활용)
        // - 플레이어 위치로 이동 (0.5초 동안)
        // - 크기를 0으로 축소 (0.5초 동안)
        // - 이동이 끝나면 오브젝트 풀로 반납
        
        Sequence seq = DOTween.Sequence();

        // 이동과 크기 조절을 동시에 진행
        seq.Join(transform.DOMove(target.position, 0.5f).SetEase(Ease.InBack)); // 살짝 뒤로 갔다 빨라지는 효과
        seq.Join(transform.DOScale(Vector3.zero, 1f).SetEase(Ease.InQuad));
        // 모든 연출이 끝나면 실행할 로직
        seq.OnComplete(() =>
        {
            Managers.Resource.Destroy(gameObject);
        });
    }
}