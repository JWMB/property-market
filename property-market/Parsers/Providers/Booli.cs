using AngleSharp.Dom;
using Newtonsoft.Json.Linq;
using Parsers.Models;
using static Parsers.PlaywrightWebPageLoader;

namespace Parsers.Providers
{
    public class Booli : IPropertyDataProvider, IPropertyListingSearchProvider, IPropertyListingProvider
    {
        public Uri DefaultUri => new Uri("https://www.booli.se");

        public string Id => nameof(Booli);

        public bool IsAggregator => true;

        public IPropertyListingSearchProvider? SearchProvider => this;

        public IPropertyListingProvider? ListingProvider => this;

        public IPropertyDataProvider DataProvider => this;

        public async Task<IPropertyListingSearchProvider.FetchResult> FetchPropertySearchResults(PropertyFilter? filter = null, int skip = 0, int take = 100)
        {
            var loader = new PlaywrightWebPageLoader();

            string? loadedHtml = null;
            var filters = new[] {
                new Filter
                {
                    IsMatch = (r, contentType) =>
                        r.Url.ToLower().StartsWith("https://www.booli.se/")
                        && contentType.StartsWith("text/html"),
                    HandleText = txt => loadedHtml = txt
                },
                new Filter
                {
                    IsMatch = (r, contentType) =>
                        r.Url.ToLower() == "https://www.booli.se/graphql"
                        && r.Request.GetHeaderFallback("Content-Type").ToLower() == "application/json"
                        && JObject.Parse(r.Request.PostData ?? "{}").GetValue("operationName")?.ToString() == "searchForSale",
                    HandleText = txt => { Console.WriteLine($"asd {txt}"); }
                }
            };
            var uri = new Uri("https://www.booli.se/sverige/77104");
            await loader.Load(uri, filters);

            if (loadedHtml == null)
                throw new Exception($"Could not fetch html from {uri}");

            //var items = ParseSearchResults(uri, loadedHtml);

            return new IPropertyListingSearchProvider.FetchResult { Source = uri, Content = loadedHtml };
        }

        public async Task<IPropertyListingProvider.FetchResult> FetchPropertyListingResult(string objectId)
        {
            var loader = new PlaywrightWebPageLoader();

            var uri = DefaultUri.ReplacePathAndQuery(objectId);
            var response = await loader.Load(uri);
            if (response == null)
                throw new Exception("");

            var html = await response.TextAsync();
            var parsed = ParseItemPage(uri, html);

            return new IPropertyListingProvider.FetchResult { Listing = parsed, RawResult = html };
        }

