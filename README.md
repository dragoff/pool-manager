# Simple Unity gameobject pool
## Installation

* Open Package Manager =>
* Tap on `+` button => 
* Add package from git URL =>
* Put inside https://github.com/dragoff/pool-manager.git

## Using 
First,
```c#
PoolManager.WarmPool(GameObject prefab, int size, Transform root) 
```

Then,
```c#
PoolManager.Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
```
Or,
```c#
PoolManager.Release(GameObject clone, float time)
```    
***Note:***  Package contains MonoBehaviour's `Singletone<T>`

