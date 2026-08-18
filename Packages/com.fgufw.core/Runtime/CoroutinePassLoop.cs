using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace FGUFW
{
    [Serializable]
    public class CoroutinePassLoop
    {
        [SerializeField]
        private Item _currentPass;

        [SerializeField]
        private List<Item> _items = new List<Item>();
        private int _loopIndex;

        public void Add(Func<IEnumerator> getPass)
        {
            var item = new Item() { GetPass = getPass, PassName = getPass.Method.Name };

            _items.Add(item);
        }

        public IEnumerator Execute()
        {
            _loopIndex = 0;
            while (true)
            {
                _loopIndex = _loopIndex % _items.Count;
                for (int i = 0; i < _items.Count; i++)
                {
                    _currentPass = _items[_loopIndex];

                    yield return _currentPass.GetPass();

                    _loopIndex++;
                }
                yield return default;
            }
        }

        public void SetNext(string methodName)
        {
            var idx = _items.FindIndex(data => data.PassName == methodName);

            if (idx == -1) return;

            _loopIndex = idx - 1 + _items.Count;
        }



        [Serializable]
        public class Item
        {
            public Func<IEnumerator> GetPass;

            [ReadOnly]
            public string PassName;
        }

    }

}