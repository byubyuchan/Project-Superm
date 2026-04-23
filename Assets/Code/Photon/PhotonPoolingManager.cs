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
    }

    // 주소가 다르더라도 이름이 같으면 오류가 생기는 문제를 해결하기위한 개선된 풀링
    public GameObject Instantiate(string prefabId, Vector3 position, Quaternion rotation)
    {
        string actualKey = prefabId;

        if (!poolDict.ContainsKey(actualKey))
        {
            foreach (string key in poolDict.Keys)
            {
                if (key.EndsWith("/" + actualKey) || actualKey.EndsWith("/" + key))
                {
                    actualKey = key;
                    break;
                }
            }
        }

        if (!poolDict.ContainsKey(actualKey))
        {
            poolDict.Add(actualKey, new Queue<GameObject>());
        }

        if (poolDict[actualKey].Count > 0)
        {
            GameObject obj = poolDict[actualKey].Dequeue();
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            return obj;
        }

        GameObject prefab = Resources.Load<GameObject>(prefabId);
        GameObject newObj = Object.Instantiate(prefab, position, rotation);

        newObj.name = actualKey;

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
            foreach (string key in poolDict.Keys)
            {
                if (key.EndsWith("/" + prefabId))
                {
                    prefabId = key;
                    break;
                }
            }
            if (!poolDict.ContainsKey(prefabId))
            {
                poolDict.Add(prefabId, new Queue<GameObject>());
            }
        }

        gameObject.SetActive(false);
        poolDict[prefabId].Enqueue(gameObject);
    }
}