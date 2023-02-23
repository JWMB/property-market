using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Newtonsoft.Json;
using System.Data;
using System.Text.RegularExpressions;

namespace Parsers
{
    public static class UriExtensions
    {
        public static Uri ReplaceQuery(this Uri uri, string? query)
        {
            var ub = new UriBuilder(uri);
            ub.Query = query ?? "";
            return ub.Uri;
        }

        public static Uri ReplacePathAndQuery(this Uri uri, string pathAndQuery)
        {
            var ub = new UriBuilder(uri);
            var split = pathAndQuery.Split("?");
            ub.Path = split[0];
            ub.Query = split.Length > 1 ? split[1] : "";
            return ub.Uri;
        }
        public static Uri ReplacePath(this Uri uri, string replacement)
        {
            var ub = new UriBuilder(uri);
            ub.Path = replacement;
            return ub.Uri;
        }

        public static Uri ReplacePath(this Uri uri, Func<string, string> replace)
        {
            var ub = new UriBuilder(uri);
            ub.Path = replace(ub.Path);
            return ub.Uri;
        }
    }

    public static class IHttpClientFactoryExtensions
    {
        public static async Task<HttpResponseMessage> CurlCall(this IHttpClientFactory clientFactory, string curl, Action<HttpRequestMessage>? modifyRequest = null)
        {
            var msg = new CurlRequest(curl).HttpRequestMessage;
            //var msg = HttpRequestMessageExtensions.FromCurl(curl);
            modifyRequest?.Invoke(msg);
            using var client = clientFactory.CreateClient("Default");
            return await client.SendAsync(msg);
        }

        //static void X()
        //{
        //    if (response.Content.Headers.ContentEncoding.Contains("gzip"))
        //    {
        //        var content = await response.Content.ReadAsStringAsync();
        //        var charset = response.Content.Headers.ContentType?.CharSet;
        //        if (string.IsNullOrEmpty(charset) || charset.ToLower() == "utf-8")
        //        {
        //            var encodings = new[] { Encoding.UTF8, Encoding.Unicode, Encoding.UTF32, Encoding.ASCII };
        //            foreach (var e in encodings)
        //            {
        //                try
        //                {
        //                    byte[] decompressed = Decompress2(e.GetBytes(content)); //Decompress
        //                    html = Encoding.UTF8.GetString(decompressed);
        //                    break;
        //                }
        //                catch (Exception ex)
        //                {
        //                    try
        //                    {
        //                        html = DecompressGzip(e.GetBytes(content));
        //                        if (html.Length > 0)
        //                            break;
        //                    }
        //                    catch { }
        //                }
        //            }
        //        }
        //        else
        //            throw new NotImplementedException();
        //    }
        //    else
        //        throw new NotImplementedException();
        //}

        //public static byte[] Decompress(byte[] bytes)
        //{
        //    using (var memoryStream = new MemoryStream(bytes))
        //    {
        //        using (var outputStream = new MemoryStream())
        //        {
        //            using (var decompressStream = new GZipStream(memoryStream, CompressionMode.Decompress, false))
        //            {
        //                decompressStream.CopyTo(outputStream);
        //            }
        //            return outputStream.ToArray();
        //        }
        //    }
        //}
        //public static byte[] Decompress2(byte[] bytes)
        //{
        //    using (var source = new MemoryStream(bytes))
        //    {
        //        byte[] lengthBytes = new byte[4];
        //        source.Read(lengthBytes, 0, 4);

        //        var length = BitConverter.ToInt32(lengthBytes, 0);
        //        using (var decompressionStream = new GZipStream(source,
        //            CompressionMode.Decompress))
        //        {
        //            var result = new byte[length];
        //            decompressionStream.Read(result, 0, length);
        //            return result;
        //        }
        //    }
        //}

        //public static string DecompressGzip(byte[] data)
        //{
        //    // Read the last 4 bytes to get the length
        //    byte[] lengthBuffer = new byte[4];
        //    Array.Copy(data, data.Length - 4, lengthBuffer, 0, 4);
        //    int uncompressedSize = BitConverter.ToInt32(lengthBuffer, 0);

        //    var buffer = new byte[uncompressedSize];
        //    using (var ms = new MemoryStream(data))
        //    {
        //        using (var gzip = new GZipStream(ms, CompressionMode.Decompress))
        //        {
        //            gzip.Read(buffer, 0, uncompressedSize);
        //        }
        //    }

        //    return Encoding.UTF8.GetString(buffer);
        //}
    }

    public static class HttpRequestMessageExtensions
    {
        //public static HttpRequestMessage FromCurl(string curl, Uri? uri = null, HttpMethod? method = null)
        //{
        //    // TODO: very fragile - find nuget for this
        //    var result = new HttpRequestMessage();

        //    if (uri == null)
        //    {
        //        var m = new Regex(@"\s(\\""|')([^\s\""\']+)(\\""|')").Match(curl);
        //        if (m.Success && m.Groups.Count >= 3)
        //            uri = new Uri(m.Groups[2].Value);
        //    }
        //    result.RequestUri = uri;

