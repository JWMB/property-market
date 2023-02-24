using AngleSharp.Common;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Brotli;
using Newtonsoft.Json.Linq;
using Parsers.Models;
using System.Text;

namespace Parsers.Providers
{
    public class Hemnet : IPropertyDataProvider, IPropertyListingSearchProvider, IPropertyListingProvider
    {
        private readonly IDataFetcher dataFetcher;

        public Uri DefaultUri => new Uri("https://www.hemnet.se");

        public string Id => nameof(Hemnet);

        public bool IsAggregator => true;

        public IPropertyListingSearchProvider? SearchProvider => this;

        public IPropertyListingProvider? ListingProvider => this;

        public IPropertyDataProvider DataProvider => this;

        public Hemnet(IDataFetcher dataFetcher)
        {
            this.dataFetcher = dataFetcher;
        }

        public async Task<FetchResult> FetchSearchListings(PropertyFilter? filter = null, int skip = 0, int take = 100)
        {
            var curl = new CurlRequest("""curl 'https://www.hemnet.se/bostader?location_ids[]=' --globoff -H 'User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/110.0' -H 'Accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8' -H 'Accept-Language: en-US,en;q=0.5' -H 'Accept-Encoding: gzip, deflate, br' -H 'DNT: 1' -H 'Connection: keep-alive' -H 'Cookie: hn_exp_kpis=41; hn_exp_bsp=522; hn_exp_sss=517; hn_exp_bts=532; _hemnet_session_id=RnRKUDcwZXpXUS9oeW5vSkJ4RE5HbTJnUXpRbnBIcytPUnFuUGZxMElkaVlROFV3Z2ZOV0FUVXJ0ZURjUm02TTFINHg4TXNCZE1id281ZGRPRmlFYUJvY1VLL1h4VGIyRExudDZyTEJsZW9aL0J3TkZnS0RDOTBZZXd4WmdpNWtjc0hNem5TbFpSOXhQTS8xVnU0R1NRZ005ZG5haFZPQWpQdjQyVStYUFFvN2JSWDV1cWFIaG9MQWgxUzNPcGxRLS13Y2hhMTBFMjNXMEZnQzRYOWxyTVh3PT0%3D--f9e964029135b7e2c6362a96bd9a1e04e8c55d79; __cfruid=2d51bb928f153b34af8444afa15d81525da056c6-1676962461; __couid=102cbc3f-684c-4d38-ac77-cca836d55260; __codnp=; _gcl_au=1.1.1194022863.1676962466; _ga=GA1.1.514366971.1676962462; _ga_42YRRBRWVM=GS1.1.1676962461.1.1.1676963086.0.0.0; _ga2=GA1.1.514366971.1676962462; _hemnet_listing_result_settings_list=normal; _hemnet_listing_result_settings_sorting=creation+desc' -H 'Upgrade-Insecure-Requests: 1' -H 'Sec-Fetch-Dest: document' -H 'Sec-Fetch-Mode: navigate' -H 'Sec-Fetch-Site: none' -H 'Sec-Fetch-User: ?1' -H 'Pragma: no-cache' -H 'Cache-Control: no-cache' -H 'TE: trailers'""");
            var content = await dataFetcher.Fetch(curl);
            //var listings = ParseSearchResults(curl.Uri, content);

            return new FetchResult { Source = curl.Uri, Content = content };
        }

