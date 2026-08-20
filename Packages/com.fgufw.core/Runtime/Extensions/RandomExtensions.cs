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

        public static Color RandomColor(float min=0f,float max=1f)
        {
            float r = UnityEngine.Random.Range(min,max);
            float g = UnityEngine.Random.Range(min,max);
            float b = UnityEngine.Random.Range(min,max);
            float a = UnityEngine.Random.Range(min,max);
            return new Color(r,g,b,a);
        }
    }
}
