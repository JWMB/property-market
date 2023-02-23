using Microsoft.Playwright;

namespace Parsers
{
    public static class IResponseExtensions
    {
        public static string GetValueOrDefault(Dictionary<string, string> dict, string key, string defaultValue, bool caseSensitive = false)
        {
            key = caseSensitive ? key : key.ToLower();
            var found = dict.FirstOrDefault(o => (caseSensitive ? o.Key : o.Key.ToLower()) == key);
            return found.Key?.ToLower() == key ? found.Value : defaultValue;
        }
        public static string? GetValueOrNull(Dictionary<string, string> dict, string key, bool caseSensitive = false)
        {
            key = key.ToLower();
            return dict.FirstOrDefault(o => o.Key.ToLower() == key).Value;
        }

        public static string GetHeaderFallback(this IResponse response, string header, string fallback = "", bool caseSensitive = false)
           => response.GetHeader(header, caseSensitive) ?? fallback;

        public static string GetHeaderFallback(this IRequest request, string header, string fallback = "", bool caseSensitive = false)
            => request.GetHeader(header, caseSensitive) ?? fallback;

        public static string? GetHeader(this IResponse response, string header, bool caseSensitive = false)
            => GetValueOrNull(response.Headers, header, caseSensitive);

        public static string? GetHeader(this IRequest request, string header, bool caseSensitive = false)
            => GetValueOrNull(request.Headers, header, caseSensitive);
    }
}