        public async Task<FetchResult> FetchListing(string objectId)
        {
            var curl = new CurlRequest("""curl 'https://www.hemnet.se/bostad/lagenhet-2rum-masta-eskilstuna-kommun-tomtebogatan-2a-19640255' --globoff -H 'User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/110.0' -H 'Accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8' -H 'Accept-Language: en-US,en;q=0.5' -H 'Accept-Encoding: gzip, deflate, br' -H 'DNT: 1' -H 'Connection: keep-alive' -H 'Cookie: hn_exp_kpis=41; hn_exp_bsp=522; hn_exp_sss=517; hn_exp_bts=532; _hemnet_session_id=RnRKUDcwZXpXUS9oeW5vSkJ4RE5HbTJnUXpRbnBIcytPUnFuUGZxMElkaVlROFV3Z2ZOV0FUVXJ0ZURjUm02TTFINHg4TXNCZE1id281ZGRPRmlFYUJvY1VLL1h4VGIyRExudDZyTEJsZW9aL0J3TkZnS0RDOTBZZXd4WmdpNWtjc0hNem5TbFpSOXhQTS8xVnU0R1NRZ005ZG5haFZPQWpQdjQyVStYUFFvN2JSWDV1cWFIaG9MQWgxUzNPcGxRLS13Y2hhMTBFMjNXMEZnQzRYOWxyTVh3PT0%3D--f9e964029135b7e2c6362a96bd9a1e04e8c55d79; __cfruid=2d51bb928f153b34af8444afa15d81525da056c6-1676962461; __couid=102cbc3f-684c-4d38-ac77-cca836d55260; __codnp=; _gcl_au=1.1.1194022863.1676962466; _ga=GA1.1.514366971.1676962462; _ga_42YRRBRWVM=GS1.1.1676962461.1.1.1676963086.0.0.0; _ga2=GA1.1.514366971.1676962462; _hemnet_listing_result_settings_list=normal; _hemnet_listing_result_settings_sorting=creation+desc' -H 'Upgrade-Insecure-Requests: 1' -H 'Sec-Fetch-Dest: document' -H 'Sec-Fetch-Mode: navigate' -H 'Sec-Fetch-Site: none' -H 'Sec-Fetch-User: ?1' -H 'Pragma: no-cache' -H 'Cache-Control: no-cache' -H 'TE: trailers'""");
            var req = curl.HttpRequestMessage;
            req.RequestUri = req.RequestUri!.ReplacePath(objectId);
            var content = await dataFetcher.Fetch(req);

            return new FetchResult { Source = curl.Uri, Content = content };
        }

        public PropertyListing ParseListing(Uri source, string html)
        {
            var (doc, head, body) = ParseTools.ParseHtmlWithHeadAndBody(html, source);
            var scripts = head.QuerySelectorAllOrThrow("script")
                .OfType<IHtmlScriptElement>()
                .Where(o => o.Type == "application/ld+json")
                .Select(o => o.Text())
                .Select(o => ExTools.TryOrDefault(() => JObject.Parse(o), null))
                .OfType<JObject>()
                .ToList();

            // Event 

            var addressJson = scripts.FirstOrDefault(o => o["@type"]?.Value<string>() == "Place")?["address"];
            var offersJson = scripts.FirstOrDefault(o => o["@type"]?.Value<string>() == "Product")?["offers"];

            //"@type": "Offer",
            //"priceCurrency": "SEK",
            //"price": 1250000,
            //"priceValidUntil": "2025-02-21T07:14:24+0100",
            //"availability": "http://schema.org/InStock",
            //"validFrom": "2023-02-21T07:14:24+0100",
            //"url": "https://www.hemnet.se/bostad/lagenhet-2rum-masta-eskilstuna-kommun-tomtebogatan-2a-19640255"

            var price = ParseTools.ParseNumber(body.QuerySelectorOrThrow(".property-info__price").Text());

            var brokerLink = new Uri(body.QuerySelectorOrThrow<IHtmlAnchorElement>(".property-description__broker-button").Href);
            brokerLink = brokerLink.ReplaceQuery("");


            var dlElements = body.QuerySelectorAll("dl");

            var labels = dlElements.SelectMany(o => o.QuerySelectorAll(".property-attributes-table__label"));
            var dict = labels
                .Select(lbl => new { Key = lbl.Text(), Value = lbl.ParentElement!.QuerySelectorOrThrow("dd").Text().Trim() })
                .Where(o => o.Key.Any())
                .ToDictionary(o => o.Key, o => (string?)o.Value);

            return new PropertyListing {
                ListingId = "",
                Provider = this,
                CrawledFrom = source,
                UriWithProvider = source,
                RealtorListingPage = brokerLink,

                Price = (int?)price,

                Property = new Property
                {
                    Type = dict.GetValueOrDefault("Bostadstyp") switch
                    {
                        "Lägenhet" => PropertyType.Apartment, // TODO:
                        _ => PropertyType.Apartment,
                    },
                    // [Driftkostnad, 4 800 kr / år]
                    // [Upplåtelseform, Bostadsrätt]
                    // [Förening, HSB BRF Slagtäppan i Eskilstuna \n Om föreningen]}
                    NumRooms = ParseTools.ParseNumber(dict, "Antal rum"),
                    LivingArea = ParseTools.ParseNumber(dict, "Boarea"),
                    MonthlyPayment = (int?)ParseTools.ParseNumber(dict, "Avgift"),
                    Floor = ParseTools.ParseNumber(dict, "Våning"), // [Våning, 1, hiss finns ej]}
                    Address = new Address
                    {
                        StreetAddress = addressJson?["streetAddress"]?.Value<string>(),
                        ZipCode = addressJson?["postalCode"]?.Value<string>(),
                        City = addressJson?["addressLocality"]?.Value<string>(),
                        //"addressRegion": "Södermanlands län"
                        //"addressCountry": "SE"
                    }
                }
            };
        }

