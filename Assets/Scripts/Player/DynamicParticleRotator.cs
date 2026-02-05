using System;
using UnityEngine;

public class DynamicParticleRotator : MonoBehaviour
{
    public float rotateSpeed = 100f; // 회전 속도
    public float radius = 3f;        // 중심으로부터의 거리
    
    private float currentAngle = 0f; // 현재 회전각
    private bool Rotating = false;

    // Update 대신 LateUpdate를 사용해서 부모(플레이어)의 움직임이 끝난 후 최종 위치를 잡기 
    void LateUpdate()
    {
        if (Rotating)
        {
            // 1. 회전 각도 계산 
            currentAngle += (rotateSpeed * Time.deltaTime)%360;

            // 2. 부모(플레이어)가 돌아가더라도 내 회전은 세계 기준(Quaternion.identity)으로 초기화
            transform.rotation = Quaternion.Euler(0, currentAngle, 0);
        }
        
    }
    
    public void SetOrbs(int count)
    {
        Rotating = count > 0;
        int activeCount = Math.Min(transform.childCount,count);
        
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }

        for (int i = 0; i < activeCount; i++)
        {
            float angle = i * (360f / activeCount);
            float rAngle = angle * Mathf.Deg2Rad;//라디안 단위로 초기화

            float x = Mathf.Cos(rAngle) * radius;
            float z = Mathf.Sin(rAngle) * radius;

            transform.GetChild(i).localPosition = new Vector3(x, 0, z);
            transform.GetChild(i).gameObject.SetActive(true);
        }
        
    }
}