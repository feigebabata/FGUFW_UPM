namespace FGUFW
{
    public interface IJsonService
    {
        string ToJson(object obj);
        T ToObject<T>(string jsonText);
    }
}