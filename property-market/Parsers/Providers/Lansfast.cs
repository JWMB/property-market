using AngleSharp.Dom;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Parsers.Models;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Parsers.Providers
{
    public class Lansfast : IPropertyDataProvider, IPropertyListingSearchProvider, IPropertyListingProvider
    {
        private readonly IDataFetcher dataFetcher;

        // https://www.lansfast.se/till-salu/
        public Uri DefaultUri => new Uri("https://www.lansfast.se");

        public IPropertyDataProvider DataProvider => this;

        public string Id => nameof(Lansfast);

        public bool IsAggregator => false;

        public IPropertyListingSearchProvider? SearchProvider => this;

        public IPropertyListingProvider? ListingProvider => this;

        public Lansfast(IDataFetcher dataFetcher)
        {
            this.dataFetcher = dataFetcher;
        }

        public async Task<IPropertyListingProvider.FetchResult> FetchPropertyListingResult(string objectId)
        {
            var request = new CurlRequest("""curl 'https://www.lansfast.se/till-salu/villa/blekinge/karlshamn/svangsta/nissavagen-11/cmvilla5aba5m3qk4vosdsb/' -H 'User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/110.0' -H 'Accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8' -H 'Accept-Language: en-US,en;q=0.5' -H 'Accept-Encoding: gzip, deflate, br' -H 'DNT: 1' -H 'Connection: keep-alive' -H 'Cookie: OptanonConsent=isGpcEnabled=0&datestamp=Tue+Feb+21+2023+16%3A03%3A41+GMT%2B0100+(Central+European+Standard+Time)&version=6.18.0&isIABGlobal=false&hosts=&landingPath=NotLandingPage&groups=C0001%3A1%2CC0002%3A1%2CC0003%3A1%2CC0004%3A1&geolocation=SE%3BAB&AwaitingReconsent=false; OptanonAlertBoxClosed=2022-12-15T17:34:43.854Z; _gcl_au=1.1.232909006.1671125684; _ga_G36QNJLL2H=GS1.1.1676991215.4.1.1676991821.0.0.0; _ga=GA1.1.2009243595.1671125684; _gcl_aw=GCL.1676728878.Cj0KCQiAi8KfBhCuARIsADp-A54DGM5vOsujRZfHXqemKTgaFar7Plyyep0cEsBCMof0BaHt4AKwqU8aAsMWEALw_wcB' -H 'Upgrade-Insecure-Requests: 1' -H 'Sec-Fetch-Dest: document' -H 'Sec-Fetch-Mode: navigate' -H 'Sec-Fetch-Site: cross-site' -H 'Pragma: no-cache' -H 'Cache-Control: no-cache' -H 'TE: trailers'""").HttpRequestMessage;
            request.RequestUri = request.RequestUri!.ReplacePathAndQuery(objectId);
            var html = await dataFetcher.Fetch(request);

            return new IPropertyListingProvider.FetchResult
            {
                Listing = ParsePropertyListing(request.RequestUri!, html),
                RawResult = html
            };
        }

        public async Task<IPropertyListingSearchProvider.FetchResult> FetchPropertySearchResults(PropertyFilter? filter = null, int skip = 0, int take = 100)
        {
            var curl = new CurlRequest("""curl 'https://www.lansfast.se/umbraco/api/findestateapi/loadestates?page=1&sortOrder=0' -H 'User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/110.0' -H 'Accept: */*' -H 'Accept-Language: en-US,en;q=0.5' -H 'Accept-Encoding: gzip, deflate, br' -H 'Referer: https://www.lansfast.se/till-salu/' -H 'newrelic: eyJ2IjpbMCwxXSwiZCI6eyJ0eSI6IkJyb3dzZXIiLCJhYyI6IjMyNjA5ODciLCJhcCI6IjUyNjU2MjM2OCIsImlkIjoiOTE4ZmFhMWRmNTEzNWQ0YSIsInRyIjoiNjNlYTZiYWNkYTYyYTM4NWQ1ODRkODQ1YmE4NDM3MTUiLCJ0aSI6MTY3Njk5MTQ0MjgwOH19' -H 'traceparent: 00-63ea6bacda62a385d584d845ba843715-918faa1df5135d4a-01' -H 'tracestate: 3260987@nr=0-1-3260987-526562368-918faa1df5135d4a----1676991442808' -H 'DNT: 1' -H 'Connection: keep-alive' -H 'Cookie: OptanonConsent=isGpcEnabled=0&datestamp=Tue+Feb+21+2023+15%3A53%3A35+GMT%2B0100+(Central+European+Standard+Time)&version=6.18.0&isIABGlobal=false&hosts=&landingPath=NotLandingPage&groups=C0001%3A1%2CC0002%3A1%2CC0003%3A1%2CC0004%3A1&geolocation=SE%3BAB&AwaitingReconsent=false; OptanonAlertBoxClosed=2022-12-15T17:34:43.854Z; _gcl_au=1.1.232909006.1671125684; _ga_G36QNJLL2H=GS1.1.1676991215.4.0.1676991442.0.0.0; _ga=GA1.1.2009243595.1671125684; _gcl_aw=GCL.1676728878.Cj0KCQiAi8KfBhCuARIsADp-A54DGM5vOsujRZfHXqemKTgaFar7Plyyep0cEsBCMof0BaHt4AKwqU8aAsMWEALw_wcB' -H 'Sec-Fetch-Dest: empty' -H 'Sec-Fetch-Mode: cors' -H 'Sec-Fetch-Site: same-origin' -H 'Pragma: no-cache' -H 'Cache-Control: no-cache' -H 'TE: trailers'""");
            var json = await dataFetcher.Fetch(curl);

            return new IPropertyListingSearchProvider.FetchResult
            {
                Content = json,
                Source = curl.Uri //ParseSearchResults(curl.Uri, json)
            };
        }

        public List<PropertyListing> ParseSearchResults(Uri source, string json)
        {
            var items = JObject.Parse(json)["estates"] as JArray;
            if (items == null)
                throw new Exception($"no JArray estates");
            var result = items.Select(item =>
            {
                return new PropertyListing
                {
                    CrawledFrom = source,
                    ListingId = GetStringOrThrow("url"),
                    Provider = this,
                    RealtorListingPage = DefaultUri.ReplacePathAndQuery(GetStringOrThrow("url")),

                    Property = new Property
                    {
                        LivingArea = GetNumber("livingSpace"),
                        AdditionalArea = GetNumber("otherSpace"),
                        YardArea = GetNumber("plotSize"),
                        NumRooms = GetNumber("numberOfRooms"),

                        Type = GetString("estateType") switch
                        {
                            "Villa" => PropertyType.House,
                            _ => PropertyType.Apartment, // TODO:
                        },
                        MonthlyPayment = (int?)GetNumber("monthlyCost"),
                        Address = new Address
                        {
                            StreetAddress = item["streetAddress"]?.Value<string?>(),
                            City = item["city"]?.Value<string?>()
                        }
                    }
                };
                decimal? GetNumber(string key) => ParseTools.ParseNumber(GetString(key));
                string GetStringOrThrow(string key)
                {
                    var val = GetString(key);
                    if (val == null) throw new Exception($"No value found for '{key}'");
                    return val;
                }
                string? GetString(string key) => item[key]?.Value<string?>();
            }).ToList();
            return result;
        }

        public PropertyListing ParsePropertyListing(Uri source, string html)
        {
            var (doc, head, body) = ParseTools.ParseHtmlWithHeadAndBody(html, source);

            var dictionaries = body.QuerySelectorAll(".main-residential").Select(ToDict).ToList();
            var dictEco = ToDict(body.QuerySelectorOrThrow(".economy-description-content"));
            dictionaries.Add(dictEco);
            var dictMain = dictionaries.Aggregate((p, c) => Merge(p, c));

            var areaString = GetValue(dictMain, "Boarea/Biarea") ?? GetValue(dictMain, "Boarea") ?? "";
            var areas = Regex.Matches(areaString, @"\d+").OfType<Match>().Select(o => ParseTools.ParseNumber(o.Value)).ToList();
            var result = new PropertyListing
            {
                CrawledFrom = source,
                ListingId = "",
                Provider = this,
                UriWithProvider = source,
                RealtorListingPage = source,

                Price = (int?)GetNumber(dictMain, "Pris"),
                Untyped = dictMain,

                Property = new Property
                {
                    Type = GetValue(dictMain, "Boendeform") switch
                    {
                        "Friliggande villa" => PropertyType.House,
                        _ => PropertyType.Apartment // TODO
                    },
                    NumRooms = GetNumber(dictMain, "Antal rum"),
                    YardArea = GetNumber(dictMain, "Tomtarea"),
                    LivingArea = areas.FirstOrDefault(),
                    AdditionalArea = areas.Count > 1 ? areas[1] : null,
                    // Upplåtelseform Friköpt

                    //Driftkostnad
                    //Pantbrev
                    //Taxeringsår
                    //Taxeringsvärde
                    //Taxeringsvärde för byggnad
                    //Taxeringsvärde för mark

                    Address = new Address
                    {
                        StreetAddress = GetValue(dictMain, "Adress"),
                        City = GetValue(dictMain, "Kommun"),
                    }
                }
            };
            return result;

            decimal? GetNumber(Dictionary<string, string> dict, string key) => ParseTools.ParseNumber(GetValue(dict, key));
            string? GetValue(Dictionary<string, string> dict, string key) => dict.TryGetValue(key, out var val) ? val : null;

            Dictionary<string, string> ToDict(IElement parent)
            {
                var list = parent.QuerySelectorAllOrThrow(".inline-block")
                    .Select(o => new { Key = o.QuerySelector("h5"), Value = o.QuerySelector("p") })
                    .Where(o => o.Key != null && o.Value != null)
                    .Select(o => new KeyValuePair<string, string>(o.Key!.Text().Trim(), o.Value!.Text().Trim()));
                return Merge(new Dictionary<string, string>(), list);
            }

            Dictionary<string, string> Merge(Dictionary<string, string> dict, IEnumerable<KeyValuePair<string, string>> upserts)
            {
                foreach (var item in upserts)
                {
                    if (dict.TryGetValue(item.Key, out var existing) && item.Value.Length < existing.Length)
                        continue;
                    dict[item.Key] = item.Value;
                }
                return dict;
            }
        }
    }
}