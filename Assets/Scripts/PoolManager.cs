using System.Collections.Generic;
using System.Threading.Tasks;
using ObjectPooling;
using UnityEngine;

public class PoolManager : Singleton<PoolManager>
{
    public bool LogStatus = false;

    private Dictionary<GameObject, ObjectPool<GameObject>> prefabLookup;
    private Dictionary<GameObject, ObjectPool<GameObject>> instanceLookup;

    private bool dirty = false;

    void Awake()
    {
        prefabLookup = new Dictionary<GameObject, ObjectPool<GameObject>>();
        instanceLookup = new Dictionary<GameObject, ObjectPool<GameObject>>();
    }

    void Update()
    {
        if (!LogStatus || !dirty) return;
        PrintStatus();
        dirty = false;
    }

    public void WarmPoolGameObjects(GameObject prefab, int size, Transform root = null)
    {
        if (prefabLookup.ContainsKey(prefab))
        {
            Debug.LogWarning($"[PoolManager] Pool for prefab {prefab.name} has already been created");
            return;
        }

        var pool = new ObjectPool<GameObject>(() =>
        {
            var go = InstantiatePrefab(prefab, root);
            go.SetActive(false);
            return go;
        }, size);
        prefabLookup[prefab] = pool;

        dirty = true;
    }

    public GameObject SpawnGameObject(GameObject prefab)
    {
        return SpawnGameObject(prefab, Vector3.zero, Quaternion.identity);
    }

    public GameObject SpawnGameObject(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (!prefabLookup.ContainsKey(prefab))
        {
            WarmPool(prefab, 1);
        }

        var pool = prefabLookup[prefab];

        var clone = pool.GetItem();
        clone.transform.position = position;
        clone.transform.rotation = rotation;
        clone.SetActive(true);

        instanceLookup.Add(clone, pool);
        dirty = true;
        return clone;
    }

    public void ReleaseGameObject(GameObject clone)
    {
        clone.SetActive(false);

        if (instanceLookup.ContainsKey(clone))
        {
            instanceLookup[clone].ReleaseItem(clone);
            instanceLookup.Remove(clone);
            dirty = true;
        }
        else
        {
            Debug.LogWarning($"[PoolManager] No pool contains the object: {clone.name}");
        }
    }

    private async void ReleaseGameObject(GameObject clone, float t)
    {
        await Task.Delay((int) (t * 1000));
        if (clone != null)
            ReleaseGameObject(clone);
    }

    private GameObject InstantiatePrefab(GameObject prefab, Transform root)
    {
        var go = Instantiate(prefab) as GameObject;
        if (root != null) go.transform.parent = root;
        return go;
    }

    public void PrintStatus()
    {
        foreach (var keyVal in prefabLookup)
            Debug.Log(($"[Pool Manager] Object Pool for Prefab: {keyVal.Key.name} In Use: {keyVal.Value.CountUsedItems} Total {keyVal.Value.Count}"));
    }

    #region Static API

    public static void WarmPool(GameObject prefab, int size, Transform root = null)
    {
        Instance.WarmPoolGameObjects(prefab, size, root);
    }

    public static GameObject SpawnObject(GameObject prefab)
    {
        return Instance.SpawnGameObject(prefab);
    }

    public static GameObject SpawnObject(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        return Instance.SpawnGameObject(prefab, position, rotation);
    }

    public static void ReleaseObject(GameObject clone)
    {
        Instance.ReleaseGameObject(clone);
    }

    public static void ReleaseObject(GameObject clone, float t)
    {
        Instance.ReleaseGameObject(clone, t);
    }

    #endregion
}