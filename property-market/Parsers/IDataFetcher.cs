using Brotli;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Parsers
{
    public interface IDataFetcher
    {
        Task<string> Fetch(HttpRequestMessage request);

        public async Task<string> Fetch(CurlRequest curl)
        {
            //var request = HttpRequestMessageExtensions.FromCurl(curl);
            return await Fetch(curl.HttpRequestMessage);
        }

        //public async Task<string> Fetch(Uri uri) =>
        //    await Fetch(new HttpRequestMessage { RequestUri = uri, Method = HttpMethod.Get });

        public async Task<string> Fetch(Uri uri)
        {
            return await Fetch(new HttpRequestMessage { RequestUri = uri, Method = HttpMethod.Get });
        }
    }

    public class HttpClientDataFetcher : IDataFetcher
    {
        private readonly IHttpClientFactory httpClientFactory;

        public HttpClientDataFetcher(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }

        public async Task<string> Fetch(HttpRequestMessage request)
        {
            using var client = httpClientFactory.CreateClient();
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
