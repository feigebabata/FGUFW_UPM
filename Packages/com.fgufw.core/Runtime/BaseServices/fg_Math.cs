using UnityEngine;

namespace FGUFW
{
    public static partial class fg
    {
        public static int ceil(float value)
        {
            return Mathf.CeilToInt(value);
        }

        public static float random(float min, float max)
        {
            return Random.Range(min, max);
        }

        public static int random(int min, int max)
        {
            return Random.Range(min, max);
        }

        public static float random0_1()
        {
            return random(0f, 1f);
        }

        public static Vector2 randomV2(float min, float max)
        {
            return new Vector2(random(min, max), random(min, max));
        }

        public static Vector2 randomCircle(float radius)
        {
            return Random.insideUnitCircle * radius;
        }
    }
}
