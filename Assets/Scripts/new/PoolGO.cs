using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ObjectPool
{
    public class PoolGO : Singleton<PoolGO>
    {
        private class PoolItems
        {
            public ObjectPool<GameObject> Pool;
            public List<GameObject> Items;
            public AsyncOperationHandle<GameObject> AddressableHandle;
        }

        private Dictionary<string, PoolItems> pools;

        private void Awake()
        {
            pools = new Dictionary<string, PoolItems>();
        }

#region GameObjects
        public void WarmPoolGameObjects(GameObject prefab, int size, Transform root = null)
        {
            if (pools.ContainsKey(prefab.name))
            {
                Debug.LogWarning($"[PoolManager] Pool for prefab {prefab.name} has already been created");
                return;
            }
            pools[prefab.name] = new PoolItems();
            pools[prefab.name].Items = new List<GameObject>(size);
            pools[prefab.name].Pool = new ObjectPool<GameObject>(() => InstantiatePrefab(prefab, prefab.name, root), true, size);
        }

        public GameObject SpawnGameObject(string name) => SpawnGameObject(name, Vector3.zero, Quaternion.identity);

        public GameObject SpawnGameObject(string name, Vector3 position, Quaternion rotation)
        {
            if (!pools.TryGetValue(name, out PoolItems poolItems))
                Debug.LogWarning($"[PoolManager] Pool for prefab {name} has not been warmed");

            if (m_IsWarming)
                throw new InvalidOperationException($"[PoolManager] This operation cannot be performed until the warming is complete.");

            GameObject clone = poolItems.Pool.Get();
            clone.transform.position = position;
            clone.transform.rotation = rotation;
            clone.SetActive(true);
            return clone;
        }

        public void ReleaseGameObject(GameObject clone)
        {
            clone.SetActive(false);

            if (pools.TryGetValue(clone.name, out PoolItems poolItems))
            {
                poolItems.Pool.Release(clone);
            }
            else
            {
                Debug.LogWarning($"[PoolManager] No pool contains the object: {clone.name}");
            }
        }

        private async void ReleaseGameObject(GameObject clone, float t)
        {
            await Task.Delay((int)(t * 1000));
            if (clone != null)
                ReleaseGameObject(clone);
        }

        private GameObject InstantiatePrefab(GameObject prefab, string name, Transform root)
        {
            GameObject go = Instantiate(prefab, root);
            go.SetActive(false);
            go.name = name;
            pools[name].Items.Add(go);
            return go;
        }

        public void Clear(string name)
        {
            if (pools.TryGetValue(name, out PoolItems poolItems))
            {
                for (int i = 0; i < poolItems.Items.Count; ++i)
                    Destroy(poolItems.Items[i]);

                poolItems.Pool.Dispose();
                if (poolItems.AddressableHandle.IsValid())
                    Addressables.ReleaseInstance(poolItems.AddressableHandle);
                pools.Remove(name);
            }
            else
            {
                Debug.LogWarning($"[PoolManager] Pool {name} does not exist");
            }
        }
#endregion

#region Addressables GameObjects
        private bool m_IsWarming;

        public async void WarmPoolAddressable(string key, int size, Transform root = null)
        {
            if (pools.ContainsKey(key))
            {
                Debug.LogWarning($"[PoolManager] Pool for prefab {key} has already been created");
                return;
            }

            m_IsWarming = true;

            AsyncOperationHandle<GameObject> asyncOperation = Addressables.LoadAssetAsync<GameObject>(key);
            while (!asyncOperation.IsDone)
                await Task.Yield();

            if (asyncOperation.Status == AsyncOperationStatus.Failed)
                ExceptionDispatchInfo.Capture(asyncOperation.OperationException).Throw();

            pools[key] = new PoolItems();
            pools[key].Items = new List<GameObject>(size);
            pools[key].Pool = new ObjectPool<GameObject>(() => InstantiatePrefab(asyncOperation.Result, key, root), true, size);
            pools[key].AddressableHandle = asyncOperation;

            m_IsWarming = false;
        }
#endregion

#region Static API
        public static void WarmGameObjects(GameObject prefab, int size, Transform root = null) => Instance.WarmPoolGameObjects(prefab, size, root);

        public static void WarmAddressable(string name, int size, Transform root = null) => Instance.WarmPoolAddressable(name, size, root);

        public static GameObject Get(GameObject prefab) => Get(prefab.name);

        public static GameObject Get(string name) => Instance.SpawnGameObject(name);

        public static GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation) => Get(prefab.name, position, rotation);

        public static GameObject Get(string name, Vector3 position, Quaternion rotation) => Instance.SpawnGameObject(name, position, rotation);

        public static void Release(GameObject clone) => Instance.ReleaseGameObject(clone);

        public static void Release(GameObject clone, float t) => Instance.ReleaseGameObject(clone, t);

        public static void Dispose(string name) => Instance.Clear(name);
#endregion
    }
}