        public PropertyListing ParseItemPage(Uri source, string html)
        {
            var (doc, head, body) = ParseTools.ParseHtmlWithHeadAndBody(html, source);
            var scripts = body.QuerySelectorAllOrThrow("script");

            var dataScript = scripts.Where(o => o.InnerHtml.Contains("objectId") && o.InnerHtml.Contains("objectType") && o.InnerHtml.Contains("latitude")).Single();

            var json = dataScript.InnerHtml.Substring(dataScript.InnerHtml.IndexOf("{"));

            var jObj = JObject.Parse(json);
            var rq = jObj["ROOT_QUERY"] as JObject;
            if (rq == null)
                throw new Exception($"ROOT_QUERY not found");

            var details = (JObject?)rq.Properties().Single(o => o.Name.StartsWith("propertyByResidenceId")).Value;
            if (details == null)
                throw new Exception($"propertyByResidenceId not found");

            //GetDetail("housingCoop");
            //GetDetail("primaryArea");

            var result = new PropertyListing
            {
                ListingId = source.PathAndQuery,
                CrawledFrom = source,
                Provider = this,
                UriWithProvider = source,
                RealtorListingPage = new Uri(GetDetailOrThrow("listingUrl")),
                //TODO: ListingAdded = DateTimeOffset.Parse(GetDetail("published")),

                Property = new Property
                {
                    Type = GetDetail("objectType") switch
                    {
                        "Lägenhet" => PropertyType.Apartment,
                        _ => PropertyType.Unknown // TODO:
                    },
                    NumRooms = ParseTools.ParseSpecialFraction(GetDetail("rooms")) ?? 0,

                    Address = new Address { StreetAddress = GetDetail("streetAddress") ?? "" },
                    LivingArea = ParseTools.ParseSpecialFraction(GetDetail("livingArea")) ?? 0,

                    MonthlyPayment = (int?)ParseTools.ParseNumber(GetDetail("monthlyPayment")),
                },
            };

            return result;

            string GetDetailOrThrow(string key)
            {
                var result = GetDetail(key);
                if (result == null)
                    throw new Exception($"Couldn't find '{key}'");
                return result;
            }

            string? GetDetail(string key)
            {
                var prop = details!.GetValue(key);
                if (prop == null)
                    return null;

                if (prop is JObject propObj)
                {
                    if (propObj.Value<string>("__typename") == "FormattedValue")
                        return propObj.Value<string>("formatted") ?? "";

                    if (propObj.Value<string>("__typename") == "HousingCoop")
                        return propObj.Value<string>("link") ?? "";

                    var val = propObj.Value<string>("name");
                    if (val != null)
                        return val;

                    val = propObj.Value<string>("value");
                    if (val != null)
                        return val;
                }
                return prop.ToString();
            }
            /*
      "latitude": 59.3404099,
      "longitude": 18.0374753,
      "constructionYear": 1928,
      "energyClass": { "__typename": "EnergyClass", "score": "E" },
      "propertyType": "active",
      "objectType": "Lägenhet",
      "addressId": "25824352",
      "streetAddress": "Sankt Eriksgatan 84",

      "housingCoop": {
        "__typename": "HousingCoop",
        "booliId": 46122,
        "name": "BRF Mindre 8 och 9",
        "link": "/bostadsrattsforening/46122"
      },
      "monthlyPayment": { "__typename": "FormattedValue", "formatted": "14 218 kr/mån" },

      "rooms": { "__typename": "FormattedValue", "formatted": "2 rum", "raw": 2 },

      "livingArea": { "__typename": "FormattedValue",
        "formatted": "64 m²",
        "value": "64",
        "unit": "m²",
        "raw": 64
      },
      "rent": { "__typename": "FormattedValue", "formatted": "1 909 kr/mån", "raw": 1909 },

      "primaryArea": { "__typename": "Area_V3", "name": "Vasastan" },

      "floor": { "__typename": "FormattedValue", "formatted": "1 tr" },
      "listPrice": {
        "__typename": "FormattedValue",
        "raw": 5995000,
        "unit": "kr",
        "formatted": "5 995 000 kr",
        "value": "5 995 000"
      },
      "listSqmPrice": {
        "__typename": "FormattedValue",
        "formatted": "93 700 kr/m²"
      },
      "listingUrl": "https://www.fastighetsbyran.com/sv/sverige/objekt/?objektid=2662525",
      "pageviews": 35,
      "published": "2023-02-16 23:59:13",
             */
        }

        public List<PropertyListing> ParseSearchResults(Uri source, string html)
        {
            var (doc, head, body) = ParseTools.ParseHtmlWithHeadAndBody(html, source);

            var elements = body.QuerySelectorAll("a[href^='/bostad']")
                .Concat(body.QuerySelectorAll("a[href^='/annons']"));

            if (!elements.Any())
                throw new Exception($"No objects found: {source}");

            return elements.Select(FromElement).ToList();

            PropertyListing FromElement(IElement el)
            {
                var areaElement = el.Descendents().Single(o => o.Text().Contains("m²"));
                if (areaElement?.Parent == null)
                    throw new Exception($"Area missing in {el.GetAttribute("href")}");

                return new PropertyListing
                {
                    ListingId = new Uri(el.GetAttributeOrThrow("href")).PathAndQuery,
                    CrawledFrom = source,
                    Provider = this,
                    UriWithProvider = new Uri(el.GetAttributeOrThrow("href")),

                    Property = new Property
                    {
                        Address = new Address { StreetAddress = el.QuerySelector("h3")?.InnerHtml },
                        LivingArea = ParseTools.ParseSpecialFractionOrThrow(el.QuerySelectorAll("p").FirstOrDefault(o => o.InnerHtml.Trim().EndsWith("m²"))?.InnerHtml ?? "", "m²")
                    }
                };
            }
        }
    }
}
