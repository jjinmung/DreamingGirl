using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionTrailRenderer : MonoBehaviour {

    [HideInInspector] public SkinnedMeshRenderer SkinMesh;
    [HideInInspector] public string ValueName;
    [HideInInspector] public float ValueDetail;
    [HideInInspector] public float ValueTimeDelay;

    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;
    private Mesh _bakedMeshResult;

    private void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _meshRenderer = GetComponent<MeshRenderer>();
        _bakedMeshResult = new Mesh();
    }

    private void OnEnable()
    {
        // 1. 활성화되자마자 쉐이더 값을 초기값(0)으로 즉시 초기화
        if (_meshRenderer != null && !string.IsNullOrEmpty(ValueName))
        {
            _meshRenderer.material.SetFloat(ValueName, 0f);
        }

        StopAllCoroutines();
        
        // 2. 타겟 메쉬가 있을 때만 실행
        if (SkinMesh != null)
        {
            StartCoroutine(MaterialColorAnimation());
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    IEnumerator MaterialColorAnimation()
    {
        // 3. 메쉬 데이터 초기화 후 새로 굽기
        _bakedMeshResult.Clear(); 
        SkinMesh.BakeMesh(_bakedMeshResult);
        _meshFilter.mesh = _bakedMeshResult;
        
        // 4. 애니메이션 루프
        // float 오차를 방지하기 위해 e를 0으로 초기화하고 시작
        float e = 0f;
        while (e <= 10.0f)
        {
            _meshRenderer.material.SetFloat(ValueName, e);
            e += ValueDetail;
            yield return new WaitForSeconds(ValueTimeDelay);
        }

        // 마지막 값을 확실히 세팅
        _meshRenderer.material.SetFloat(ValueName, 1f);

        // 5. 종료 처리
        gameObject.SetActive(false);
    }
}