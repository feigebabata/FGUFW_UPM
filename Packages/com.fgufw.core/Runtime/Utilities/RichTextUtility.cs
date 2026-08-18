
using System.Text;
using UnityEngine;

namespace FGUFW
{
    public static class RichTextUtility
    {
        public static string B(string text)
        {
            return $"<b>{text}</b>";
        }

        public static string I(string text)
        {
            return $"<i>{text}</i>";
        }

        public static string Size(string text, int size)
        {
            return $"<size={size}>{text}</size>";
        }

        public static string Color(string text, Color color)
        {
            return $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{text}</color>";
        }

        public static string RichText(this string self,Color color,bool b=false,bool i=false,int size=default)
        {
            if (color != default)
            {
                self = Color(self, color);
            }
            if (b)
            {
                self = B(self);
            }
            if (i)
            {
                self = I(self);
            }
            if (size != default)
            {
                self = Size(self,size);
            }
            return self;
        }

    }
}
