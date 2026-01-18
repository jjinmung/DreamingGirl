using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ResourceManager : MonoBehaviour
{
    // 에셋 캐시 (Addressables)
    private Dictionary<string, AsyncOperationHandle> _resources = new Dictionary<string, AsyncOperationHandle>();
    
    // 오브젝트 풀 (Pooling)
    private Dictionary<string, Queue<GameObject>> _pools = new Dictionary<string, Queue<GameObject>>();
    
    GameObject _root;

    public void Init()
    {
        _root = GameObject.Find("@Pool");
        if(_root == null)
            _root = new GameObject { name = "@Pool" };
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

    // [Instantiate / Spawn] 풀링을 포함한 생성
    public GameObject Instantiate(string address, Vector3 position=default, Quaternion rotation=default,Transform parent = null)
    {
        // 1. 풀에 남아있는게 있는지 확인
        if (_pools.ContainsKey(address) && _pools[address].Count > 0)
        {
            GameObject obj = _pools[address].Dequeue();
            obj.SetActive(true);
            if(parent == null)
                obj.transform.SetParent(_root.transform);
            else
                obj.transform.SetParent(parent);
            obj.transform.SetPositionAndRotation(position, rotation);
            return obj;
        }

        // 2. 없다면 새로 로드 및 생성
        GameObject prefab = Load<GameObject>(address);
        if (prefab == null) return null;

        GameObject go = Object.Instantiate(prefab, parent);
        
        go.transform.SetParent(parent);
        go.transform.SetPositionAndRotation(position, rotation);
        go.name = prefab.name;

        // 풀링 정보 기입
        PooledObject po = go.GetOrAddComponent<PooledObject>();
        po.address = address;

        return go;
    }

    // [Destroy / Release] 풀로 반납
    public void Destroy(GameObject go)
    {
        if (go == null) return;

        PooledObject po = go.GetComponent<PooledObject>();
        
        // 풀링 대상이 아니면 그냥 파괴
        if (po == null)
        {
            Object.Destroy(go);
            return;
        }

        // 풀에 반납
        if (!_pools.ContainsKey(po.address))
            _pools.Add(po.address, new Queue<GameObject>());

        _pools[po.address].Enqueue(go);
        go.SetActive(false);
        //go.transform.SetParent(transform);
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


