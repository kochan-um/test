using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace Multiplayer
{
    // Connects to Photon, joins/creates a room, and spawns the local player using the custom prefab pool.
    public class PlayerSpawner : MonoBehaviourPunCallbacks
    {
        [Tooltip("Prefab ID registered in PhotonPrefabPool entries.")]
        public string playerPrefabId = "PlayerArmature";

        [Tooltip("Optional spawn points; if empty, uses Vector3.zero.")]
        public Transform[] spawnPoints;

        [Tooltip("Max players per room.")]
        public byte maxPlayers = 8;

        private bool _spawned;

        private void Awake()
        {
            // Ensure a prefab pool is set
            if (PhotonNetwork.PrefabPool == null)
            {
                var pool = FindObjectOfType<PhotonPrefabPool>();
                if (pool == null)
                {
                    var go = new GameObject("PhotonPrefabPool");
                    go.AddComponent<PhotonPrefabPool>();
                    Debug.LogWarning("[PlayerSpawner] Created empty PhotonPrefabPool. Please add entries via inspector.");
                }
            }
        }

        private void Start()
        {
            ConnectIfNeeded();
        }

        private void ConnectIfNeeded()
        {
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.GameVersion = Application.version;
            PhotonNetwork.SendRate = 30;            // network send rate
            PhotonNetwork.SerializationRate = 15;  // OnPhotonSerializeView calls per second

            if (!PhotonNetwork.IsConnected)
            {
                PhotonNetwork.ConnectUsingSettings();
            }
            else if (PhotonNetwork.InRoom)
            {
                TrySpawn();
            }
            else if (PhotonNetwork.IsConnectedAndReady)
            {
                JoinDefaultRoom();
            }
        }

        public override void OnConnectedToMaster()
        {
            JoinDefaultRoom();
        }

        private void JoinDefaultRoom()
        {
            var roomOptions = new RoomOptions { MaxPlayers = maxPlayers };
            PhotonNetwork.JoinOrCreateRoom("Default", roomOptions, TypedLobby.Default);
        }

        public override void OnJoinedRoom()
        {
            TrySpawn();
        }

        private void TrySpawn()
        {
            if (_spawned) return;

            var spawn = Vector3.zero;
            var rot = Quaternion.identity;
            if (spawnPoints != null && spawnPoints.Length > 0)
            {
                var t = spawnPoints[Random.Range(0, spawnPoints.Length)];
                if (t) { spawn = t.position; rot = t.rotation; }
            }

            var go = PhotonNetwork.Instantiate(playerPrefabId, spawn, rot);
            if (go == null)
            {
                Debug.LogError($"[PlayerSpawner] Failed to instantiate '{playerPrefabId}'. Check PhotonPrefabPool entries.");
                return;
            }

            _spawned = true;
        }
    }
}

