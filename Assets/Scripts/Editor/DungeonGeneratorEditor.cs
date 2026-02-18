using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DungeonGenerator))]
public class DungeonGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 기존 인스펙터 속성들을 그대로 그림
        DrawDefaultInspector();

        DungeonGenerator generator = (DungeonGenerator)target;
        // 메쉬 결합 컴포넌트 가져오기
        cdc_MeshCombine combiner = generator.GetComponent<cdc_MeshCombine>();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Dungeon Tools", EditorStyles.boldLabel);

        if (GUILayout.Button("1. 던전 생성 (Collider 포함)", GUILayout.Height(30)))
        {
            generator.GenerateDungeon();
        }

        if (GUILayout.Button("2. 메쉬 합치기 & 프리팹 저장", GUILayout.Height(30)))
        {
            if (combiner != null)
            {
                // 1. 메쉬 합치기
                combiner.CombineMeshesChildrens();
                // 2. 프리팹 저장
                generator.SaveAsPrefab();
            }
            else
            {
                Debug.LogError("cdc_MeshCombine 컴포넌트가 없습니다!");
            }
        }

        if (GUILayout.Button("3. 초기화 (Clear)"))
        {
            if (EditorUtility.DisplayDialog("초기화", "정말 모든 자식 오브젝트를 삭제하시겠습니까?", "예", "아니오"))
            {
                generator.DeleteGO();
            }
        }
    }
}