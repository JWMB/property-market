using Brotli;
using System.Text;

namespace Parsers
{
    public interface IDataFetcher
    {
        Task<string> Fetch(HttpRequestMessage request);

        public async Task<string> Fetch(CurlRequest curl)
        {
            return await Fetch(curl.HttpRequestMessage);
        }

        public async Task<string> Fetch(Uri uri)
        {
            return await Fetch(new HttpRequestMessage { RequestUri = uri, Method = HttpMethod.Get });
        }
    }

    public class HttpClientDataFetcher : IDataFetcher
    {
        public const string HttpClientName = "Default";
        private readonly IHttpClientFactory httpClientFactory;

        public HttpClientDataFetcher(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }

        public async Task<string> Fetch(HttpRequestMessage request)
        {
            using var client = httpClientFactory.CreateClient(HttpClientName);
            var response = await client.SendAsync(request);
            return await HandleResponse(request, response);
        }

        private async Task<string> HandleResponse(HttpRequestMessage request, HttpResponseMessage? response)
        {
            if (response == null)
                throw new Exception($"No response from {request.RequestUri}");
            if (!response.IsSuccessStatusCode)
                throw new Exception($"Could not fetch {request.RequestUri}");

            var encoding = response.Content.Headers.ContentEncoding;
            if (encoding.Contains("br"))
            {
                var decompressed = response.Content.ReadAsStream().DecompressFromBrotli();
                return Encoding.UTF8.GetString(decompressed);
            }
            return await response.Content.ReadAsStringAsync();
        }

    }
}
