using System;

namespace FGUFW
{
    public static class UnitNumExtensions
    {
        const string UNITS = "KMGTPEZYRQXWV"; //10^39
        const char UNIT_NONE = '\u200B';//默认不可见

        // 【性能优化】预计算倍率表，替代极其耗时的 Math.Pow 运算
        private static readonly double[] MULTIPLIERS = new double[]
        {
            1e3,  // K
            1e6,  // M
            1e9,  // G
            1e12, // T
            1e15, // P
            1e18, // E
            1e21, // Z
            1e24, // Y
            1e27, // R
            1e30, // Q
            1e33, // X
            1e36, // W
            1e39  // V
        };

        /// <summary>
        /// IS 国际标准千进制单位
        /// </summary>
        /// <param name="self"></param>
        /// <returns>余数和后缀</returns>
        public static (double,char) ToUnit(this double self)
        {
            double absVal = Math.Abs(self);
            
            // 小于1000的数值直接输出
            if (absVal < 1000d)
            {
                return (self,UNIT_NONE);
            }

            double displayVal = self;
            int unitIndex = -1;

            // 循环除以1000，直到数值小于1000，或者触及单位上限
            while (Math.Abs(displayVal) >= 1000d && unitIndex < UNITS.Length - 1)
            {
                displayVal /= 1000d;
                unitIndex++;
            }

            return (displayVal,UNITS[unitIndex]);
        }

        public static string ToUnitNum(this int self)
        {
            double num = self;
            return ToUnitNum(num);
        }

        public static string ToUnitNum(this float self)
        {
            double num = (double)self;
            return ToUnitNum(num);
        }

        public static string ToUnitNum(this double self)
        {
            var (n,u) = self.ToUnit();

            return $"{n:0.##}{u}";
        }

        public static double ToUnitNum(this string self)
        {
            if (string.IsNullOrEmpty(self)) return default;

            // 【性能优化】使用 ReadOnlySpan 替代 text.Substring()，避免产生额外的字符串 GC 分配
            ReadOnlySpan<char> span = self.AsSpan();
            char lastChar = char.ToUpper(span[span.Length - 1]);
            int unitIndex = UNITS.IndexOf(lastChar);

            if (unitIndex == -1)
            {
                return double.Parse(span); 
            }
            else
            {
                // 截取除最后一位单位字符以外的数字部分 (Slice 不产生 GC)
                ReadOnlySpan<char> numSpan = span.Slice(0, span.Length - 1);
                
                return double.Parse(numSpan) * MULTIPLIERS[unitIndex]; 
            }
        }

    }
}