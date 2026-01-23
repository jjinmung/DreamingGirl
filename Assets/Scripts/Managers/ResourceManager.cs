using System.Collections;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AI;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ResourceManager : MonoBehaviour
{
    // 에셋 캐시 (Addressables)
    private Dictionary<string, AsyncOperationHandle> _resources = new Dictionary<string, AsyncOperationHandle>();
    
    // 오브젝트 풀 (Pooling)
    private Dictionary<string, Queue<GameObject>> _pools = new Dictionary<string, Queue<GameObject>>();
    
    public GameObject Pool
    {
        get
        {
            GameObject root = GameObject.Find("@Pool");
            if (root == null)
                root = new GameObject { name = "@Pool" };
            return root;
        }
    }
    // [Load] 에셋 로드 (동기)
    public T Load<T>(string address) where T : Object
    {
        if (_resources.TryGetValue(address, out AsyncOperationHandle handle))
            return handle.Result as T;

        var loadHandle = Addressables.LoadAssetAsync<T>(address);
        loadHandle.WaitForCompletion();

        _resources.Add(address, loadHandle);
        return loadHandle.Result;
    }
    public T[] LoadAll<T>(string label) where T : Object
    {
        // Addressables에서 라벨로 에셋 위치 목록을 가져옴
        var handle = Addressables.LoadAssetsAsync<T>(label, null);
        handle.WaitForCompletion();

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            IList<T> resultList = handle.Result;
        
            // 내부 캐시에 개별적으로 등록 (나중에 개별 Load 시 중복 로드 방지)
            // 주의: 라벨 로드 자체의 핸들은 따로 관리하거나, 개별 에셋 핸들을 추적해야 합니다.
            // 여기서는 단순화를 위해 결과 리스트를 반환합니다.
        
            T[] array = new T[resultList.Count];
            resultList.CopyTo(array, 0);
        
            // 캐싱 로직: 개별 에셋의 주소를 알 수 있다면 좋지만, 
            // 라벨 로드 시에는 보통 핸들 전체를 관리하는 전용 딕셔너리를 하나 더 두는 것이 좋습니다.
            if (!_resources.ContainsKey(label))
                _resources.Add(label, handle);

            return array;
        }

        Debug.LogError($"[ResourceManager] Failed to load assets with label: {label}");
        return null;
    }

    // [Instantiate / Spawn] 풀링을 포함한 생성
    public GameObject Instantiate(string address, Vector3 position = default, Quaternion rotation = default, Transform parent = null)
    {
        GameObject go = null;

        // 1. 풀에 남아있는게 있는지 확인
        if (_pools.ContainsKey(address) && _pools[address].Count > 0)
        {
            go = _pools[address].Dequeue();
            go.SetActive(true);
        }
        else
        {
            // 2. 없다면 새로 로드 및 생성
            GameObject prefab = Load<GameObject>(address);
            if (prefab == null) return null;

            go = Object.Instantiate(prefab); // 생성 시 부모 설정은 아래에서 일괄 처리
            go.name = prefab.name;

            // 풀링 정보 기입
            PooledObject po = go.GetOrAddComponent<PooledObject>();
            po.address = address;
        }
        
        if (parent == null)
            go.transform.SetParent(Pool.transform);
        else
            go.transform.SetParent(parent);

        // 핵심: NavMeshAgent 체크 및 위치 설정
        NavMeshAgent agent = go.GetComponent<NavMeshAgent>();
        if (agent != null && agent.isOnNavMesh)
        {
            agent.enabled = false;
            agent.enabled = true;
            // 에이전트가 활성화된 상태에서 Warp 호출
            agent.Warp(position);
            go.transform.rotation = rotation;
        }
        else
        {
            // 에이전트가 없거나 NavMesh 위에 없는 경우 일반 이동
            go.transform.SetPositionAndRotation(position, rotation);
        }

        return go;
    }

    // [Destroy / Release] 풀로 반납
    public void Destroy(GameObject go, float delay=0f)
    {
        if (go == null) return;

        // 즉시 처리가 필요한 경우 (delay가 0 이하)
        if (delay <= 0f)
        {
            ReturnToPool(go);
        }
        else
        {
            // 지연 처리를 위해 코루틴 실행
            StartCoroutine(CoDestroy(go, delay));
        }
    }

    private IEnumerator CoDestroy(GameObject go, float delay)
    {
        yield return new WaitForSeconds(delay);
        ReturnToPool(go);
    }
    
    private void ReturnToPool(GameObject go)
    {
        if (go == null) return;

        PooledObject po = go.GetComponent<PooledObject>();
    
        if (po == null)
        {
            Object.Destroy(go);
            return;
        }

        if (!_pools.ContainsKey(po.address))
            _pools.Add(po.address, new Queue<GameObject>());

        // 중복 반납 방지 체크 (이미 비활성화된 경우 제외)
        if (go.activeSelf)
        {
            _pools[po.address].Enqueue(go);
            go.SetActive(false);
            // go.transform.SetParent(transform);
        }
    }
    public void Clear()
    {
        // 1. 풀에 저장된 실제 GameObject들을 모두 파괴
        foreach (var queue in _pools.Values)
        {
            while (queue.Count > 0)
            {
                GameObject go = queue.Dequeue();
                Object.Destroy(go);
            }
        }
        _pools.Clear();

        // 2. 에셋 참조 해제 (이제 원본을 안전하게 제거 가능)
        foreach (var handle in _resources.Values)
        {
            Addressables.Release(handle);
        }
        _resources.Clear();
    }
}


