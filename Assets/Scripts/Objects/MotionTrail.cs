using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionTrail : MonoBehaviour {
    
    [Header("타겟 스킨메쉬 배열 (여러 개 등록 가능)")]
    public SkinnedMeshRenderer[] TargetSkinMeshes; // 배열로 변경

    [Header("이펙트 출력할 속도간격")]
    [Range(0, 1)]
    public float ExportSpeedDelay = 0.1f;

    [Header("이펙트 출력시간 설정")]
    public bool UseLifeTime = false; 
    public float EffectLifeTime = 3;

    [Header("쉐이더 변수 설정")]
    public string ValueName;
    [Range(0, 1)]
    public float ValueTimeDelay = 0.1f;
    [Range(0, 1)]
    public float ValueDetail = 0.1f;
    
    private void OnEnable()
    {
        // 유효성 검사
        if (TargetSkinMeshes == null || TargetSkinMeshes.Length == 0)
        {
            return;
        }
        if (string.IsNullOrEmpty(ValueName))
        {
            Debug.LogError("변경할 쉐이더 변수이름이 없습니다.", this);
            return;
        }
        
        StopAllCoroutines();
        StartCoroutine(GhostStart());

        if(UseLifeTime)
        {
            StartCoroutine(TimerStart());
        }
    }

    IEnumerator GhostStart()
    {
        while (true)
        {
            // 모든 타겟 메쉬에 대해 잔상 생성 시도
            foreach (SkinnedMeshRenderer target in TargetSkinMeshes)
            {
                if (target == null) continue;
                
                bool success = false;
                // 현재 자식들 중 비활성화된 녀석을 찾아서 할당
                for (int i = 0; i < transform.childCount; i++)
                {
                    success = TryActivateTrail(i, target.gameObject);
                    if (success) break;
                }

                // 만약 남는 자식이 없다면 새로 생성해서 할당
                if (!success)
                {
                    GameObject newGhost = Instantiate(transform.GetChild(0).gameObject, this.transform);
                    newGhost.SetActive(false); // 일단 끄고 다음 프레임이나 루프에서 사용
                    TryActivateTrail(transform.childCount - 1, target.gameObject);
                }
            }
            yield return new WaitForSeconds(ExportSpeedDelay);
        }
    }

    // 특정 인덱스의 자식 오브젝트를 특정 타겟 메쉬의 잔상으로 활성화 시도
    private bool TryActivateTrail(int childIndex, GameObject target)
    {
        GameObject ghost = transform.GetChild(childIndex).gameObject;

        if (!ghost.activeSelf)
        {
            ghost.transform.position = target.transform.position;
            ghost.transform.rotation = target.transform.rotation;
            
            var trailRenderer = ghost.GetComponent<MotionTrailRenderer>();
            var targetSMR = target.GetComponent<SkinnedMeshRenderer>();

            if (trailRenderer != null && targetSMR != null)
            {
                trailRenderer.SkinMesh = targetSMR;
                trailRenderer.ValueName = ValueName;
                trailRenderer.ValueTimeDelay = ValueTimeDelay;
                trailRenderer.ValueDetail = ValueDetail;
                
                ghost.SetActive(true);
                return true;
            }
        }
        return false;
    }

    IEnumerator TimerStart()
    {
        yield return new WaitForSeconds(EffectLifeTime);
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
        this.gameObject.SetActive(false); // 코루틴 중단 대신 오브젝트 자체를 끄는 것이 깔끔함
    }

    public void EffectOff()
    {
        StopAllCoroutines();
        StartCoroutine(TimerStart());
    }

}