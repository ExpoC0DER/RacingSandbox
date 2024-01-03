using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace _game.Scripts.Saving
{
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField] private List<TKey> _keys = new List<TKey>();
        [SerializeField] private List<TValue> _values = new List<TValue>();

        public void OnBeforeSerialize()
        {
            _keys.Clear();
            _values.Clear();

            foreach (KeyValuePair<TKey, TValue> pair in this)
            {
                _keys.Add(pair.Key);
                _values.Add(pair.Value);
            }
        }
        public void OnAfterDeserialize()
        {
            Clear();

            if (_keys.Count != _values.Count)
            {
                Debug.LogError($"Something fucked up while trying to deserialize SerializableDictionary and keys count({_keys.Count}) and value count({_values.Count}) aren't same!");
                return;
            }

            for(int i = 0; i < _keys.Count; i++)
            {
                Add(_keys[i], _values[i]);
            }
        }
    }
}
