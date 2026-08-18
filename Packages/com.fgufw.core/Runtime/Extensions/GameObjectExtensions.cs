using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FGUFW
{
    public static class GameObjectExtensions
    {
        public static GameObject Copy(this GameObject self, Transform parent)
        {
            return GameObject.Instantiate(self, parent);
        }

        public static T Copy<T>(this GameObject self, Transform parent) where T : Component
        {
            return GameObject.Instantiate(self, parent).GetComponent<T>();
        }
        public static GameObject Copy(this GameObject self)
        {
            return GameObject.Instantiate(self, self.transform.parent);
        }

        public static T Copy<T>(this GameObject self) where T : Component
        {
            return GameObject.Instantiate(self, self.transform.parent).GetComponent<T>();
        }


        public static bool IsNull(this UnityEngine.Object self)
        {
            return self == null || !(self is UnityEngine.Object);
        }

        public static Transform GetChild(this GameObject self, int childIndex)
        {
            return self.transform.GetChild(childIndex);
        }

        public static void SetLayer(this GameObject self, string layerName)
        {
            var layer = LayerMask.NameToLayer(layerName);
            setLayer(self.transform, layer);
        }

        private static void setLayer(Transform parent, int layer)
        {
            parent.gameObject.layer = layer;

            foreach (Transform item in parent)
            {
                setLayer(item, layer);
            }
        }
        

        public static void Reaction(this GameObject self)
        {
            self.SetActive(false);
            self.SetActive(true);
        }

    }
}