        //    if (method == null)
        //    {
        //        var m = new Regex(@"-X\s+(\w+)").Match(curl);
        //        if (m.Success)
        //        {
        //            var methodStr = m.Groups[1].Value.ToUpper().Trim();
        //            method = methodStr switch
        //            {
        //                "POST" => HttpMethod.Post,
        //                "PUT" => HttpMethod.Put,
        //                "DELETE" => HttpMethod.Delete,
        //                "PATCH" => HttpMethod.Patch,
        //                _ => HttpMethod.Get
        //            };
        //        }
        //    }
        //    result.Method = method ?? HttpMethod.Get;

        //    var rxHeaders = new Regex(@"-H\s+'([^']*)'");
        //    var matches = rxHeaders.Matches(curl);

        //    var headers = matches.OfType<Match>()
        //        .Select(o => o.Groups[1].Value)
        //        .Select(o => { var index = o.IndexOf(':'); return new { Key = o.Remove(index), Value = o.Substring(index + 1) }; })
        //        .ToDictionary(o => o.Key, o => o.Value);

        //    var mBody = new Regex(@"--data-raw\s+'([^']+)'").Match(curl);
        //    var body = mBody.Success ? mBody.Groups[1].Value : null;

        //    return result.SetRequestProperties(headers, body);
        //}

        public static HttpRequestMessage SetRequestProperties(this HttpRequestMessage request, Dictionary<string, string>? headers = null, object? body = null)
        {
            if (body != null)
            {
                request.Content = new StringContent(body is string str ? str : JsonConvert.SerializeObject(body));
            }

            if (headers != null)
            {
                var contentHeaders = new[] { "Content-Type" }.Select(o => o.ToLower());

                foreach (var item in headers.Where(o => contentHeaders.Contains(o.Key.ToLower()) == false))
                    request.Headers.Add(item.Key, item.Value);

                if (request.Content != null)
                {
                    foreach (var item in headers.Where(o => contentHeaders.Contains(o.Key.ToLower()) == true))
                    {
                        if (request.Content.Headers.Contains(item.Key))
                            request.Content.Headers.Remove(item.Key);
                        request.Content.Headers.Add(item.Key, item.Value);
                    }
                }
            }

            return request;
        }
    }

    public static class ExTools
    {
        public static T TryOrDefault<T>(Func<T> func, T defaultValue, Action<Exception>? catcher = null)
        {
            try
            {
                return func();
            }
            catch (Exception ex)
            {
                catcher?.Invoke(ex);
                return defaultValue;
            }
        }

        public static async Task<T> TryOrDefault<T>(Func<Task<T>> func, T defaultValue, Action<Exception>? catcher = null)
        {
            try
            {
                return await func();
            }
            catch (Exception ex)
            {
                catcher?.Invoke(ex);
                return defaultValue;
            }
        }
    }

    public static class IElementExtensions
    {
        public static List<Dictionary<string, string?>> ToDictionaries(this IHtmlTableElement table)
        {
            var dt = table.ToDataTable();
            var result = new List<Dictionary<string, string?>>();
            foreach (DataRow row in dt.Rows)
            {
                var dict = new Dictionary<string, string?>();
                result.Add(dict);
                foreach (DataColumn column in dt.Columns)
                    dict.Add(column.ColumnName, row[column]?.ToString());
            }
            return result;
        }

        public static DataTable ToDataTable(this IHtmlTableElement table)
        {
            var dt = new DataTable();
            var thead = table.QuerySelector("thead");
            if (thead != null)
            {
                dt.Columns.AddRange(thead.QuerySelectorAll("th").Select(o => o.Text()).Select(o => new DataColumn(o)).ToArray());
            }

            var trs = (table.QuerySelector("tbody") ?? table).QuerySelectorAll("tr").ToList();

            if (dt.Columns.Count == 0)
            {
                var firstRowCells = trs.First().QuerySelectorAll("td");
                dt.Columns.AddRange(firstRowCells.Select(o => o.Text()).Select(o => new DataColumn(o)).ToArray());
                trs = trs.Skip(1).ToList();
            }

            foreach (var row in trs)
            {
                var dr = dt.NewRow();
                dt.Rows.Add(dr);
                dr.ItemArray = row.QuerySelectorAll("td").Select(o => o.Text()).ToArray();
            }
            return dt;
        }

        public static IHtmlCollection<IElement> QuerySelectorAllOrThrow(this IElement element, string selectors)
        {
            var result = element.QuerySelectorAll(selectors);
            if (result == null || result.Any() == false)
                throw new Exception($"No elements found: {selectors}");
            return result;
        }

        public static T QuerySelectorOrThrow<T>(this IElement element, string selectors) where T : IElement
        {
            var result = element.QuerySelectorAllOrThrow(selectors).OfType<T>();
            if (result.Any() == false)
                throw new Exception($"No elements of type {typeof(T).Name} found for '{selectors}'");
            return result.First();
        }

        public static IElement QuerySelectorOrThrow(this IElement element, string selectors)
        {
            var result = element.QuerySelector(selectors);
            if (result == null)
                throw new Exception($"No elements found: {selectors}");
            return result;
        }

        public static string GetAttributeOrThrow(this IElement element, string name)
        {
            var attr = element.GetAttribute(name);
            if (attr == null)
                throw new Exception($"No attribute found: {name}");
            return attr;
        }
    }
}
