using UnityEngine;

public class HealthBarController : MonoBehaviour
{
    public Transform targetMonster; // 몬스터의 위치 (머리 위 Offset)
    public Vector3 screenOffset = new Vector3(0, 100f, 0);

    void LateUpdate()
    {
        if (targetMonster == null) return;

        // 1. 몬스터의 현재 월드 중심 좌표를 그대로 화면 좌표로 변환
        Vector3 screenPos = Camera.main.WorldToScreenPoint(targetMonster.position);

        // 2. 카메라 뒤에 있는 경우 처리 (Z값이 0보다 작으면 화면 뒤임)
        if (screenPos.z < 0)
        {
            // 화면 밖으로 나가면 UI를 안 보이게 처리 (알파값을 0으로 하거나 좌표를 멀리 보냄)
            return;
        }

        // 3. 변환된 2D 좌표에 픽셀 단위의 오프셋을 더함
        // 팁: Screen Space Overlay 캔버스라면 transform.position에 바로 넣어도 됩니다.
        transform.position = screenPos + screenOffset;
    }
}
