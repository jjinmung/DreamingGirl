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
    public SpawnData[] enemiesToSpawn;
    public GameObject[] PatrolPoints;
    public int spawnCount;
    public float spawnRadius = 5f; // 스폰 지점 주변 탐색 반경
    public float overlapCheckRadius = 1f; // 적끼리 겹치지 않게 체크할 반경
    public LayerMask enemyLayer; // Enemy 레이어 설정 (인스펙터에서 선택)
    public bool Draw = true;

    private List<GameObject> enemies = new List<GameObject>();

    public void SpawnEnemys()
    {
        for (int i = 0; i < spawnCount; i++)
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
        GameObject go = Managers.Resource.Instantiate(path, spawnPos, Quaternion.identity);
        var enemy = go.GetComponent<EnemyBase>();
        enemy.Init(selectedEnemy.ID);
        enemy.SetAdditionalData(PatrolPoints.ToList()); 
    
        go.SetLayerRecursively("Default");
        enemies.Add(go);
    }

    // NavMesh 위에서 겹치지 않는 좌표를 반환하는 함수
    Vector3 GetValidSpawnPosition()
    {
        // 랜덤하게 스폰 포인트 하나 선택
        Transform basePoint = transform;

        for (int i = 0; i < 15; i++) // 최대 15번 시도
        {
            // 선택한 포인트 주변 랜덤 좌표 생성
            Vector3 randomDirection = Random.insideUnitSphere * spawnRadius;
            randomDirection += basePoint.position;

            NavMeshHit hit;
            // NavMesh 위의 가장 가까운 점을 찾음
            if (NavMesh.SamplePosition(randomDirection, out hit, spawnRadius, NavMesh.AllAreas))
            {
                // 해당 위치에 이미 다른 적이 있는지 원형 체크
                if (!Physics.CheckSphere(hit.position, overlapCheckRadius, enemyLayer))
                {
                    return hit.position;
                }
            }
        }

        return Vector3.zero; // 실패 시 zero 반환
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
        if (enemiesToSpawn.Length == 0) return null;

        // 1. 가중치 기반 적 선택 로직 
        float totalWeight = enemiesToSpawn.Sum(data => data.spawnWeight);
        float pivot = Random.Range(0, totalWeight);
        float cumulative = 0;
        SpawnData selectedEnemy = null;

        foreach (var data in enemiesToSpawn)
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

        // 기즈모 색상 설정 (반투명한 하늘색)
        Gizmos.color = new Color(1, 1, 1, 0.3f);
        Gizmos.DrawSphere(transform.position, spawnRadius);
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}