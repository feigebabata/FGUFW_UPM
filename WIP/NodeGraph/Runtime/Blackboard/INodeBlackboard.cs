using System;
using System.Collections.Generic;

namespace FGUFW.NodeGraph
{
    public interface INodeBlackboard
    {
        event Action<string, object, object> ValueChanged;
        int Count { get; }

        IEnumerable<KeyValuePair<string, object>> Entries { get; }

        void Set<T>(string key, T value);
        void Set(string key, object value);
        T Get<T>(string key);
        bool TryGet<T>(string key, out T value);
        bool Contains(string key);
        bool Remove(string key);
        void Clear();
    }
}