        public List<PropertyListing> ParseSearchListings(Uri source, string html)
        {
            var (doc, head, body) = ParseTools.ParseHtmlWithHeadAndBody(html, source);

            var urls = ParseHeadUris(head);

            var resultElement = body.QuerySelectorOrThrow("#result");

            var lis = resultElement.Children.Single(o => o.NodeName == "UL").Children;

            var result = lis.Select(li => {
                var elLink = (IHtmlAnchorElement?)li.QuerySelector("a");
                if (elLink == null)
                    return null;
                var link = elLink.Href;

                var streetAddress = li.QuerySelector(".listing-card__street-address")?.Text().Trim();
                var location = li.QuerySelector(".listing-card__location-name")?.Text().Trim();

                var elAttribs = li.QuerySelector(".listing-card__attributes-container");
                if (elAttribs == null)
                    throw new Exception("no attribs");

                var desc = elAttribs.Descendents()
                    .Where(o => o.NodeType == NodeType.Text)
                    .Select(o => o.Text().Trim())
                    .Where(o => o.Length > 0)
                    .ToList();

                FindAndRemove("kr/m²");

                var compositeIndices = desc.Select((o, i) => new { Index = i, HasPlus = o.StartsWith("+") }).Where(o => o.HasPlus).Select(o => o.Index).ToList();
                if (compositeIndices.Any())
                {
                    compositeIndices.Reverse();
                    compositeIndices.ToList().ForEach(index =>
                    {
                        var joined = $"{desc[index - 1]} {desc[index]}";
                        desc[index - 1] = joined;
                        desc.RemoveAt(index);
                    });
                }

                var property = new Property
                {
                    YardArea = ParseTools.ParseNumber(FindAndRemove("m² tomt")),
                    NumRooms = ParseTools.ParseNumber(FindAndRemove("rum")),
                    MonthlyPayment = (int?)ParseTools.ParseNumber(FindAndRemove("kr/mån")),
                    Address = new Address { StreetAddress = streetAddress, Region = location }
                };

                var livingArea = FindAndRemove("m²")?.Split('+');
                if (livingArea != null)
                {
                    property.LivingArea = ParseTools.ParseNumber(livingArea[0]);
                    if (livingArea.Length > 1)
                        property.AdditionalArea = ParseTools.ParseNumber(livingArea[1]);
                }

                var price = (int?)ParseTools.ParseNumber(FindAndRemove("kr"));

                int FindIndex(string contains) => desc.FindIndex(o => o.Contains(contains));
                string? FindAndRemove(string contains)
                {
                    var index = FindIndex(contains);
                    if (index < 0)
                        return null;
                    var val = desc[index].Replace(contains, "");
                    if (contains == "kr")
                    { }
                    desc.RemoveAt(index);
                    return val;
                }

                return new PropertyListing
                {
                    ListingId = link, // TODO
                    Provider = this,
                    CrawledFrom = source,
                    Price = price,
                    //UriWithProvider = 
                    //RealtorListingPage = 

                    Property = property,
                };
            }).OfType<PropertyListing>().ToList();

            return result;
        }

        List<string?> ParseHeadUris(IHtmlHeadElement head)
        {
            var itemListJson = head.QuerySelectorAllOrThrow("script")
                .OfType<IHtmlScriptElement>()
                .Where(o => o.Type == "application/ld+json")
                .Select(o => o.Text())
                .Where(o => o.Contains("ItemList"))
                .SingleOrDefault();

            if (string.IsNullOrEmpty(itemListJson))
                throw new Exception($"ItemList JSON not found in head");

            var jObj = JObject.Parse(itemListJson);
            if (jObj == null)
                throw new Exception($"Couldn't parse ItemList JSON");

            var items = jObj["itemListElement"] as JArray;
            if (items == null)
                throw new Exception($"itemListElement not found");

            return items.Select(o => o["url"]?.Value<string>()).ToList();
        }
    }
}
