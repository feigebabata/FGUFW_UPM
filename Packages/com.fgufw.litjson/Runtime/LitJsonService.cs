using FGUFW;

namespace LitJson
{
    public class LitJsonService : IJsonService
    {
        public string ToJson(object obj)
        {
            return LitJsonExtensions.ToJson(obj);
        }

        public T ToObject<T>(string jsonText)
        {
            return LitJsonExtensions.ToObject<T>(jsonText);
        }
    }
}