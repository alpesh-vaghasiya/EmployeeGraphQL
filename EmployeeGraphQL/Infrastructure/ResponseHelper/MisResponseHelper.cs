using Newtonsoft.Json.Linq;

public static class MisResponseHelper
{
    public static T Normalize<T>(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return default;

        var token = JToken.Parse(content);

        // ✅ Case 1: { succeeded, data }
        if (token.Type == JTokenType.Object && token["data"] != null)
        {
            return token["data"].ToObject<T>();
        }

        // ✅ Case 2: { result: ... }
        if (token.Type == JTokenType.Object && token["result"] != null)
        {
            return token["result"].ToObject<T>();
        }

        // ✅ Case 3: direct array or object
        return token.ToObject<T>();
    }
}