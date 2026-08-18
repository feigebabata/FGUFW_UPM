using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FGUFW
{
    [Serializable]
    public sealed class FieldListener<T>
    {
        [SerializeField]
        private AnyData _data = new AnyData();
        private Action<T> _listener;

        public T Get()
        {
            return _data.Get<T>();
        }

        public void Set(T val)
        {
            _data.Set(val);

            if(_listener!=default)
            {
                _listener(val);
            }
        }

        public void Bind(Action<T> callback)
        {
            _listener += callback;
            callback(Get());
        }

        public void Unbind(Action<T> callback)
        {
            _listener -= callback;
        }

        public IEnumerator Set(T val,Func<T,T,float,T> lerp,float duration=0.5f)
        {
            var oldVal = Get();
            var newVal = val;
            float t = 0;
            while (t<duration)
            {
                var v = lerp(oldVal,newVal,t/duration);
                Set(v);
                yield return default;
                t += Time.deltaTime;
            }
            Set(val);
        }

    }
}