using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

[RequireComponent(typeof(cdc_MeshCombine))]
public class DungeonGenerator : MonoBehaviour
{
    [Header("Settings")]
    public int width = 10;
    public int height = 15;
    public float tileSize = 1.0f;
    public float wallHeight = 5f; // 벽 콜라이더 높이 설정
    public bool LeftDoor=true;
    public bool RightDoor=true;
    public bool ForwardDoor=true;
    public bool BackDoor=true;
    
    [Header("Prefabs Lists")]
    public List<GameObject> floorPrefabs;
    public List<GameObject> wallPrefabs;
    public GameObject doorPrefab;
    public GameObject cornerPrefab;
    List<GameObject> objs = new List<GameObject>();
    private bool isDoor;
    public void GenerateDungeon()
    {
        DeleteGO();

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Vector3 center = new Vector3(x * tileSize, 0, z * tileSize);
                //바닥 생성
                SpawnRandomPrefab(floorPrefabs, center, Quaternion.identity);
                
                if (x == 0 || x == width - 1 || z == 0 || z == height - 1)
                {
                    
                    if (!IsDoorPosition(x,z,center))
                    {
                        //벽 생성
                        if (x == 0) SpawnRandomPrefab(wallPrefabs, center + Vector3.left * tileSize * 0.5f, Quaternion.Euler(0, 90, 0));
                        if (x == width - 1) SpawnRandomPrefab(wallPrefabs, center + Vector3.right * tileSize * 0.5f, Quaternion.Euler(0, -90, 0));
                        if (z == 0) SpawnRandomPrefab(wallPrefabs, center + Vector3.back * tileSize * 0.5f, Quaternion.identity);
                        if (z == height - 1) SpawnRandomPrefab(wallPrefabs, center + Vector3.forward * tileSize * 0.5f, Quaternion.Euler(0, 180, 0));
                        
                        //코너 생성
                        if(x==0&&z == 0)SpawnEditorObject(cornerPrefab, center + Vector3.left * tileSize * 0.5f+ Vector3.back * tileSize * 0.5f, Quaternion.identity);
                        if(x==0&&z == height - 1)SpawnEditorObject(cornerPrefab, center + Vector3.left * tileSize * 0.5f+ Vector3.forward * tileSize * 0.5f, Quaternion.Euler(0, 90, 0));
                        if(x== width - 1&&z == 0)SpawnEditorObject(cornerPrefab, center + Vector3.right * tileSize * 0.5f+ Vector3.back * tileSize * 0.5f, Quaternion.Euler(0, -90, 0));
                        if(x== width - 1&&z == height - 1)SpawnEditorObject(cornerPrefab, center + Vector3.right * tileSize * 0.5f+ Vector3.forward * tileSize * 0.5f, Quaternion.Euler(0, 180, 0));
                    }

                }
            }
        }

        CreateOptimizedColliders(); // 콜라이더 생성 함수 호출
        Debug.Log("던전 생성 및 콜라이더 배치 완료");
    }

    // 바닥과 4면의 벽에 최적화된 박스 콜라이더 추가
    void CreateOptimizedColliders()
    {
        // 바닥 콜라이더
        BoxCollider floorCol = gameObject.AddComponent<BoxCollider>();
        floorCol.center = new Vector3((width - 1) * tileSize * 0.5f, -0.05f, (height - 1) * tileSize * 0.5f);
        floorCol.size = new Vector3(width * tileSize, 0.1f, height * tileSize);

        // 네 개의 벽 콜라이더 (위치와 크기 자동 계산)
        AddWallCollider("WallCol_Left", new Vector3(-tileSize * 0.5f, wallHeight * 0.5f, (height - 1) * tileSize * 0.5f), new Vector3(1f, wallHeight, height * tileSize));
        AddWallCollider("WallCol_Right", new Vector3((width - 1) * tileSize + tileSize * 0.5f, wallHeight * 0.5f, (height - 1) * tileSize * 0.5f), new Vector3(1f, wallHeight, height * tileSize));
        AddWallCollider("WallCol_Bottom", new Vector3((width - 1) * tileSize * 0.5f, wallHeight * 0.5f, -tileSize * 0.5f), new Vector3(width * tileSize, wallHeight, 1f));
        AddWallCollider("WallCol_Top", new Vector3((width - 1) * tileSize * 0.5f, wallHeight * 0.5f, (height - 1) * tileSize + tileSize * 0.5f), new Vector3(width * tileSize, wallHeight, 1f));
    }

    void AddWallCollider(string n, Vector3 center, Vector3 size)
    {
        BoxCollider col = gameObject.AddComponent<BoxCollider>();
        col.center = center;
        col.size = size;
    }

    void SpawnRandomPrefab(List<GameObject> prefabList, Vector3 position, Quaternion rotation)
    {
        bool isNormal;
        if (prefabList != null && prefabList.Count > 0)
        {
            int index = Random.Range(0, prefabList.Count);

            SpawnEditorObject(prefabList[index], position, rotation,index <= 9);
        }
    }

    void SpawnEditorObject(GameObject prefab, Vector3 position, Quaternion rotation,bool Parent=true)
    {
        if (prefab == null) return;
#if UNITY_EDITOR
        GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        go.transform.position = position;
        go.transform.rotation = rotation;
        if(Parent) go.transform.SetParent(this.transform);
        else objs.Add(go);
        Undo.RegisterCreatedObjectUndo(go, "Create Dungeon Object");
#endif
    }

    bool IsDoorPosition(int x, int z,Vector3 center)
    {
        
        if (LeftDoor&&(x == 0 && z == height /2)) 
        {
            SpawnEditorObject(doorPrefab, center + Vector3.left * tileSize * 0.5f, Quaternion.Euler(0,90,0),false);
            return true;
        }
        if (RightDoor&&(x == width -1 && z == height/2)) 
        {
            SpawnEditorObject(doorPrefab, center + Vector3.right * tileSize * 0.5f, Quaternion.Euler(0,-90,0),false);
            return true;
        }
        if (ForwardDoor&&(x == width / 2 && z == height - 1)) 
        {
            SpawnEditorObject(doorPrefab, center + Vector3.forward * tileSize * 0.5f, Quaternion.Euler(0,180,0),false);
            return true;
        }
        if (BackDoor&&(x == width / 2 && z == 0)) 
        {
            SpawnEditorObject(doorPrefab, center + Vector3.back * tileSize * 0.5f, Quaternion.identity,false);
            return true;
        }
        return false;
    }

    public void DeleteGO()
    {
#if UNITY_EDITOR
        // 1. 프리팹 언팩 (연결 끊기)
        // 이 오브젝트가 프리팹 인스턴스인지 확인 후 완전히 언팩합니다.
        if (PrefabUtility.IsPartOfPrefabInstance(gameObject))
        {
            PrefabUtility.UnpackPrefabInstance(gameObject, PrefabUnpackMode.Completely, InteractionMode.UserAction);
            Debug.Log($"{gameObject.name}의 프리팹 연결이 해제되었습니다.");
        }

        // 2. 자식 오브젝트들을 리스트에 담아 삭제
        List<GameObject> children = new List<GameObject>();
        foreach (Transform child in transform) children.Add(child.gameObject);
        foreach (GameObject child in children) 
        {
            Undo.DestroyObjectImmediate(child);
        }
        
        objs.Clear();
        // 3. 기존에 생성된 콜라이더들 제거
        BoxCollider[] colliders = GetComponents<BoxCollider>();
        foreach (var col in colliders) 
        {
            Undo.DestroyObjectImmediate(col);
        }

        // 4. 메쉬 필터 초기화 (결합된 메쉬 데이터 제거)
        MeshFilter mf = GetComponent<MeshFilter>();
        if (mf != null) mf.sharedMesh = null;
#else
        foreach (Transform child in transform) Destroy(child.gameObject);
#endif
    }

    // 프리팹 저장 기능
    public void SaveAsPrefab()
    {
#if UNITY_EDITOR
        // 하이어라키의 자식들(비활성화된 원본들) 삭제 후  프리팹 저장 
        List<GameObject> children = new List<GameObject>();
        foreach (Transform child in transform) children.Add(child.gameObject);
        foreach (GameObject child in children) DestroyImmediate(child);
        
        string folderPath = "Assets/Prefabs/Map/BattleMap";
        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

        string fileName = $"{folderPath}/Dungeon_{System.DateTime.Now.Ticks}.prefab";
        PrefabUtility.SaveAsPrefabAssetAndConnect(gameObject, fileName, InteractionMode.UserAction);
        
        

        Debug.Log($"프리팹이 저장되었습니다: {fileName}");
#endif
    }
}


[CustomEditor(typeof(DungeonGenerator))]
public class DungeonGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        DungeonGenerator generator = (DungeonGenerator)target;
        cdc_MeshCombine combiner = generator.GetComponent<cdc_MeshCombine>();

        EditorGUILayout.Space();
        if (GUILayout.Button("1. 던전 생성 (Collider 포함)", GUILayout.Height(30)))
        {
            generator.GenerateDungeon();
        }

        if (GUILayout.Button("2. 메쉬 합치기 & 프리팹 저장", GUILayout.Height(30)))
        {
            if (combiner != null)
            {
                // 1. 메쉬 합치기 (자식들은 비활성화됨)
                combiner.CombineMeshesChildrens();
                // 2. 프리팹 저장 및 자식 삭제
                generator.SaveAsPrefab();
            }
        }

        if (GUILayout.Button("3. 초기화 (Clear)"))
        {
            generator.DeleteGO();
        }
    }
}