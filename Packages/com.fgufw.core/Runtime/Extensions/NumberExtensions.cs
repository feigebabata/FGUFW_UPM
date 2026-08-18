using System;

namespace FGUFW
{
    public static class NumberExtensions
    {

        public static int Ads(this int self)
        {
            if(self<0)
            {
                return self * -1;
            }
            return self;
        }

        public static float Ads(this float self)
        {
            if(self<0)
            {
                return self * -1;
            }
            return self;
        }

    }
}
