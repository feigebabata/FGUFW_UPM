using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Pool;

namespace FGUFW
{
    public static class TransformExtensions
    {
        public static RectTransform AsRT(this Transform t)
        {
            return t as RectTransform;
        }
        public static RectTransform GetChildRT(this Transform t,int index)
        {
            return t.GetChild(index) as RectTransform;
        }

        public static IEnumerator MoveWorld(this Transform transform,Vector3 endPos,float time,AnimationCurve curve=default)
        {
            float startTime = Time.time;
            var startPos = transform.position;
            while (Time.time - startTime < time)
            {
                float t = (Time.time - startTime) / time;
                if (curve != default)
                {
                    t = curve.Evaluate(t);
                }
                transform.position = Vector3.LerpUnclamped(startPos, endPos, t);
                yield return default;
            }
            transform.position = endPos;
        }

        public static IEnumerator MoveLocal(this Transform transform,Vector3 endPos,float time,AnimationCurve curve=default)
        {
            float startTime = Time.time;
            var startPos = transform.localPosition;
            while (Time.time-startTime<time)
            {
                float t = (Time.time-startTime)/time;
                if (curve != default)
                {
                    t = curve.Evaluate(t);
                }
                transform.localPosition = Vector3.LerpUnclamped(startPos,endPos,t);
                yield return default;
            }
            transform.localPosition = endPos;
        }

        public static IEnumerator ScaleLocal(this Transform transform,Vector3 endScale,float time,AnimationCurve curve=default)
        {
            float startTime = Time.time;
            var startScale = transform.localScale;
            while (Time.time - startTime < time)
            {
                float t = (Time.time - startTime) / time;
                if (curve != default)
                {
                    t = curve.Evaluate(t);
                }
                transform.localScale = Vector3.LerpUnclamped(startScale, endScale, t);
                yield return default;
            }
            transform.localScale = endScale;
        }

        public static IEnumerator RotateLocal(this Transform transform,Vector3 endAngle,float time,AnimationCurve curve=default)
        {
            float startTime = Time.time;
            Quaternion endQ = Quaternion.Euler(endAngle);
            var startQ = transform.localRotation;
            while (Time.time - startTime < time)
            {
                float t = (Time.time - startTime) / time;
                if (curve != default)
                {
                    t = curve.Evaluate(t);
                }
                transform.localRotation = Quaternion.LerpUnclamped(startQ, endQ, t);
                yield return default;
            }
            transform.localRotation = endQ;
        }

        public static IEnumerator RotateLoopLocal(this Transform self,Vector3 axis,float angle,float time,AnimationCurve curve=default)
        {
            float startTime = Time.time;
            while (Time.time - startTime < time)
            {
                float t = (Time.time - startTime) / time;
                if (curve != default)
                {
                    t = curve.Evaluate(t);
                }
                var a = Mathf.LerpUnclamped(0, angle, t);
                self.localRotation = Quaternion.AngleAxis(a, axis);
                yield return default;
            }
            self.localRotation = Quaternion.AngleAxis(angle, axis);
        }

        public static IEnumerator RotateWorld(this Transform transform,Vector3 endAngle,float time,AnimationCurve curve=default)
        {
            float startTime = Time.time;
            Quaternion endQ = Quaternion.Euler(endAngle);
            var startQ = transform.rotation;
            while (Time.time-startTime<time)
            {
                float t = (Time.time-startTime)/time;
                if (curve != default)
                {
                    t = curve.Evaluate(t);
                }
                transform.rotation = Quaternion.LerpUnclamped(startQ,endQ,t);
                yield return default;
            }
            transform.rotation = endQ;
        }

        public static void Foreach<VALUE>(this Transform transform,IEnumerable list,Action<Transform,VALUE> callback)
        {
            int idx = 0;
            if(list!=null)
            {
                var enumerator = list.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    Transform item_t = transform.GetOrCreateChild(idx);
                    callback?.Invoke(item_t,(VALUE)enumerator.Current);
                    item_t.gameObject.SetActive(true);
                    idx++;
                }
            }
            for (int i = idx; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
        }

        public static void Foreach<COMP,VALUE>(this Transform transform,IEnumerable list,Action<COMP,VALUE> callback)
        {
            int idx = 0;
            if(list!=null)
            {
                var enumerator = list.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    Transform item_t = transform.GetOrCreateChild(idx);
                    callback?.Invoke(item_t.GetComponent<COMP>(),(VALUE)enumerator.Current);
                    item_t.gameObject.SetActive(true);
                    idx++;
                }
            }
            for (int i = idx; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
        }

        public static void For(this Transform transform,int count,Action<int,Transform> callback)
        {
            for (int i = 0; i < count; i++)
            {
                Transform item_t = transform.GetOrCreateChild(i);
                callback?.Invoke(i,item_t);
                item_t.gameObject.SetActive(true);
            }
            for (int i = count; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
        }

        public static void For<T>(this Transform transform,int count,Action<int,T> callback)
        {
            for (int i = 0; i < count; i++)
            {
                Transform item_t = transform.GetOrCreateChild(i);
                callback(i,item_t.GetComponent<T>());
                item_t.gameObject.SetActive(true);
            }
            for (int i = count; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
        }

        public static void For<T>(this Transform transform,Action<int,T> callback)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform item_t = transform.GetChild(i);
                callback(i,item_t.GetComponent<T>());
            }
        }

