using System.Threading.Tasks;
using ObjectPool;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    public GameObject prefab;

    // Start is called before the first frame update
    private async void Start()
    {
        await Task.Delay(3000);

        PoolGO.WarmAddressable(prefab.name, 5000);
        // PoolGO.WarmGameObjects(prefab, 5000);
        await Task.Delay(3000);
        
        var go = PoolGO.Get(prefab.name);
        PoolGO.Release(go, 3);

        await Task.Delay(5000);
        PoolGO.Dispose(prefab.name);
        await Task.Delay(3000);
        //
        // PoolGO.WarmAddressables(prefab.name, 5);
        // go = PoolGO.Get(prefab.name);
        // PoolGO.Release(go, 5);
    }

    // Update is called once per frame
    private void Update()
    {
    }
}
