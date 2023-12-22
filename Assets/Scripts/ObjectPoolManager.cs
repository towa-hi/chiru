using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager ins;

    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    public List<Pool> pools;
    public Dictionary<string, Queue<GameObject>> poolDictionary;

    void Awake()
    {
        if (ins == null)
        {
            ins = this;
        }
        else
        {
            if (this != ins)
            {
                Destroy(gameObject);
            }
        }
    }
    
    void Start()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(pool.tag, objectPool);
        }
    }

    public GameObject GetFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("Pool with tag " + tag + " doesn't exist.");
            return null;
        }
        
        if (poolDictionary[tag].Count == 0 || poolDictionary[tag].Peek().activeInHierarchy)
        {
            // Optional: Expand the pool by instantiating a new object
            ExpandPool(tag);
        }

        GameObject objectToSpawn = poolDictionary[tag].Dequeue();

        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        poolDictionary[tag].Enqueue(objectToSpawn);

        return objectToSpawn;
    }
    
    private void ExpandPool(string tag)
    {
        Pool pool = pools.Find(p => p.tag == tag);
        if (pool != null)
        {
            GameObject obj = Instantiate(pool.prefab);
            obj.SetActive(false);
            poolDictionary[tag].Enqueue(obj);
        }
        else
        {
            Debug.LogError("No pool found with tag: " + tag);
        }
    }

}
