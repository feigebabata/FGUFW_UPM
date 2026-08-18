using System;
using UnityEngine;
using UnityEngine.Pool;

namespace FGUFW
{
    public static class ComponentExtensions
    {
        public static void AutoRefField(this Component self)
        {
            if (self == null)
            {
                throw new ArgumentNullException(nameof(self));
            }

            var targetCache = ListPool<Transform>.Get();
            var similarValues = ListPool<float>.Get();

            try
            {
                var fields = self.GetType().GetFieldsByCache(TypeExtensions.PUBLIC_MEMBER);
                foreach (var field in fields)
                {
                    if (field.IsInitOnly)
                    {
                        continue;
                    }

                    var fieldType = field.FieldType;
                    if (fieldType != typeof(GameObject) && !typeof(Component).IsAssignableFrom(fieldType))
                    {
                        continue;
                    }

                    targetCache.Clear();
                    similarValues.Clear();
                    self.transform.FindSimilar(field.Name, targetCache, similarValues);

                    foreach (var target in targetCache)
                    {
                        object value = fieldType == typeof(GameObject) ? target.gameObject : target.GetComponent(fieldType);
                        if (value == null)
                        {
                            continue;
                        }

                        field.SetValue(self, value);
                        break;
                    }
                }
            }
            finally
            {
                ListPool<Transform>.Release(targetCache);
                ListPool<float>.Release(similarValues);
            }
        }

        public static Transform GetChild(this Component self, int childIndex)
        {
            return self.transform.GetChild(childIndex);
        }

        public static T GetChild<T>(this Component self, int childIndex)
        {
            return self.transform.GetChild(childIndex).GetComponent<T>();
        }

        public static Transform GetChild(this Component self, string path)
        {
            return self.transform.Find(path);
        }

        public static T GetChild<T>(this Component self, string path)
        {
            var child = self.transform.Find(path);
            return child == null ? default : child.GetComponent<T>();
        }

        public static void SetActive(this Component self, bool active)
        {
            self.gameObject.SetActive(active);
        }
    }
}
