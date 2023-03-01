using System.Collections.Generic;
using System.Threading.Tasks;
using ObjectPool;
using UnityEngine;

public class PoolGO : Singleton<PoolGO>
{
    private class PoolItems
    {
        public ObjectPool<GameObject> Pool;
        public List<GameObject> Items;
    }

    private Dictionary<string, PoolItems> pools;

    private void Awake()
    {
        pools = new Dictionary<string, PoolItems>();
    }

    public void WarmPoolGameObjects(GameObject prefab, int size, Transform root = null)
    {
        if (pools.ContainsKey(prefab.name))
        {
            Debug.LogWarning($"[PoolManager] Pool for prefab {prefab.name} has already been created");
            return;
        }
        pools[prefab.name] = new PoolItems();
        pools[prefab.name].Items = new List<GameObject>(size);
        pools[prefab.name].Pool = new ObjectPool<GameObject>(() => InstantiatePrefab(prefab, root), true, size);
    }

    public GameObject SpawnGameObject(string name) => SpawnGameObject(name, Vector3.zero, Quaternion.identity);

    public GameObject SpawnGameObject(string name, Vector3 position, Quaternion rotation)
    {
        if (!pools.TryGetValue(name, out PoolItems poolItems))
        {
            Debug.LogWarning($"[PoolManager] Pool for prefab {name} has not been warmed");
        }

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

    private GameObject InstantiatePrefab(GameObject prefab, Transform root)
    {
        GameObject go = Instantiate(prefab);
        if (root != null)
            go.transform.SetParent(root);

        go.SetActive(false);
        go.name = prefab.name;
        pools[prefab.name].Items.Add(go);
        return go;
    }

    public void Clear(string name)
    {
        if (pools.TryGetValue(name, out PoolItems poolItems))
        {
            for (int i = 0; i < poolItems.Items.Count; i++)
                Destroy(poolItems.Items[i]);
            poolItems.Pool.Dispose();
            pools.Remove(name);
        }
        else
        {
            Debug.LogWarning($"[PoolManager] Pool {name} does not exist");
        }
    }

#region Static API
    public static void Warm(GameObject prefab, int size, Transform root = null) => Instance.WarmPoolGameObjects(prefab, size, root);
    public static GameObject Get(GameObject prefab) => Get(prefab.name);
    public static GameObject Get(string name) => Instance.SpawnGameObject(name);
    public static GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation) => Get(prefab.name, position, rotation);
    public static GameObject Get(string name, Vector3 position, Quaternion rotation) => Instance.SpawnGameObject(name, position, rotation);

    public static void Release(GameObject clone) => Instance.ReleaseGameObject(clone);
    public static void Release(GameObject clone, float t) => Instance.ReleaseGameObject(clone, t);
    public static void Dispose(string name) => Instance.Clear(name);
#endregion
}
