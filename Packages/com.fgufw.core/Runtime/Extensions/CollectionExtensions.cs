using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FGUFW
{
    public static class CollectionExtensions
    {
        public static T Random<T>(this IList<T> self)
        {
            if (self == null || self.Count == 0)
            {
                return default;
            }
            int idx = UnityEngine.Random.Range(0, self.Count);
            return self[idx];
        }

        public static int IndexOf<T>(this T[] self, T val, int startIndex = 0)
        {
            if (self != null)
            {
                return Array.IndexOf<T>(self, val, startIndex);
            }
            return -1;
        }

        public static int IndexOf<T>(this T[] self, Predicate<T> match, int startIndex = 0, int length = 0)
        {
            if (self != null)
            {
                if (length == 0) length = self.Length;

                for (int i = startIndex; i < length; i++)
                {
                    var t_obj = self[i];
                    if (match(t_obj)) return i;
                }
            }
            return -1;
        }

        public static T Find<T>(this T[] self, Predicate<T> match)
        {
            if (self != null)
            {
                int length = self.Length;
                for (int i = 0; i < length; i++)
                {
                    var t_obj = self[i];
                    if (match(t_obj)) return t_obj;
                }
            }
            return default(T);
        }


        /// <summary>
        /// 按权重随机
        /// </summary>
        public static T RandomByWeight<T>(this IEnumerable<T> collection, Func<T, float> getWeight)
        {
            float maxValue = 0;
            foreach (T item in collection)
            {
                maxValue += getWeight(item);
            }
            float val = UnityEngine.Random.Range(0, maxValue);
            float weight = 0;
            foreach (T item in collection)
            {
                weight += getWeight(item);
                if (val < weight)
                {
                    return item;
                }
            }
            return default(T);
        }

        public static int RandomIndexByWeight(this IEnumerable<int> weights)
        {
            int index = -1;
            float length = 0;
            foreach (var item in weights)
            {
                length += item;
            }
            float val = UnityEngine.Random.Range(0,length);
            foreach (var item in weights)
            {
                index++;
                if(item==0)continue;
                if(val<item)break;
                val-=item;
            }
            return index;
        }

        public static T[] Copy<T>(this T[] self, int length)
        {
            if (self == null || self.Length == 0)
            {
                return null;
            }
            T[] newArray = new T[length];
            Array.Copy(self, newArray, length);
            return newArray;
        }

        public static U[] To<T, U>(this T[] self, Func<T, U> convert)
        {
            var length = self.Length;
            U[] array = new U[length];
            for (int i = 0; i < length; i++)
            {
                array[i] = convert(self[i]);
            }
            return array;
        }

        public static void RemoveAtSwapBack<T>(this List<T> self, int index)
        {
            int backIndex = self.Count - 1;
            self[index] = self[backIndex];
            self.RemoveAt(backIndex);
        }

        public static void RemoveSwapBack<T>(this List<T> self, T item)
        {
            int index = self.IndexOf(item);
            if (index == -1) return;
            self.RemoveAtSwapBack(index);
        }

        public static void RemoveSwapBack<T>(this List<T> self, Predicate<T> match)
        {
            int index = self.FindIndex(match);
            if (index == -1) return;
            int backIndex = self.Count - 1;
            self[index] = self[backIndex];
            self.RemoveAt(backIndex);
        }

        public static void ReplaceAllData<T>(this List<T> self, T[] array)
        {
            int length = array == null ? 0 : array.Length;

            for (int i = self.Count; i > length; i--)
            {
                self.RemoveAt(i - 1);
            }

            for (int i = 0; i < length; i++)
            {
                if (i < self.Count)
                {
                    self[i] = array[i];
                }
                else
                {
                    self.Add(array[i]);
                }
            }
        }

        public static void Set<K, V>(this Dictionary<K, V> self, K key, V value)
        {
            if (!self.TryAdd(key, value))
            {
                self[key] = value;
            }
        }

        public static V Get<K, V>(this Dictionary<K, V> self, K key)
        {
            V v = default;
            self.TryGetValue(key, out v);

            return v;
        }

        public static V GetOrNew<K, V>(this Dictionary<K, V> self, K key) where V:class,new()
        {
            V v = default;
            if(!self.TryGetValue(key, out v))
            {
                v = new V();
                self.Add(key,v);
            }

            return v;
        }

        public static void MoveTo<T>(this List<T> self, List<T> ls)
        {
            ls.Clear();
            foreach (var item in self)
            {
                ls.Add(item);
            }
            self.Clear();
        }

        /// <summary>
        /// 洗牌 打乱顺序 然后从头到尾就是一种随机不重复效果
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        public static void Shuffle<T>(this IList<T> self)
        {
            for (int i = 0; i < self.Count; i++)
            {
                int idx = fg.random(0, self.Count);
                var temp = self[i];
                self[i] = self[idx];
                self[idx] = temp;
            }
        }


        public static void Sort<T>(this T[] self, Comparison<T> comparison)
        {
            Array.Sort(self, comparison);
        }

        public static int LastIndex<T>(this T[] self)
        {
            return self.Length-1;
        }

        public static T Lerp<T>(this T[] self,float rate)
        {
            var length = self.Length;
            int idx = Mathf.Clamp((int)(rate*length),0,length-1);
            
            return self[idx];
        }

        public static T Lerp<T>(this List<T> self,float rate)
        {
            var length = self.Count;
            int idx = Mathf.Clamp((int)(rate*length),0,length-1);
            
            return self[idx];
        }

        public static T Lerp<T>(this T[] self,float rate,Func<T,T,float,T> onLerp)
        {
            var length = self.Length;
            int idx = Mathf.Clamp((int)(rate*length),0,length-1);
            int nextIdx = Mathf.Clamp(idx+1,0,length-1);
            var itemLength = 1f/length;
            rate = rate%itemLength / itemLength;
            return onLerp(self[idx],self[nextIdx],rate);
        }

        public static T Lerp<T>(this List<T> self,float rate,Func<T,T,float,T> onLerp)
        {
            var length = self.Count;
            int idx = Mathf.Clamp((int)(rate*length),0,length-1);
            int nextIdx = Mathf.Clamp(idx+1,0,length-1);
            var itemLength = 1f/length;
            rate = rate%itemLength / itemLength;
            return onLerp(self[idx],self[nextIdx],rate);
        }

    }
}