        public static string FullPath(this Transform t)
        {
            StringBuilder s = new StringBuilder();
            do
            {
                s.Insert(0,$"\\{t.name}");
                t = t.parent;
            } 
            while (t!=null);
            return s.ToString();
        }

        public static void Sort<T>(this Transform t,Comparison<T> comparison) where T:MonoBehaviour
        {
            List<T> childs = ListPool<T>.Get();
            foreach (Transform item in t)
            {
                childs.Add(item.GetComponent<T>());
            }
            childs.Sort(comparison);
            for (int i = 0; i < childs.Count; i++)
            {
                childs[i].transform.SetSiblingIndex(i);
            }
        }

        public static Transform RandomChild(this Transform t)
        {
            int idx = UnityEngine.Random.Range(0,t.childCount);
            return t.GetChild(idx);
        }

        public static T RandomChild<T>(this Transform t)
        {
            int idx = UnityEngine.Random.Range(0,t.childCount);
            return t.GetChild(idx).GetComponent<T>();
        }

        public static Transform GetOrCreateChild(this Transform t,int index,string createName=null)
        {
            while (index >= t.childCount)
            {
                var child = GameObject.Instantiate(t.GetChild(0).gameObject,t);
                if(createName!=null)child.name=createName;
            }
            return t.GetChild(index);
        }

        public static T GetOrCreateChild<T>(this Transform t,int index,string createName=null)
        {
            while (index >= t.childCount)
            {
                var child = GameObject.Instantiate(t.GetChild(0).gameObject,t);
                if(createName!=null)child.name=createName;
            }
            return t.GetChild(index).GetComponent<T>();
        }

        public static Transform First(this Transform self)
        {
            if(self.childCount>0)
            {
                return self.GetChild(0);
            }
            return default;
        }

        public static Transform Last(this Transform self)
        {
            if(self.childCount>0)
            {
                return self.GetChild(self.childCount-1);
            }
            return default;
        }

        public static void ToList(this Transform self,List<Transform> cache)
        {
            cache.Clear();
            foreach (Transform item in self)
            {
                cache.Add(item);
            }
        }

        public static void SetChilidsParent(this Transform self,Transform parent,bool worldStay=true)
        {
            while (self.childCount>0)
            {
                self.GetChild(0).SetParent(parent,worldStay);
            }
        }


        public static void DestroyChilds(this Transform self)
        {
            var des = ListPool<Transform>.Get();

            foreach (Transform item in self)
            {
                des.Add(item);
            }

            foreach (var item in des)
            {
                GameObject.Destroy(item.gameObject);
            }
        }

        public static void DestroyChilds(this Transform self,Predicate<Transform> match)
        {
            var des = ListPool<Transform>.Get();

            foreach (Transform item in self)
            {
                des.Add(item);
            }

            foreach (var item in des)
            {
                if(match(item))
                {
                    GameObject.Destroy(item.gameObject);
                }
            }
        }

        public static T Find<T>(this Transform self,Predicate<T> match) where T:Component
        {
            foreach (Transform item in self)
            {
                T comp = item.GetComponent<T>();
                if(comp!=default && match(comp)) return comp;
            }
            return default;
        }

        public static void FindSimilar(this Transform self,string targetName,List<Transform> targetCache,List<float> similarValues)
        {
            for (int i = 0; i < self.childCount; i++)
            {
                var item = self.GetChild(i);
                var similarVal = targetName.Similarity(item.name);
                if(similarVal>0)
                {
                    var insertIdx = -1;
                    for (int j = 0; j < similarValues.Count; j++)
                    {
                        if(similarVal>similarValues[j])
                        {
                            insertIdx = j;
                            break;
                        }
                    }

                    if(insertIdx==-1)
                    {
                        targetCache.Add(item);
                        similarValues.Add(similarVal);
                    }
                    else
                    {
                        targetCache.Insert(insertIdx,item);
                        similarValues.Insert(insertIdx,similarVal);
                    }

                }
                item.FindSimilar(targetName,targetCache,similarValues);
            }
        }

        /// <summary>
        /// 逐级向上查找
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="includeInactive">仅限活动的对象</param>
        /// <returns></returns>
        public static T GetObjectFormUpOrSelf<T>(this Transform self, bool includeInactive)
        {
            T obj = default;

            do
            {
                if(includeInactive)
                {
                    if(self.gameObject.activeSelf)
                    {
                        obj = self.GetComponent<T>();
                    }
                }
                else
                {
                    obj = self.GetComponent<T>();
                }

                if(!EqualityComparer<T>.Default.Equals(obj, default))
                {
                    return obj;
                }

                if(self.GetSiblingIndex()==0)
                {
                    self = self.parent;
                }
                else if(self.parent!=default)
                {
                    self = self.parent.GetChild(self.GetSiblingIndex()-1);
                }
            }
            while (self.parent!=default);

            return default;
        }
                
        public static T GetObjectFormUp<T>(this Transform self, bool includeInactive)
        {
            T obj = default;

            do
            {

                if(self.GetSiblingIndex()==0)
                {
                    self = self.parent;
                }
                else if(self.parent!=default)
                {
                    self = self.parent.GetChild(self.GetSiblingIndex()-1);
                }
                
                if(includeInactive)
                {
                    if(self.gameObject.activeSelf)
                    {
                        obj = self.GetComponent<T>();
                    }
                }
                else
                {
                    obj = self.GetComponent<T>();
                }

                if(!EqualityComparer<T>.Default.Equals(obj, default))
                {
                    return obj;
                }
            }
            while (self.parent!=default);

            return default;
        }

    }
}
