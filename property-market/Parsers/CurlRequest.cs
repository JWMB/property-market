using System.Text.RegularExpressions;

namespace Parsers
{
    public class CurlRequest
    {
        private HttpRequestMessage httpRequestMessage;

        public CurlRequest(string curl, Uri? uri = null, HttpMethod? method = null)
        {
            httpRequestMessage = Parse(curl, uri, method);
        }

        public static HttpRequestMessage Parse(string curl, Uri? uri = null, HttpMethod? method = null)
        {
            // TODO: very fragile - find nuget for this
            var result = new HttpRequestMessage();

            if (uri == null)
            {
                var m = new Regex(@"\s(\\""|')([^\s\""\']+)(\\""|')").Match(curl);
                if (m.Success && m.Groups.Count >= 3)
                    uri = new Uri(m.Groups[2].Value);
            }
            result.RequestUri = uri;

            if (method == null)
            {
                var m = new Regex(@"-X\s+(\w+)").Match(curl);
                if (m.Success)
                {
                    var methodStr = m.Groups[1].Value.ToUpper().Trim();
                    method = methodStr switch
                    {
                        "POST" => HttpMethod.Post,
                        "PUT" => HttpMethod.Put,
                        "DELETE" => HttpMethod.Delete,
                        "PATCH" => HttpMethod.Patch,
                        _ => HttpMethod.Get
                    };
                }
            }
            result.Method = method ?? HttpMethod.Get;

            var rxHeaders = new Regex(@"-H\s+'([^']*)'");
            var matches = rxHeaders.Matches(curl);

            var headers = matches.OfType<Match>()
                .Select(o => o.Groups[1].Value)
                .Select(o => { var index = o.IndexOf(':'); return new { Key = o.Remove(index), Value = o.Substring(index + 1) }; })
                .ToDictionary(o => o.Key, o => o.Value);

            var mBody = new Regex(@"--data-raw\s+'([^']+)'").Match(curl);
            var body = mBody.Success ? mBody.Groups[1].Value : null;

            return result.SetRequestProperties(headers, body);
        }

        public HttpRequestMessage HttpRequestMessage => httpRequestMessage;
        public Uri Uri => httpRequestMessage.RequestUri!;
    }
}
