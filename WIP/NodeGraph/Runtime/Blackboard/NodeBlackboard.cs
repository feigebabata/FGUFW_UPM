using System;
using System.Collections.Generic;

namespace FGUFW.NodeGraph
{
    public sealed class NodeBlackboard : INodeBlackboard
    {
        private readonly Dictionary<string, object> values = new Dictionary<string, object>(StringComparer.Ordinal);
        public event Action<string, object, object> ValueChanged;
        public int Count => values.Count;
        public IEnumerable<KeyValuePair<string, object>> Entries => values;

        public void Set<T>(string key, T value)
        {
            Set(key, (object)value);
        }

        public void Set(string key, object value)
        {
            ValidateKey(key);
            values.TryGetValue(key, out var previous);
            values[key] = value;
            ValueChanged?.Invoke(key, previous, value);
        }

        public T Get<T>(string key)
        {
            if (!TryGet<T>(key, out var value))
            {
                throw new KeyNotFoundException($"Blackboard key '{key}' is missing or not {typeof(T).Name}.");
            }

            return value;
        }

        public bool TryGet<T>(string key, out T value)
        {
            if (values.TryGetValue(key, out var stored))
            {
                if (stored is T typedValue)
                {
                    value = typedValue;
                    return true;
                }

                if (stored == null && (!typeof(T).IsValueType || Nullable.GetUnderlyingType(typeof(T)) != null))
                {
                    value = default;
                    return true;
                }
            }

            value = default;
            return false;
        }

        public bool Contains(string key)
        {
            return !string.IsNullOrWhiteSpace(key) && values.ContainsKey(key);
        }

        public bool Remove(string key)
        {
            return !string.IsNullOrWhiteSpace(key) && values.Remove(key);
        }

        public void Clear()
        {
            values.Clear();
        }

        private static void ValidateKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Blackboard key cannot be empty.", nameof(key));
            }
        }
    }
}
