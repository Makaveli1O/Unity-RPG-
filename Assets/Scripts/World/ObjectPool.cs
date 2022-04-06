using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour, ObjectPoolInterface
{
    public int id;
    private List<GameObject> pooledObjects = new List<GameObject>();
    public int amountToPool;

    public List<GameObject> activeObjects = new List<GameObject>();
    [SerializeField] private GameObject prefab;


    /// <summary>
    /// Initialize inactive gameobjects in pool to begin with.
    /// </summary>
    public void Awake()
    {
        for (int i = 0; i < amountToPool; i++)
        {
            GameObject obj = Instantiate(prefab, new Vector3(0,0,1), Quaternion.identity);
            obj.transform.parent = gameObject.transform;
            obj.SetActive(false);
            pooledObjects.Add(obj);
        }
    }

    /// <summary>
    /// Use this method instead of initialization when loading chunk. This will return
    /// inactive gameobject in pool.
    /// </summary>
    /// <returns>Inactive gameobject or null.</returns>
    public GameObject GetPooledObject(){
        for (int i = 0; i < pooledObjects.Count; i++)
        {
            if (!pooledObjects[i].activeInHierarchy)
            {
                activeObjects.Add(pooledObjects[i]); //mark as active
                return pooledObjects[i];
            }
        }
        return null;
    }

    /// <summary>
    /// Deactivates all active objects within this pool.
    /// </summary>
    public void DeactivateAll(){
        activeObjects.Clear();
    }

    /// <summary>
    /// Search for all available (inactive) gameobjects within pool and returns it
    /// </summary>
    /// <returns>Inactive gameobjects within pool</returns>
    public List<GameObject> GetInactiveObjects(){
        List<GameObject> retArr = new List<GameObject>();
        for (int i = 0; i < pooledObjects.Count; i++)
        {
            if (!pooledObjects[i].activeInHierarchy)
            {
                retArr.Add(pooledObjects[i]);
            }
        }
        return retArr;
    }
}