using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

namespace Multiplayer
{
    // Generic Animator parameter synchronizer for PUN.
    // - Auto-discovery syncs all non-trigger parameters (float/int/bool)
    // - Optional manual list to control which parameters to sync
    // - Triggers require explicit SetTrigger forwarding via this component
    [RequireComponent(typeof(PhotonView))]
    public class AnimatorParamSync : MonoBehaviour, IPunObservable
    {
        public enum ParamType { Float, Int, Bool, Trigger }

        [Serializable]
        public class Param
        {
            public string Name;
            public ParamType Type = ParamType.Float;
        }

        [Tooltip("If true, collect all non-trigger parameters from the first Animator found.")]
        public bool AutoDiscover = true;

        [Tooltip("Manual parameter list (used in addition to auto-discovery if enabled).")]
        public List<Param> Parameters = new List<Param>();

        private Animator _animator;
        private PhotonView _view;

        // Triggers fired this frame (local owner only)
        private readonly HashSet<string> _pendingTriggers = new HashSet<string>(StringComparer.Ordinal);

        private void Awake()
        {
            _view = GetComponent<PhotonView>();
            _animator = GetComponentInChildren<Animator>();
            if (AutoDiscover && _animator)
            {
                DiscoverParameters(_animator);
            }
        }

        private void DiscoverParameters(Animator animator)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (var p in Parameters)
            {
                if (!string.IsNullOrEmpty(p.Name)) names.Add(p.Name);
            }
            foreach (var ap in animator.parameters)
            {
                if (ap.type == AnimatorControllerParameterType.Trigger) continue;
                if (names.Contains(ap.name)) continue;
                var type = ParamType.Float;
                switch (ap.type)
                {
                    case AnimatorControllerParameterType.Float: type = ParamType.Float; break;
                    case AnimatorControllerParameterType.Int: type = ParamType.Int; break;
                    case AnimatorControllerParameterType.Bool: type = ParamType.Bool; break;
                }
                Parameters.Add(new Param { Name = ap.name, Type = type });
                names.Add(ap.name);
            }
        }

        // Public helper to forward triggers through network in a generic way.
        public void SetTrigger(string name)
        {
            if (!_animator) return;
            _animator.SetTrigger(name);
            if (_view != null && _view.IsMine)
            {
                _pendingTriggers.Add(name);
            }
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (_animator == null)
                return;

            if (stream.IsWriting)
            {
                // Parameters
                stream.SendNext(Parameters.Count);
                for (int i = 0; i < Parameters.Count; i++)
                {
                    var p = Parameters[i];
                    stream.SendNext(p.Name);
                    stream.SendNext((int)p.Type);
                    switch (p.Type)
                    {
                        case ParamType.Float: stream.SendNext(_animator.GetFloat(p.Name)); break;
                        case ParamType.Int: stream.SendNext(_animator.GetInteger(p.Name)); break;
                        case ParamType.Bool: stream.SendNext(_animator.GetBool(p.Name)); break;
                        case ParamType.Trigger:
                            // Send trigger fired state for this frame (true once)
                            bool fired = _pendingTriggers.Remove(p.Name);
                            stream.SendNext(fired);
                            break;
                    }
                }
            }
            else
            {
                int count = (int)stream.ReceiveNext();
                for (int i = 0; i < count; i++)
                {
                    string name = (string)stream.ReceiveNext();
                    var type = (ParamType)(int)stream.ReceiveNext();
                    switch (type)
                    {
                        case ParamType.Float: _animator.SetFloat(name, (float)stream.ReceiveNext()); break;
                        case ParamType.Int: _animator.SetInteger(name, (int)stream.ReceiveNext()); break;
                        case ParamType.Bool: _animator.SetBool(name, (bool)stream.ReceiveNext()); break;
                        case ParamType.Trigger:
                            bool fired = (bool)stream.ReceiveNext();
                            if (fired) _animator.SetTrigger(name);
                            break;
                    }
                }
            }
        }
    }
}

