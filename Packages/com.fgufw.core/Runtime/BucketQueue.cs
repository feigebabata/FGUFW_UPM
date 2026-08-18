using System;
using System.Collections;
using System.Collections.Generic;

namespace FGUFW
{
    public class BucketQueue<T> : IEnumerable<T>
    {
        private T[] _array;
        private int _lastIndex;
        private int _count;

        public int Count => _count;

        private BucketQueue() { }
        public BucketQueue(int length)
        {
            _array = new T[length];
        }

        public void Enqueue(T item)
        {
            if (_count < _array.Length)
            {
                _count++;
            }
            _array[_lastIndex] = item;

            _lastIndex = (_lastIndex + 1) % _array.Length;
        }

        public void Clear()
        {
            _lastIndex = 0;
            _count = 0;
        }

        public IEnumerator<T> GetEnumerator()
        {
            int firstIndex = (_lastIndex - _count + _array.Length)%_array.Length;
            for (int i = 0; i < _count; i++)
            {
                var idx = (firstIndex + i) % _array.Length;
                yield return _array[idx];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

    }
}