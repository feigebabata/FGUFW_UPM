using System;
using UnityEngine;

namespace FGUFW
{
    public static class RandomExtensions
    {
        public static float Range(this System.Random random,float min,float max)
        {
            float val = (float)random.NextDouble();
            float space = max-min;
            return min+space*val;
        }

        public static Vector2 RangeV2(this System.Random random,float min,float max)
        {
            Vector2 v2 = Vector2.zero;
            v2.x = random.Range(min,max);
            v2.y = random.Range(min,max);
            return v2;
        }

        public static Vector3 RangeV3(this System.Random random,float min,float max)
        {
            Vector3 v3 = Vector3.zero;
            v3.x = random.Range(min,max);
            v3.y = random.Range(min,max);
            v3.z = random.Range(min,max);
            return v3;
        }

        
        public static int RandomIndexByWeight(int length,Func<int, float> getWeight)
        {
            float maxValue = 0;
            for (int i = 0; i < length; i++)
            {
                maxValue += getWeight(i);
            }
            float val = UnityEngine.Random.Range(0, maxValue);
            float weight = 0;

            for (int i = 0; i < length; i++)
            {
                weight += getWeight(i);
                if (val < weight)
                {
                    return i;
                }
            }
            return default;
        }

        /// <summary>
        /// 根据权重随机
        /// </summary>
        /// <param name="props"></param>
        /// <returns></returns>
        public static int rangeIndex(params float[] props)
        {
            float max = 0;
            foreach (var item in props)
            {
                max += item;
            }
            var v = range(0,max);
            for (int i = 1; i < props.Length; i++)
            {
                if(v>=props[i-1] && v<props[i])
                {
                    return i;
                }
            }
            return 0;
        }

        public static int range(int min,int max)
        {
            return UnityEngine.Random.Range(min,max);
        }

        public static float range(float min,float max)
        {
            return UnityEngine.Random.Range(min,max);
        }

        public static Vector2 range2(float min,float max)
        {
            Vector2 v2 = Vector2.zero;
            v2.x = UnityEngine.Random.Range(min,max);
            v2.y = UnityEngine.Random.Range(min,max);
            return v2;
        }

        public static Vector2 range3(float min,float max)
        {
            Vector3 v3 = Vector3.zero;
            v3.x = UnityEngine.Random.Range(min,max);
            v3.y = UnityEngine.Random.Range(min,max);
            v3.z = UnityEngine.Random.Range(min,max);
            return v3;
        }

        public static Color rangec(float min=0,float max=1)
        {
            float r = UnityEngine.Random.Range(min,max);
            float g = UnityEngine.Random.Range(min,max);
            float b = UnityEngine.Random.Range(min,max);
            float a = UnityEngine.Random.Range(min,max);
            return new Color(r,g,b,a);
        }
    }
}
