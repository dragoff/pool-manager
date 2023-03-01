using System.Threading.Tasks;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    public GameObject prefab;

    // Start is called before the first frame update
    private async void Start()
    {
        PoolGO.Warm(prefab, 5);
        var go = PoolGO.Get(prefab.name);
        PoolGO.Release(go, 5);

        await Task.Delay(3000);
        PoolGO.Dispose(prefab.name);
        await Task.Delay(3000);

        PoolGO.Warm(prefab, 5);
        go = PoolGO.Get(prefab.name);
        PoolGO.Release(go, 5);
    }

    // Update is called once per frame
    private void Update()
    {
    }
}
