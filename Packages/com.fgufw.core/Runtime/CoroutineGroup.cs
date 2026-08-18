using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace FGUFW
{
    public struct CoroutineGroup
    {
        private MonoBehaviour _mb;
        private List<Coroutine> _ls;

        public CoroutineGroup(MonoBehaviour mb)
        {
            _mb = mb;
            _ls = ListPool<Coroutine>.Get();
        }

        public Coroutine Start(IEnumerator enumerator)
        {
            if(_mb==default)
            {
                throw new InvalidOperationException("CoroutineGroup未正确初始化");
            }

            if(_ls==default)
            {
                throw new ObjectDisposedException("CoroutineGroup已释放");
            }

            var c = _mb.StartCoroutine(enumerator);
            _ls.Add(c);
            return c;
        }


        public IEnumerator WaitAllStop()
        {
            if(_mb==default)
            {
                throw new InvalidOperationException("CoroutineGroup未正确初始化");
            }

            if(_ls==default)
            {
                throw new ObjectDisposedException("CoroutineGroup已释放");
            }

            foreach (var item in _ls)
            {
                yield return item;
            }

            ListPool<Coroutine>.Release(_ls);
            _ls = default;
        }

    }

}
