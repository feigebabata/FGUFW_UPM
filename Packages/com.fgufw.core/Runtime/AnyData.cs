using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FGUFW
{
    
    /// <summary>
    /// 方便编辑器查看的object类型扩展
    /// </summary>
    [Serializable]
    public class AnyData
    {
        private object _data;

        [SerializeField]
        private GameObject m_Data;

        [SerializeField]
        private Component Data;

        public void Set(object data)
        {
            _data = data;

            if (data == default)
            {
                m_Data = default;
                Data = default;
                return;
            }

            if (data is Component)
            {
                Data = data as Component;
                m_Data = default;
            }
            else if (data is GameObject)
            {
                m_Data = data as GameObject;
                Data = default;
            }
            else
            {
                m_Data = default;
                Data = default;
            }
        }

        public T Get<T>()
        {
            if (_data == default)
            {
                return default;
            }
            return (T)_data;
        }
    }

}