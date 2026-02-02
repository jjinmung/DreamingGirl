using System;
using UnityEngine;

public class WaterBullet : MonoBehaviour
{
    public TrailRenderer[] trailRenderers; // 복수형 이름 권장

    public void OnEnable()
    {
        // 1. 활성화 시점에는 일단 모두 끕니다.
        foreach (TrailRenderer tr in trailRenderers)
        {
            tr.Clear();
            tr.enabled = false;
        }
        
        // 2. 위치가 셋팅된 직후(보통 다음 프레임)에 트레일을 켭니다.
        Invoke(nameof(EnableTrails), 0.02f); 
    }

    private void EnableTrails()
    {
        foreach (TrailRenderer tr in trailRenderers)
        {
            tr.enabled = true;
        }
    }

    private void OnDisable()
    {
        foreach (TrailRenderer tr in trailRenderers)
        {
            tr.Clear();
            tr.enabled = false;
        }
    }
}