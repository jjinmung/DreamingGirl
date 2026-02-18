using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AI;
using UnityEngine.Pool;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

public class ResourceManager : MonoBehaviour
{
    // 에셋 캐시 (Addressables)
    private Dictionary<string, AsyncOperationHandle> _resources = new Dictionary<string, AsyncOperationHandle>();

    // 오브젝트 풀 (UnityEngine.Pool 사용)
    // 주소(Key)별로 IObjectPool을 관리합니다.
    private Dictionary<string, IObjectPool<GameObject>> _pools = new Dictionary<string, IObjectPool<GameObject>>();

    public AddressableData Data;
    public GameObject PoolRoot
    {
        get
        {
            GameObject root = GameObject.Find("@Pool");
            if (root == null)
                root = new GameObject { name = "@Pool" };
            return root;
        }
    }

    #region Pool 생성 로직 (Internal)

    private IObjectPool<GameObject> GetOrCreatePool(string address, GameObject prefab)
    {
        if (_pools.TryGetValue(address, out var pool))
            return pool;

        // 새로운 풀 생성
        pool = new ObjectPool<GameObject>(
            createFunc: () => {
                GameObject go = Object.Instantiate(prefab);
                go.name = prefab.name;
                // PooledObject 컴포넌트 강제 추가
                var po = go.GetComponent<PooledObject>() ?? go.AddComponent<PooledObject>();
                po.address = address;
                po.isReleased = false;
                return go;
            },
            actionOnGet: (go) => {
                go.SetActive(true);
                if (go.TryGetComponent<PooledObject>(out var po))
                    po.isReleased = false; // 꺼낼 때 false로 초기화
            },
            actionOnRelease: (go) => {
                go.SetActive(false);
                if (go.TryGetComponent<PooledObject>(out var po))
                    po.isReleased = true; // 반납될 때 true로 설정
            },
            actionOnDestroy: (go) => Object.Destroy(go),
            collectionCheck: true, // 중복 반납 시 예외 발생 (안전 장치)
            defaultCapacity: 10,
            maxSize: 1000
        );

        _pools.Add(address, pool);
        return pool;
    }

    #endregion

    #region Async Load (비동기 로드)

    public async UniTask<T> LoadAsync<T>(string address) where T : Object
    {
        if (_resources.TryGetValue(address, out AsyncOperationHandle handle))
        {
            await handle.Task;
            return handle.Result as T;
        }

        var loadHandle = Addressables.LoadAssetAsync<T>(address);
        _resources.Add(address, loadHandle);
        await loadHandle.Task;

        if (loadHandle.Status == AsyncOperationStatus.Succeeded)
            return loadHandle.Result;

        Debug.LogError($"[ResourceManager] Failed to load async: {address}");
        return null;
    }
    public async UniTask<Sprite> LoadIconAsync(AssetReferenceSprite referenceSprite)
    {
        if (referenceSprite.OperationHandle.IsValid())
        {
            return referenceSprite.OperationHandle.Result as Sprite;
        }

        return await referenceSprite.LoadAssetAsync<Sprite>();
    }
 
    public async UniTask<T[]> LoadAllAsync<T>(string label) where T : Object
    {
        if (_resources.TryGetValue(label, out AsyncOperationHandle handle))
        {
            await handle.Task;
            return (handle.Result as IList<T>)?.ToArray();
        }

        var loadHandle = Addressables.LoadAssetsAsync<T>(label, null);
        _resources.Add(label, loadHandle);
        await loadHandle.Task;

        if (loadHandle.Status == AsyncOperationStatus.Succeeded)
            return loadHandle.Result.ToArray();

        return null;
    }
    
    public async UniTask<T> LoadAsync<T>(AssetReference assetRef) where T : Object
    {
        if (assetRef == null || !assetRef.RuntimeKeyIsValid()) return null;

        // RuntimeKey 자체를 키로 사용하여 딕셔너리에 저장/조회
        string key = assetRef.RuntimeKey.ToString(); 

        if (_resources.TryGetValue(key, out AsyncOperationHandle handle))
        {
            await handle.Task;
            return handle.Result as T;
        }
        
        var loadHandle = assetRef.LoadAssetAsync<T>(); 
        _resources.Add(key, loadHandle);
    
        await loadHandle.Task;
        return loadHandle.Result;
    }

    #endregion

    #region Async Instantiate (비동기 생성)

    public async UniTask<GameObject> InstantiateAsync(string address, Vector3 position = default, Quaternion rotation = default, Transform parent = null)
    {
        GameObject prefab = await LoadAsync<GameObject>(address);
        if (prefab == null) return null;

        IObjectPool<GameObject> pool = GetOrCreatePool(address, prefab);
        GameObject go = pool.Get();

        SetTransformAndAgent(go, position, rotation, parent);
        return go;
    }

    public async UniTask<GameObject> InstantiateAsync(AssetReference assetRef, Vector3 position = default, Quaternion rotation = default, Transform parent = null)
    {
        if (assetRef == null || !assetRef.RuntimeKeyIsValid()) return null;
        return await InstantiateAsync(assetRef.RuntimeKey.ToString(), position, rotation, parent);
    }

    #endregion

    #region Synchronous Load (동기 로드)

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
        if (_resources.TryGetValue(label, out AsyncOperationHandle handle))
            return (handle.Result as IList<T>)?.ToArray();

        var loadHandle = Addressables.LoadAssetsAsync<T>(label, null);
        loadHandle.WaitForCompletion();

        if (loadHandle.Status == AsyncOperationStatus.Succeeded)
        {
            _resources.TryAdd(label, loadHandle);
            return loadHandle.Result.ToArray();
        }
        return null;
    }

    public T Load<T>(AssetReference assetRef) where T : Object
    {
        if (assetRef == null || !assetRef.RuntimeKeyIsValid()) return null;
        return Load<T>(assetRef.RuntimeKey.ToString());
    }

    #endregion


    #region Destroy / Release (반납)

    public void Destroy(GameObject go, float delay = 0f)
    {
        if (go == null) return;

        if (delay <= 0f)
        {
            ReturnToPool(go);
        }
        else
        {
            DelayDestroy(go, delay).Forget();
        }
    }

    private async UniTaskVoid DelayDestroy(GameObject go, float delay)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(delay));
        ReturnToPool(go);
    }

    private void ReturnToPool(GameObject go)
    {
        if (go == null) return;

        if (go.TryGetComponent<PooledObject>(out var po))
        {
            // 핵심: 이미 반납된 상태라면 중복 처리를 하지 않음
            if (po.isReleased) return;

            if (_pools.TryGetValue(po.address, out var pool))
            {
                pool.Release(go);
                return;
            }
        }

        Object.Destroy(go);
    }

    #endregion

    #region Utils & Cleanup

    private void SetTransformAndAgent(GameObject go, Vector3 position, Quaternion rotation, Transform parent)
    {
        go.transform.SetParent(parent ?? PoolRoot.transform);

        if (go.TryGetComponent<NavMeshAgent>(out var agent))
        {
            agent.enabled = false;
            go.transform.SetPositionAndRotation(position, rotation);
            agent.enabled = true;
            if (agent.isOnNavMesh) agent.Warp(position);
        }
        else
        {
            go.transform.SetPositionAndRotation(position, rotation);
        }
    }

    public void Clear()
    {
        // 1. 모든 풀 내부 에셋 파괴
        foreach (var pool in _pools.Values)
            pool.Clear();
        _pools.Clear();

        // 2. 어드레서블 핸들 해제
        foreach (var handle in _resources.Values)
            Addressables.Release(handle);
        _resources.Clear();
    }

    #endregion
}