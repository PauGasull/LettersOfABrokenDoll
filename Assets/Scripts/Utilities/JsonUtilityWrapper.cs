using Newtonsoft.Json;

public static class JsonUtilityWrapper
{
    public static T FromJson<T>(string json)
    {
        return JsonConvert.DeserializeObject<T>(json);
    }
}
