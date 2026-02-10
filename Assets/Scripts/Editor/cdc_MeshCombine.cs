using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[ExecuteInEditMode]
public class cdc_MeshCombine : MonoBehaviour
{
    [Header("3D Mesh Center Pivot")]
    [Tooltip("중심축이 될 Transform을 연결. 비어있을 경우 월드 기준 좌표가 정렬")]
    public Transform centerPosition;
    public bool autoCenterThis = true;

    private void Awake()
    {
        if (autoCenterThis)
        {
            centerPosition = this.transform;
        }
    }

    public void CombineMeshesChildrens()
    {
        // 1. 초기화
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length - 1]; // 자신 제외

        int combineIndex = 0;
        for (int i = 0; i < meshFilters.Length; i++)
        {
            // 자기 자신(부모)의 MeshFilter는 합치기 대상에서 제외
            if (meshFilters[i].gameObject == gameObject) continue;
            if (meshFilters[i].sharedMesh == null) continue;

            combine[combineIndex].mesh = meshFilters[i].sharedMesh;
            // 부모의 로컬 좌표계를 기준으로 자식들의 위치를 계산
            combine[combineIndex].transform = transform.worldToLocalMatrix * meshFilters[i].transform.localToWorldMatrix;
        
            meshFilters[i].gameObject.SetActive(false);
            combineIndex++;
        }

        // 2. 새로운 메쉬 생성 및 합치기
        Mesh finalMesh = new Mesh();
        finalMesh.name = "CombinedMesh_" + gameObject.name;
        finalMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        
        finalMesh.CombineMeshes(combine, true, true);

        // 3. 메쉬 할당
        GetComponent<MeshFilter>().sharedMesh = finalMesh;

#if UNITY_EDITOR
        // 4. 매쉬 프리펩 저장
        if (!Application.isPlaying)
        {
            string path = "Assets/Meshes/CombinedMesh/CombinedMesh_" + System.DateTime.Now.Ticks + ".asset";
            AssetDatabase.CreateAsset(finalMesh, path);
            AssetDatabase.SaveAssets();
            Debug.Log("메쉬가 저장되었습니다: " + path);
        }
#endif

     
    }

    #region EditorOnly
    #if UNITY_EDITOR
    [CustomEditor(typeof(cdc_MeshCombine))]
    public class cdc_MeshCombine_Instance : Editor
    {
        public cdc_MeshCombine myFild;

        private void OnEnable()
        {
            if (AssetDatabase.Contains(target))
            {
                myFild = null;
            }
            else
            {
                myFild = (cdc_MeshCombine)target;
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            cdc_MeshCombine script = target as cdc_MeshCombine;

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.HelpBox("자신을 포함한 MeshFilter를 합쳐줍니다.\nCenter Position에 트랜스폼 연결 시 해당 트랜스 폼 중심으로 정점들이 재 배치 됩니다.", MessageType.Info);
            EditorGUILayout.Space();

            if (GUILayout.Button("자식 메쉬 하나로 합치기"))
            {
                script.CombineMeshesChildrens();
            }
            EditorGUILayout.Space();
        }
     }
    #endif
    #endregion
}