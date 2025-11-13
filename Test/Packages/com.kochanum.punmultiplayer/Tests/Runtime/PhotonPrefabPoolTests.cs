using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace Multiplayer.Tests
{
    public class PhotonPrefabPoolTests
    {
        private T GetPrivateField<T>(object obj, string name)
        {
            var f = obj.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            return (T)f.GetValue(obj);
        }

        [Test]
        public void Instantiate_UnknownPrefab_ReturnsNull()
        {
            var go = new GameObject("PoolTest");
            try
            {
                var pool = go.AddComponent<PhotonPrefabPool>();
                var inst = pool.Instantiate("NoSuchPrefab", Vector3.zero, Quaternion.identity);
                Assert.IsNull(inst);
            }
            finally
            {
                GameObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void Instantiate_RegisteredPrefab_ReturnsInstance_WithPhotonView()
        {
            var go = new GameObject("PoolTest");
            var prefab = new GameObject("DummyPrefab");
            try
            {
                var pool = go.AddComponent<PhotonPrefabPool>();
                // Reflect into private 'entries' to register a prefab
                var entriesField = typeof(PhotonPrefabPool).GetField("entries", BindingFlags.Instance | BindingFlags.NonPublic);
                var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(entriesField.FieldType.GetGenericArguments()[0]));
                // Create entry instance
                var entryType = entriesField.FieldType.GetGenericArguments()[0];
                var entry = Activator.CreateInstance(entryType);
                entryType.GetField("Name").SetValue(entry, "Dummy");
                entryType.GetField("Prefab").SetValue(entry, prefab);
                list.Add(entry);
                entriesField.SetValue(pool, list);

                var inst = pool.Instantiate("Dummy", Vector3.one, Quaternion.identity);
                Assert.IsNotNull(inst);
                Assert.IsNotNull(inst.GetComponent<Photon.Pun.PhotonView>(), "PhotonView should be added");
                GameObject.DestroyImmediate(inst);
            }
            finally
            {
                GameObject.DestroyImmediate(prefab);
                GameObject.DestroyImmediate(go);
            }
        }
    }
}

