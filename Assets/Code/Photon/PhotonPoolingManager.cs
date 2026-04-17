using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PhotonPoolingManager : MonoBehaviour, IPunPrefabPool
{
    public static PhotonPoolingManager Instance;

    private Dictionary<string, Queue<GameObject>> poolDict = new Dictionary<string, Queue<GameObject>>();

    private Transform poolRoot;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            GameObject root = new GameObject("Pooling");
            poolRoot = root.transform;

            PhotonNetwork.PrefabPool = this;
            DontDestroyOnLoad(poolRoot.gameObject);
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 포톤이 Instantiate를 호출할 때 내부적으로 이 메소드를 실행함
    public GameObject Instantiate(string prefabId, Vector3 position, Quaternion rotation)
    {
        if (!poolDict.ContainsKey(prefabId))
        {
            poolDict.Add(prefabId, new Queue<GameObject>());
        }

        if (poolDict[prefabId].Count > 0)
        {
            GameObject obj = poolDict[prefabId].Dequeue();
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            return obj;
        }

        GameObject prefab = Resources.Load<GameObject>(prefabId);
        GameObject newObj = Object.Instantiate(prefab, position, rotation);

        newObj.transform.SetParent(poolRoot);
        newObj.SetActive(false);
        return newObj;
    }

    // 포톤이 Destroy를 호출할 때 내부적으로 이 메소드를 실행함
    public void Destroy(GameObject gameObject)
    {
        string prefabId = gameObject.name.Replace("(Clone)", "").Trim();

        if (!poolDict.ContainsKey(prefabId))
        {
            poolDict.Add(prefabId, new Queue<GameObject>());
        }

        gameObject.SetActive(false);
        poolDict[prefabId].Enqueue(gameObject);
    }
}