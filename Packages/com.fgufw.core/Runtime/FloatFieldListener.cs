using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FGUFW
{
    [Serializable]
    public sealed class FloatFieldListener
    {
        [SerializeField]
        private float _data;
        private Action<float> _listener;

        public float Data
        {
            get
            {
                return _data;
            }
            set
            {
                _data = value;

                if(_listener!=default)
                {
                    _listener(_data);
                }
            }
        }

        public void Bind(Action<float> callback)
        {
            _listener += callback;
            callback(Data);
        }

        public void Unbind(Action<float> callback)
        {
            _listener -= callback;
        }

        public IEnumerator Set(float val,float duration=0.5f)
        {
            var oldVal = Data;
            var newVal = val;
            float t = 0;
            while (t<duration)
            {
                var v = Mathf.Lerp(oldVal,newVal,t/duration);
                Data = v;
                yield return default;
                t += Time.deltaTime;
            }   

            Data = val;
        }

    }
}