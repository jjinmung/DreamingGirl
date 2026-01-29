using System.Collections;
using UnityEngine;

public class AbilityThunderflash : ActiveAbilityEffect
{
    private float blinkDistance = 10f; // 이동 거리
    private LayerMask wallLayer = LayerMask.GetMask("Map");      // 벽으로 인식할 레이어
    private PlayerController player => Managers.Player.PlayerControl;
    public override void Apply(int stack)
    {
        Managers.Player.PlayerControl.GetAciveSkill(Define.AbilityID.Thunderflash);
    }
    public override void Execute()
    {
        player.StartCoroutine(BlinkCoroutine());
    }
    
    
    IEnumerator BlinkCoroutine()
    {
        Vector3 startPosition = player.transform.position;
        Vector3 direction = player.transform.forward;
        Vector3 targetPosition = startPosition + direction * blinkDistance;
       
        // 1. 도착 지점 혹은 경로에 벽이 있는지 체크
        // Raycast(시작점, 방향, 결과 저장, 최대 거리, 레이어 마스크)
        if (Physics.Raycast(startPosition, direction, out RaycastHit hit, blinkDistance, wallLayer))
        {
            // 벽이 있다면: 벽의 충돌 지점에서 약간 뒤쪽으로 이동 (벽에 파묻힘 방지)
            targetPosition = hit.point - (direction * 0.5f);
        }
        // 1. 트레일 활성화
        player.ThunderTrail[0].emitting = true;
        player.ThunderTrail[1].emitting = true;
        // 2. 아주 짧은 시간(예: 0.05초) 동안 목적지로 부드럽게 이동
        float elapsed = 0f;
        float duration = 0.05f; // 이 시간이 짧을수록 순간이동에 가깝습니다.

        while (elapsed < duration)
        {
            player.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        player.transform.position = targetPosition;

        // 3. 잠시 후 트레일 끄기 (여운을 남기기 위해 약간 대기)
        yield return new WaitForSeconds(0.1f);
        player.ThunderTrail[0].emitting = false;
        player.ThunderTrail[1].emitting = false;
    }
}