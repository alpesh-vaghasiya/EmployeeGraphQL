using Newtonsoft.Json.Linq;

public static class MisResponseHelper
{
    public static T Normalize<T>(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return default;

        var token = JToken.Parse(content);

        // ✅ Case 1: Wrapped response
        if (token.Type == JTokenType.Object && token["data"] != null)
        {
            var dataToken = token["data"];

            // 🔥 If data is ARRAY
            if (dataToken.Type == JTokenType.Array)
            {
                // If T is collection → direct
                if (typeof(T).IsGenericType &&
                    typeof(T).GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    return dataToken.ToObject<T>();
                }

                // If T is single → take first
                var first = dataToken.FirstOrDefault();
                return first != null ? first.ToObject<T>() : default;
            }

            // 🔥 If data is OBJECT
            return dataToken.ToObject<T>();
        }

        // ✅ Case 2: Direct response
        return token.ToObject<T>();
    }
}