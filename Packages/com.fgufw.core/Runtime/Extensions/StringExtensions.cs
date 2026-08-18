using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using UnityEngine;

namespace FGUFW
{
    public static class StringExtensions
    {
        public readonly static IFormatProvider NumberFormat = new CultureInfo("en-us").NumberFormat;

        public static Uri ToUri(this string text)
        {
            return new Uri(text);
        }
        
        public static IPAddress ToIP(this string text)
        {
            return IPAddress.Parse(text);
        }

        public static int ToInt32(this string text)
        {
            if(text.IsNull())return default;
            return int.Parse(text);
        }
        
        public static float ToFloat(this string text)
        {
            if(text.IsNull())return default;
            return float.Parse(text,NumberFormat);
        }
        
        public static float ToFloat(this ReadOnlySpan<char> text)
        {
            return float.Parse(text,NumberStyles.AllowThousands | NumberStyles.Float,NumberFormat);
        }

        // public static T FromJson<T>(this string text)
        // {
        //     return JsonUtility.FromJson<T>(text);
        // }

        public static Color ToColor(this string self)
        {
            Color color;
            ColorUtility.TryParseHtmlString(self,out color);
            return color;
        }

        public static T ToEnum<T>(this string self) where T:Enum
        {
            return (T)Enum.Parse(typeof(T),self);
        }
        
        public static bool IsNull(this string self)
        {
            return string.IsNullOrEmpty(self);
        }
        
        /// <summary>
        /// 折叠
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static string Fold(this string self,int maxLength)
        {
            if(self.Length>maxLength)
            {
                return self.Substring(0,maxLength) + "...";
            }
            return self;
        }

        /// <summary>
        /// 相似度
        /// </summary>
        public static float Similarity(this string self, string target)
        {
            // 完全相等
            if (self == target) return 1.0f;
            
            // 忽略大小写相等
            if (string.Equals(self, target, StringComparison.OrdinalIgnoreCase)) 
            {
                return Mathf.Lerp(0.8f,0.5f,Mathf.Abs(self.Length-target.Length)/(float)Mathf.Min(self.Length,target.Length));
            }

            // 动态计算包含比例
            float maxScore = 0;
            if (!string.IsNullOrEmpty(self) && !string.IsNullOrEmpty(target))
            {
                // 检查str1是否包含str2
                if (self.IndexOf(target, StringComparison.OrdinalIgnoreCase) >= 0)
                    maxScore = Mathf.Max(maxScore, (float)target.Length / Math.Max(self.Length, target.Length) * 0.5f);
                
                // 检查str2是否包含str1
                if (target.IndexOf(self, StringComparison.OrdinalIgnoreCase) >= 0)
                    maxScore = Mathf.Max(maxScore, (float)self.Length / Math.Max(self.Length, target.Length) * 0.5f);
            }
            
            return maxScore;

        }

        public static string ts(this object self)
        {
            return self.ToString();
        }

    }
}