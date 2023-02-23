using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Parsers.Models;
using System;
using System.Data;
using System.Text.RegularExpressions;

namespace Parsers.Providers
{
    public class SvenskFast : IPropertyDataProvider, IPropertyListingSearchProvider, IPropertyListingProvider
    {
        private readonly IDataFetcher dataFetcher;

        public Uri DefaultUri => new Uri("https://www.svenskfast.se");

        public IPropertyDataProvider DataProvider => this;

        public string Id => nameof(SvenskFast);

        public bool IsAggregator => false;

        public IPropertyListingSearchProvider? SearchProvider => this;

        public IPropertyListingProvider? ListingProvider => this;

        public SvenskFast(IDataFetcher dataFetcher)
        {
            this.dataFetcher = dataFetcher;
        }

        public async Task<IPropertyListingSearchProvider.FetchResult> FetchPropertySearchResults(PropertyFilter? filter = null, int skip = 0, int take = 100)
        {
            var uri = new Uri("https://www.svenskfast.se/bostad/");
            var html = await dataFetcher.Fetch(uri);

            return new IPropertyListingSearchProvider.FetchResult { Content = html, Source = uri }; // ParseSearchResults(uri, html) };
        }

        public async Task<IPropertyListingProvider.FetchResult> FetchPropertyListingResult(string objectId)
        {
            var uri = DefaultUri.ReplacePathAndQuery(objectId);
            var html = await dataFetcher.Fetch(uri);
            
            return new IPropertyListingProvider.FetchResult { RawResult = html, Listing = ParseListingPage(uri, html) };
        }

        public PropertyListing ParseListingPage(Uri source, string html)
        {
            var (doc, head, body) = ParseTools.ParseHtmlWithHeadAndBody(html, source);

            var stringToFind = "objectInfo:";
            var script = head.GetElementsByTagName("script")
                .Select(o => o.Text())
                .Where(o => o.Contains("window.dataLayer") && o.Contains(stringToFind))
                .SingleOrDefault();

            if (script == null)
                throw new Exception($"Script tag not found in {source}");

            var index = script.IndexOf(stringToFind) + stringToFind.Length;
            script = script.Substring(index);
            var m = Regex.Match(script, @"\s+\}(\s+)");
            if (!m.Success)
                throw new Exception($"Couldn't parse script: {source}");
                
            script = script.Remove(m.Groups[1].Index);
            //{
            //    id: '355940',
            //    type: 'Bostadsrättslägenhet',
            //    form: 'Bostadsrätt',
            //    postalCode: '13173',
            //    city: 'Nacka',
            //    price: '20000000',
            //    currency: 'SEK',
            //    rooms: '5',
            //    area: '198',
            //    constructionYear: '2015',
            //    monthlyFee: '14368',
            //    service: 'Premium',
            //    broker: 'sfchstm',
            //    sqmPrice: '101010',
            //    priceType: 'Pris (Utgångspris)',
            //    numberOfObjects: '',
            //    projectName: ''
            //}

            var objectDescription = body.QuerySelectorOrThrow(".object-description__info");

            var dict = objectDescription.QuerySelectorAll("li")
                .Select(o => new
                {
                    Key = o.ChildNodes.Single(p => p.NodeType == NodeType.Text).Text().Trim().Trim(':'),
                    Value = o.ChildNodes.Single(p => p.NodeType == NodeType.Element).Text()
                }).ToDictionary(o => o.Key, o => (string?)o.Value);
            if (dict == null)
                throw new Exception($"object-description__info not found");
            //"Typ": "Bostadsrättslägenhet",
            //"Rum": "5, varav 3 sovrum",
            //"Boarea": "198 kvm",
            //"Lägenhetsnummer": "1202",
            //"Byggnadsår": "2015",
            //"Månadsavgift": "14 368 kr",
            //"Upplåtelseform": "Bostadsrätt",
            //"Våning": "6+7",
            //"Hiss": "Ja"

            var bidsTable = body.QuerySelector(".object__bidding--tbl") as IHtmlTableElement; //bid__bids
            //object-description__info
            var bids = bidsTable?
                .ToDictionaries()
                .Select(kv => new Bid
                {
                    Bidder = kv.GetValueOrDefault("Budgivare", "") ?? "",
                    Price = (int)(ParseTools.ParseNumber(kv, "Bud") ?? 0),
                    Time = ParseTools.ParseDate(kv, "Datum") ?? new DateTimeOffset(DateTimeOffset.Now.Year, 1, 1, 0, 0, 0, TimeSpan.Zero) 
                })
                .ToList();

            return new PropertyListing
            {
                ListingId = source.PathAndQuery,
                Provider = this,
                CrawledFrom = source,
                Bids = bids?.ToList() ?? new(),
                Property = new Property
                {
                    Type = dict["Typ"] switch
                    {
                        "Bostadsrättslägenhet" => PropertyType.Apartment,
                        _ => PropertyType.Apartment,
                    },
                    LivingArea = ParseTools.ParseNumber(dict, "Boarea"),
                    NumRooms = ParseTools.ParseNumber(dict, "Rum"),
                    DateConstructed = ParseTools.ParseYear(dict, "Byggnadsår"),
                    MonthlyPayment = (int?)ParseTools.ParseNumber(dict, "Månadsavgift"),
                }
            };
        }

        public List<PropertyListing> ParseSearchResults(Uri source, string html)
        {
            var (doc, head, body) = ParseTools.ParseHtmlWithHeadAndBody(html, source);

            var items = body.QuerySelectorAll("a > article")
                .Select(o => o.ParentElement)
                .OfType<IHtmlAnchorElement>(); //IElement

            var result = items.Select(o =>
            {
                //var parent = o.ParentElement as IHtmlAnchorElement;
                //if (parent == null)
                //    throw new Exception($"No anchor found");

                var info = o.QuerySelectorOrThrow(".search-hit__info--text").Children.Select(o => o.Text());
                return new PropertyListing
                {
                    ListingId = o.Href,
                    Provider = this,
                    CrawledFrom = source,
                    RealtorListingPage = DefaultUri.ReplacePathAndQuery(o.GetAttributeOrThrow("href")),

                    Property = new Property
                    {
                        Address = new Address { }, // string.Join(", ", o.QuerySelector(".search-hit__address").Children.Select(o => o.Text())),
                        NumRooms = ParseTools.ParseNumber(Find("rok")),
                        LivingArea = ParseTools.ParseNumber(Find("kvm")),
                    },
                    Price = (int?)ParseTools.ParseNumber(Find("kr"))
                };

                string? Find(string contains)
                {
                    var found = info.FirstOrDefault(x => x.Contains(contains));
                    return found?.Replace(contains, "").Trim();
                }
            }).ToList();

            return result;
        }
    }
}
