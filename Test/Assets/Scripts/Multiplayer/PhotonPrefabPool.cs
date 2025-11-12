using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

namespace Multiplayer
{
    // Custom prefab pool that avoids Resources and uses explicit prefab references.
    public class PhotonPrefabPool : MonoBehaviour, IPunPrefabPool
    {
        [Serializable]
        public class Entry
        {
            public string Name;          // prefabId used with PhotonNetwork.Instantiate
            public GameObject Prefab;    // referenced prefab (not in Resources)
        }

        [SerializeField]
        private List<Entry> entries = new List<Entry>();

        private Dictionary<string, GameObject> map;

        private void Awake()
        {
            BuildMap();
            PhotonNetwork.PrefabPool = this;
            DontDestroyOnLoad(gameObject);
        }

        private void BuildMap()
        {
            map = new Dictionary<string, GameObject>(StringComparer.Ordinal);
            foreach (var e in entries)
            {
                if (!string.IsNullOrWhiteSpace(e.Name) && e.Prefab != null)
                {
                    map[e.Name] = e.Prefab;
                }
            }
        }

        public GameObject Instantiate(string prefabId, Vector3 position, Quaternion rotation)
        {
            if (map == null || map.Count == 0)
                BuildMap();

            if (!map.TryGetValue(prefabId, out var prefab) || prefab == null)
            {
                Debug.LogError($"[PhotonPrefabPool] Prefab '{prefabId}' is not registered. Add it to the pool entries.");
                return null;
            }

            var go = GameObject.Instantiate(prefab, position, rotation);

            // Ensure a PhotonView exists so PUN can assign view IDs.
            var pv = go.GetComponent<PhotonView>();
            if (pv == null)
            {
                pv = go.AddComponent<PhotonView>();
            }

            // Ensure a NetworkPlayer exists to handle ownership & sync when the prefab is a player.
            if (go.GetComponent<NetworkPlayer>() == null)
            {
                go.AddComponent<NetworkPlayer>();
            }

            return go;
        }

        public void Destroy(GameObject gameObject)
        {
            GameObject.Destroy(gameObject);
        }
    }
}

