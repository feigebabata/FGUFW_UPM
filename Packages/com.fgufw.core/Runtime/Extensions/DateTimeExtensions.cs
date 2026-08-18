using System;
using UnityEngine;

namespace FGUFW
{
    public static class DateTimeExtensions
    {
        public static string SecondTickName(this DateTime dateTime)
        {
            return DateTime.Now.ToString("yyyyMMddHHmmss");
        }

        public static DateTime ToDate(this string self)
        {
            if (self.IsNull()) return DateTime.Now;
            return DateTime.Parse(self);
        } 

        public static string Normal(this DateTime self)
        {
            return self.ToString("yyyy-MM-dd HH:mm:ss");
        }

    }
}