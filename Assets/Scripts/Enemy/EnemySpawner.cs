using System;
using System.Collections.Generic; // List 사용을 위해 추가
using System.Linq;
using UnityEngine;
using UnityEngine.AI; // NavMesh 사용을 위해 필수
using Random = UnityEngine.Random;

[System.Serializable]
public class SpawnData
{
    public int ID; 
    public float spawnWeight;      
}

public class EnemySpawner : MonoBehaviour
{
    public EnemyRoom Room;
    public GameObject[] PatrolPoints;
    
    public float spawnWidth = 10f; // 가로 길이 (X축)
    public float spawnDepth = 5f;  // 세로 길이 (Z축)
    public float overlapCheckRadius = 1f; // 적끼리 겹치지 않게 체크할 반경
    public LayerMask enemyLayer; // Enemy 레이어 설정 (인스펙터에서 선택)
    public bool Draw = true;

    private List<GameObject> enemies = new List<GameObject>();

    public void SpawnEnemys()
    {
        if (Room == null)
            Room = GetComponentInParent<EnemyRoom>();
        for (int i = 0; i < Room.spawnCount; i++)
            SpawnEnemy();
    }


    private void SpawnEnemy()
    {
        // 1. 가중치 기반 선택 (로직 분리 추천)
        SpawnData selectedEnemy = GetWeightedRandomEnemy();
        if (selectedEnemy == null) return;

        // 2. 위치 검색
        Vector3 spawnPos = GetValidSpawnPosition();
        if (spawnPos == Vector3.zero) return;

        // 3. 생성 및 초기화
        string path = $"Assets/Prefabs/Enemy/{selectedEnemy.ID}.prefab";
        GameObject go = Managers.Resource.Instantiate(path, spawnPos, Quaternion.Euler(0, Random.Range(0, 360), 0));
        var enemy = go.GetComponent<EnemyBase>();
        enemy.Init(selectedEnemy.ID);
        enemy.SetAdditionalData(PatrolPoints.ToList()); 
    
        go.SetLayerRecursively("Default");
        enemies.Add(go);
    }

    // NavMesh 위에서 겹치지 않는 좌표를 반환하는 함수
    Vector3 GetValidSpawnPosition()
    {
        Vector3 basePosition = transform.position;

        for (int i = 0; i < 15; i++)
        {
            // [수정 포인트] 원형이 아닌 직사각형 범위 내 랜덤 좌표 생성
            // 중심점에서 -Range ~ +Range 사이의 값을 가짐
            float randomX = Random.Range(-spawnWidth / 2f, spawnWidth / 2f);
            float randomZ = Random.Range(-spawnDepth / 2f, spawnDepth / 2f);
            
            Vector3 randomPos = basePosition + new Vector3(randomX, 0, randomZ);

            NavMeshHit hit;
            // NavMesh 위의 점인지 확인
            if (NavMesh.SamplePosition(randomPos, out hit, 2.0f, NavMesh.AllAreas))
            {
                // 겹침 체크
                if (!Physics.CheckSphere(hit.position, overlapCheckRadius, enemyLayer))
                {
                    return hit.position;
                }
            }
        }

        return Vector3.zero;
    }
    

    public void StartBattle()
    {
        int targetLayer = LayerMask.NameToLayer("Enemy");
        foreach (var enemy in enemies)
        {
            Transform[] allChildren = enemy.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in allChildren)
            {
                child.gameObject.layer = targetLayer;
            }
            
            var hpBar = Managers.UI.MakeSubItem<UI_EnemyHPBar>(Address.Enemy_HP_BAR);
            var enemyBase = enemy.GetComponent<EnemyBase>();
            hpBar.SetMaxHP(enemyBase.stat.MaxHp);
            hpBar.GetComponentInChildren<HealthBarController>().target = enemy.transform;

            enemyBase.takeDamageAction -= hpBar.TakeDamage;
            enemyBase.takeDamageAction += hpBar.TakeDamage;
            enemyBase.dieAcation -= hpBar.Destroy;
            enemyBase.dieAcation += hpBar.Destroy;
        }
    }

    private SpawnData GetWeightedRandomEnemy()
    {
        if (Room.enemiesToSpawn.Length == 0) return null;

        // 1. 가중치 기반 적 선택 로직 
        float totalWeight = Room.enemiesToSpawn.Sum(data => data.spawnWeight);
        float pivot = Random.Range(0, totalWeight);
        float cumulative = 0;
        SpawnData selectedEnemy = null;

        foreach (var data in Room.enemiesToSpawn)
        {
            cumulative += data.spawnWeight;
            if (pivot <= cumulative)
            {
                selectedEnemy = data;
                break;
            }
        }

        return selectedEnemy;
    }

    private void OnDrawGizmos()
    {
        if (!Draw) return;

        Gizmos.color = new Color(0, 1, 0, 0.3f);
        // 직사각형 큐브 형태로 표시 (높이는 0.1 정도로 얇게)
        Gizmos.DrawCube(transform.position, new Vector3(spawnWidth, 0.1f, spawnDepth));
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(spawnWidth, 0.1f, spawnDepth));
    }
}