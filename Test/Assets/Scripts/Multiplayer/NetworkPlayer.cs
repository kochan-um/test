using Photon.Pun;
using UnityEngine;
using System.Linq;

namespace Multiplayer
{
    // Attachable to the player prefab at runtime by PhotonPrefabPool
    // Controls local/remote behavior and syncs transform state.
    public class NetworkPlayer : MonoBehaviourPun, IPunObservable
    {
        private Transform _transform;
        private Rigidbody _rb;

        // ThirdPersonController and input (Starter Assets)
        private MonoBehaviour _thirdPersonController;
        private MonoBehaviour _starterAssetsInputs;
        private Behaviour _playerInput; // UnityEngine.InputSystem.PlayerInput is a Behaviour

        // Remote interpolation targets
        private Vector3 _netPos;
        private Quaternion _netRot;
        private float _lerpSpeed = 12f;
        [SerializeField]
        private bool autoAddAnimatorSync = true;

        private void Awake()
        {
            _transform = transform;
            _rb = GetComponent<Rigidbody>();

            // Try to find Starter Assets components by type names to avoid hard dependency
            _thirdPersonController = GetComponentByName("ThirdPersonController");
            _starterAssetsInputs = GetComponentByName("StarterAssetsInputs");
            _playerInput = GetComponentByName("PlayerInput") as Behaviour;

            if (!photonView.IsMine)
            {
                // Disable local-only components on remotes
                if (_thirdPersonController) _thirdPersonController.enabled = false;
                if (_starterAssetsInputs) _starterAssetsInputs.enabled = false;
                if (_playerInput) _playerInput.enabled = false;
                if (_rb) _rb.isKinematic = true;
            }
            else
            {
                if (_rb) _rb.isKinematic = false;
                // Bind camera to this player if possible
                TryBindCamera();
                TryEnsureAnimatorSync();
            }
        }

        private void Update()
        {
            if (photonView.IsMine)
                return;

            // Smoothly interpolate remote motion
            _transform.position = Vector3.Lerp(_transform.position, _netPos, Time.deltaTime * _lerpSpeed);
            _transform.rotation = Quaternion.Slerp(_transform.rotation, _netRot, Time.deltaTime * _lerpSpeed);
        }

        private void TryEnsureAnimatorSync()
        {
            var anim = GetComponentInChildren<Animator>();
            if (!anim) return;

            var existing = GetComponent<AnimatorParamSync>();
            if (!existing && autoAddAnimatorSync)
            {
                var sync = gameObject.AddComponent<AnimatorParamSync>();
                sync.AutoDiscover = true;
            }
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(_transform.position);
                stream.SendNext(_transform.rotation);
            }
            else
            {
                _netPos = (Vector3)stream.ReceiveNext();
                _netRot = (Quaternion)stream.ReceiveNext();
            }
        }

        private MonoBehaviour GetComponentByName(string typeName)
        {
            var type = System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); } catch { return System.Array.Empty<System.Type>(); }
                })
                .FirstOrDefault(t => t.Name == typeName);

            if (type == null) return null;
            var comp = GetComponent(type) as MonoBehaviour;
            return comp;
        }

        private void TryBindCamera()
        {
            // Prefer PlayerCameraRoot if it exists under the player; fallback to CinemachineCameraTarget.
            Transform cameraRoot = FindChildRecursive(transform, "PlayerCameraRoot");
            if (cameraRoot == null)
                cameraRoot = FindChildRecursive(transform, "CinemachineCameraTarget");
            if (cameraRoot == null)
                cameraRoot = transform; // ultimate fallback

            // Find Cinemachine Virtual Camera in scene (separate from Main Camera)
            var vcamType = System.Type.GetType("Cinemachine.CinemachineVirtualCamera, Cinemachine");
            if (vcamType == null) return; // Cinemachine not installed

            object vcam = null;
            // Prefer an object explicitly named PlayerFollowCamera (Starter Assets 3PC default)
            var go = GameObject.Find("PlayerFollowCamera");
            if (go != null)
            {
                vcam = go.GetComponent(vcamType);
            }
            // Fallback: first vcam found in the scene
            if (vcam == null)
            {
                var findMethod = typeof(Object).GetMethods()
                    .FirstOrDefault(m => m.Name == "FindObjectOfType" && m.IsGenericMethodDefinition && m.GetParameters().Length == 0);
                if (findMethod != null)
                {
                    var generic = findMethod.MakeGenericMethod(vcamType);
                    vcam = generic.Invoke(null, null);
                }
            }
            if (vcam == null) return;

            // Set Follow and LookAt via reflection
            var followProp = vcamType.GetProperty("Follow");
            var lookAtProp = vcamType.GetProperty("LookAt");
            followProp?.SetValue(vcam, cameraRoot);
            lookAtProp?.SetValue(vcam, cameraRoot);
        }

        private static Transform FindChildRecursive(Transform root, string name)
        {
            if (root.name == name) return root;
            for (int i = 0; i < root.childCount; i++)
            {
                var t = root.GetChild(i);
                var r = FindChildRecursive(t, name);
                if (r != null) return r;
            }
            return null;
        }
    }
}
