using System;
using System.Collections.Generic;
using UnityEngine;

namespace FGUFW
{
    [Serializable]
    public class Table<K, V> : Dictionary<K, V>, ISerializationCallbackReceiver
    {
#if UNITY_EDITOR
        [Serializable]
        public class ItemData
        {
            public K Key;
            public V Value;
        }

        [Header("ReadOnly")]
        public List<ItemData> Items = new List<ItemData>();
#endif

        public Table(Dictionary<K, V> dict) : base(dict)
        {
            
        }
        
        public Table()
        {

        }

        // 序列化前调用：将字典数据存入两个列表
        public void OnBeforeSerialize()
        {

#if UNITY_EDITOR
            Items.Clear();
            foreach (var (k, v) in this)
            {
                Items.Add(new ItemData
                {
                    Key = k,
                    Value = v,
                });
            }
#endif
        }

        // 反序列化后调用：根据两个列表重建字典
        public void OnAfterDeserialize()
        {
            // this.Clear();
            
        }
    }
}