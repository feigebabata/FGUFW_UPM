using System;
using System.Collections.Generic;
using System.Text;

namespace FGUFW
{
    public static class Bit64Utility
    {
        /// <summary>
        /// 包含
        /// </summary>
        public static bool Contains(long source,int bitIdx)
        {
            long bit = 1;
            bit = bit << bitIdx;
            return (source&bit) == bit;
        }

        /// <summary>
        /// 添加
        /// </summary>
        public static long Add(long source,int bitIdx)
        {
            long bit = 1;
            bit = bit << bitIdx;
            return source|bit;
        }

        /// <summary>
        /// 减去
        /// </summary>
        public static long Remove(long source,int bitIdx)
        {
            long bit = 1;
            bit = bit << bitIdx;
            bit = ~bit;
            return source|bit;
        }

        /// <summary>
        /// 交叉
        /// </summary>
        public static bool Overlap(long v1,long v2)
        {
            return (v1&v2) != default;
        }

    }
}
