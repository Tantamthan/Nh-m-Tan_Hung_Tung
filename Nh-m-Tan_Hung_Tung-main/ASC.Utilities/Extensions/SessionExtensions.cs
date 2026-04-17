using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace ASC.Utilities.Extensions
{
    public static class SessionExtensions
    {
        public static void SetObjectAsJson(this ISession session, string key, object value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        public static T? GetObjectFromJson<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return string.IsNullOrWhiteSpace(value)
                ? default
                : JsonConvert.DeserializeObject<T>(value);
        }
    }
}
