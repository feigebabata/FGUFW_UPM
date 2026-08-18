using System.Text;

namespace LitJson
{
    public static class LitJsonExtensions
    {
        public static T ToObject<T>(this string self)
        {
            return JsonMapper.ToObject<T>(self);
        }
        
        public static string ToJson(this object self)
        {
            var sb = new StringBuilder();
            var writer = new JsonWriter(sb);
            JsonMapper.ToJson(self,writer);

            return sb.ToString();
        }
    }
}