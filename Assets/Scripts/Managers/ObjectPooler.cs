using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static ObjectPooler Instance;

    private void Awake()
    {
        Instance = this;
    }

    // 프리팹의 InstanceID를 키로 사용하여 풀을 관리합니다.
    private Dictionary<int, Queue<GameObject>> poolDictionary = new Dictionary<int, Queue<GameObject>>();

    /// <summary>
    /// 오브젝트를 풀에서 가져오거나 새로 생성합니다.
    /// </summary>
    /// <param name="prefab">생성할 프리팹</param>
    /// <param name="position">생성 위치</param>
    /// <param name="rotation">생성 회전</param>
    /// <returns>생성된 GameObject</returns>
    public GameObject SpawnFromPool(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        int poolKey = prefab.GetInstanceID();

        // 해당 프리팹의 풀이 없다면 새로 만듭니다.
        if (!poolDictionary.ContainsKey(poolKey))
        {
            poolDictionary.Add(poolKey, new Queue<GameObject>());
        }

        GameObject objectToSpawn;

        // 풀에 비활성화된 오브젝트가 있다면 꺼내고, 없다면 새로 생성합니다.
        if (poolDictionary[poolKey].Count > 0)
        {
            objectToSpawn = poolDictionary[poolKey].Dequeue();
        }
        else
        {
            objectToSpawn = Instantiate(prefab);
            var pooledObject = objectToSpawn.GetComponent<PooledObject>();
            if (pooledObject == null)
                pooledObject= objectToSpawn.AddComponent<PooledObject>();
            pooledObject.prefabKey = poolKey;
        }
        
        objectToSpawn.SetActive(true);
        objectToSpawn.transform.SetPositionAndRotation(position, rotation);
        return objectToSpawn;
    }

    /// <summary>
    /// 오브젝트를 파괴하는 대신 풀로 다시 반합합니다.
    /// </summary>
    public void ReturnToPool(GameObject obj)
    {
        var pooledScript = obj.GetComponent<PooledObject>();
        if (pooledScript == null)
        {
            Debug.LogWarning("풀링된 오브젝트가 아니므로 파괴합니다.");
            Destroy(obj);
            return;
        }

        obj.SetActive(false);
        poolDictionary[pooledScript.prefabKey].Enqueue(obj);
    }
}
