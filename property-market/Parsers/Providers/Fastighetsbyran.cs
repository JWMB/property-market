using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Newtonsoft.Json;
using Parsers.Models;
using System.Text.Json.Nodes;

namespace Parsers.Providers
{
    public class Fastighetsbyran : IPropertyDataProvider, IPropertyListingSearchProvider, IPropertyListingProvider
    {
        private readonly IDataFetcher dataFetcher;

        public Uri DefaultUri => new Uri("https://www.fastighetsbyran.com");

        public string Id => nameof(Fastighetsbyran);

        public bool IsAggregator => false;

        public IPropertyListingSearchProvider? SearchProvider => this;

        public IPropertyListingProvider? ListingProvider => this;

        public IPropertyDataProvider DataProvider => this;

        public Fastighetsbyran(IDataFetcher dataFetcher)
        {
            this.dataFetcher = dataFetcher;
        }

        public async Task<FetchResult> FetchSearchListings(PropertyFilter? filter = null, int skip = 0, int take = 100)
        {
            // https://www.fastighetsbyran.com/sv/sverige/till-salu
            var curl = new CurlRequest("""curl 'https://www.fastighetsbyran.com/HemsidanAPI/api/v1/soek/objekt/1/false/' -X POST -H 'User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/109.0' -H 'Accept: application/json' -H 'Accept-Language: en-US,en;q=0.5' -H 'Accept-Encoding: gzip, deflate, br' -H 'Referer: https://www.fastighetsbyran.com/sv/sverige/till-salu' -H 'Content-Type: application/json' -H 'spraak: sv' -H 'webbmarknad: 204' -H 'X-Forwarded-For: 90.143.11.236' -H 'Origin: https://www.fastighetsbyran.com' -H 'DNT: 1' -H 'Connection: keep-alive' -H 'Cookie: FSDANONYMOUS=MBeI5zt52QEkAAAANmFhODEwN2ItODM1ZS00Y2VjLWI5NWYtOTUwYWViODM3NDQ5_PQ14fr2nVHHTAlsarTzYtnBdSY1; CookieConsent={stamp:%27tG2idt5oRPXK2ifI0j4cKPCMnne99GI4lFfpYgG8qPRGpBNeVPBEjw==%27%2Cnecessary:true%2Cpreferences:true%2Cstatistics:false%2Cmarketing:false%2Cmethod:%27explicit%27%2Cver:1%2Cutc:1676645281660%2Cregion:%27se%27}; ForSaleLargeGallery=true' -H 'Sec-Fetch-Dest: empty' -H 'Sec-Fetch-Mode: cors' -H 'Sec-Fetch-Site: same-origin' -H 'Pragma: no-cache' -H 'Cache-Control: no-cache' --data-raw '{"valdaMaeklarObjektTyper":[],"valdaNyckelord":[],"valdaLaen":[],"valdaKontor":[],"valdaKommuner":[],"valdaNaeromraaden":[],"valdaPostorter":[],"inkluderaNyproduktion":true,"inkluderaPaaGaang":true,"positioner":[]}'""");
            var json = await dataFetcher.Fetch(curl);

            return new FetchResult
            {
                Content = json,
                Source = curl.Uri //ParseSearchResults(curl.Uri, json),
            };
        }

        public async Task<FetchResult> FetchListing(string objectId)
        {
            var curl = new CurlRequest("curl 'https://www.fastighetsbyran.com/ObjektpresentationAPI/api/Visning/2804947' -H 'User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/109.0' -H 'Accept: application/json' -H 'Accept-Language: en-US,en;q=0.5' -H 'Accept-Encoding: gzip, deflate, br' -H 'Referer: https://www.fastighetsbyran.com/sv/sverige/objekt/?objektid=2804947' -H 'spraak: sv' -H 'webbmarknad: 204' -H 'X-Forwarded-For: 90.143.11.236' -H 'DNT: 1' -H 'Connection: keep-alive' -H 'Cookie: FSDANONYMOUS=MBeI5zt52QEkAAAANmFhODEwN2ItODM1ZS00Y2VjLWI5NWYtOTUwYWViODM3NDQ5_PQ14fr2nVHHTAlsarTzYtnBdSY1; CookieConsent={stamp:%27tG2idt5oRPXK2ifI0j4cKPCMnne99GI4lFfpYgG8qPRGpBNeVPBEjw==%27%2Cnecessary:true%2Cpreferences:true%2Cstatistics:false%2Cmarketing:false%2Cmethod:%27explicit%27%2Cver:1%2Cutc:1676645281660%2Cregion:%27se%27}; ForSaleLargeGallery=true' -H 'Sec-Fetch-Dest: empty' -H 'Sec-Fetch-Mode: cors' -H 'Sec-Fetch-Site: same-origin' -H 'Pragma: no-cache' -H 'Cache-Control: no-cache'");
            var req = curl.HttpRequestMessage;
            req.RequestUri = req.RequestUri!.ReplacePath(str => str.Replace("2804947", objectId));
            var json = await dataFetcher.Fetch(curl);

            return new FetchResult
            {
                Content = json,
                Source = curl.Uri
                //Listing = ParseItemPage(curl.Uri, json)
            };
        }

        public List<PropertyListing> ParseSearchListings(Uri source, string json)
        {
            var root = JsonConvert.DeserializeObject<Root>(json);
            if (root == null)
                throw new Exception($"Couldn't parse json {source}");

            return root.results.Select(o => {
                return new PropertyListing
                {
                    CrawledFrom = source,
                    Provider = this,
                    ListingId = "", // TODO
                                    // TODO: RealtorListingPage = 
                    Property = new Property
                    {
                        Type = o.bostadsTyp switch
                        {
                            _ => PropertyType.Apartment // TODO
                        },
                    }
                };
            }).ToList();
        }

        public PropertyListing ParseListing(Uri source, string html)
        {
            var (doc, head, body) = ParseTools.ParseHtmlWithHeadAndBody(html, source);

            var headerJson = head.Children.OfType<IHtmlScriptElement>()
                .Where(o => o.Type == "application/ld+json")
                .Select(o => ExTools.TryOrDefault(() => JsonObject.Parse(o.InnerHtml), null))
                .FirstOrDefault();

            // h5 budgivning / parent / parent -> div>span

            if (headerJson != null)
            {
                //{
                //"@context": "https://schema.org",
                //"@type": ["RealEstateListing", "House", "Offer"],
                //"name": "Vällingby Parkstad / Råcksta, Stockholm, Ljusdalsgatan 73",
                //"description": "Unikt boende i omtyckta och växande Vällingby Parkstad på gränsen till Bromma med närhet till både tunnelbana och Vällingby centrum. Bostaden erbjuder utöver påkostade materialval och en unik design två stora terrasser för soliga dagar och härliga kvällar. Detta är boendet för familjen som söker ett modernt och lyxigt boende och som vill ha både radhuskänslan men samtidigt den underhållsfria bekvämligheten som en lägenhet ger. I närområdet finner du gym, matbutiker och tunnelbanan ca 3 min bort. Mälarens glittrande vatten och badklippor/stränder finner du en kort cykeltur bort. Välkommen hem!",
                //"numberOfRooms": 5,
                //"image": "https://media.fastighetsbyran.se/38147928.jpg",
                //"floorSize": {
                //  "@type": "QuantitativeValue",
                //  "value": "134 kvm",
                //  "unitCode": "MTK"
                //},
                //"yearBuilt": "2021",
                //"address": {
                //  "@type": "PostalAddress",
                //  "addressCountry": "SV",
                //  "addressRegion": "Vällingby Parkstad / Råcksta, Stockholm",
                //  "streetAddress": "Ljusdalsgatan 73"
                //},
                //"provider": {
                //  "@type": "RealEstateAgent",
                //  "name": "Fastighetsbyrån AB",
                //  "telephone": "08-445 84 60",
                //  "address": "Box 240\r\n16213 Vällingby",
                //  "image": "https://www.fastighetsbyran.com/static/media/fastighetsbyran_logo.c970934692dae071ab18efef0a997377.svg",
                //  "email": "mailto:jimmy.armasson@fastighetsbyran.se?Subject=Ljusdalsgatan%2073%20(Webbnr:%201410-29909)"
                //}
                //}
            }

            var keyValue = body.QuerySelectorAllOrThrow("div > h3")
                .Select(o => o.ParentElement)
                .OfType<IElement>()
                .Where(o => o.ChildElementCount == 2)
                .Select(o => o.Children.Select(o => o.Text()).ToList())
                .Select(o => new { Key = o[0], Value = o[1] })
                .ToList();

            var dict = new Dictionary<string, string?>();
            foreach (var item in keyValue)
            {
                if (dict.TryGetValue(item.Key, out var value))
                {
                    if (value != item.Value)
                    { }
                }
                else
                    dict.Add(item.Key, item.Value);
            }
            //"Upplåtelseform": "Bostadsrätt",
            
            //"Webbnr": "1410-29909",
            //"Lägenhetsnummer": "1020",
            //"Andel i föreningen": "0,047051 ",
            //"Andel av årsavgift": "0,0471 %",
            //"Byggnadstyp": "Bostadsrättsradhus",
            //"Byggnadsår": "2021",
            //"Uppvärmning": "Frånluftsvärmepump, vattenburen distribution",
            //"Typ av ventilation": "Mekanisk frånluft via frånluftsvärmepump",
            //"Elförbrukning": " 10 786 kWh/år",
            //"Försäkring": " 2 148 kr/år",
            //"Förening": "Brf Parkstaden i Råcksta"

            var area = (dict.GetValueOrDefault("Boarea/Biarea", "")?.Split("/").Where(o => o.Any()) ?? new string[0]).ToList(); //"Boarea/Biarea": "134 kvm",
            var yearConstructed = ParseTools.ParseNumber(dict["Byggår"]);

            return new PropertyListing
            {
                Provider = this,
                CrawledFrom = source,
                ListingId = source.Segments.Last(),
                UriWithProvider = source,
                RealtorListingPage = source,

                Price = (int?)ParseTools.ParseNumber(dict.GetValueOrDefault("Pris", "")), //"Pris": "5 545 000 kr utgångspris",
                PriceType = dict.GetValueOrDefault("Pris", "")?.Contains("accept") == true ? PriceType.Accept : PriceType.Starting, // utgångspris

                Property = new Property
                {
                    DateConstructed = yearConstructed == null ? null : new DateTimeOffset((int)yearConstructed, 1, 1, 0, 0, 0, TimeSpan.Zero),
                    Type = dict["Typ"] switch
                    {
                        "Radhus" => PropertyType.Terraced, // TODO:
                        _ => PropertyType.Unknown,
                    },
                    NumRooms = ParseTools.ParseNumber(dict, "Antal rum"), //"Antal rum": "5 rum + kök",
                    LivingArea = area.Count > 0 ? ParseTools.ParseNumber(area[0]) : null,
                    AdditionalArea = area.Count > 1 ? ParseTools.ParseNumber(area[1]) : null,

                    MonthlyPayment = (int?)ParseTools.ParseNumber(dict, "Avgift"), //"Avgift": " 5 774 kr/mån",
                    CoopId = dict.GetValueOrDefault("Förening", null),
                    OwnershipPartOfCoop = ParseTools.ParseNumber(dict, "Andel i föreningen"),
                }
            };
        }
        
        class SearchBody
        {
            public List<string> valdaMaeklarObjektTyper { get; set; } = new();
            public List<string> valdaLaen { get; set; } = new();
            public List<string> valdaKontor { get; set; } = new();
            public List<string> valdaKommuner { get; set; } = new();

            public List<string> valdaNaeromraaden { get; set; } = new();
            public List<string> valdaPostorter { get; set; } = new();
            public bool inkluderaNyproduktion { get; set; } = true;
            public bool inkluderaPaaGaang { get; set; } = true;
            public List<string> positioner { get; set; } = new();
        }

        public class Result
        {
            public int maeklarObjektId { get; set; }
            public string bildUrl { get; set; } = string.Empty;
            public List<string> bildUrlLista { get; set; } = new();
            public string litenRubrik { get; set; } = string.Empty;
            public string storRubrik { get; set; } = string.Empty;
            public List<string> metaData { get; set; } = new();
            public double xKoordinat { get; set; }
            public double yKoordinat { get; set; }
            public bool paaGang { get; set; }
            public bool budgivningPagaar { get; set; }
            public bool aerNyproduktion { get; set; }
            public bool aerProjekt { get; set; }
            public bool aerReferensobjekt { get; set; }
            public bool harDigitalLiveVisning { get; set; }
            public int maeklarId { get; set; }
            public object? avtalsdag { get; set; }
            public DateTime senasteTidObjektetBlevTillSalu { get; set; }
            public DateTime? senasteTidpunktSomObjektetBlevIntagetOchSkallAnnoserasFastStatusBaraIntaget { get; set; }
            public string bostadsTyp { get; set; } = string.Empty;
            public int boendeform { get; set; }
            public List<int> delomraaden { get; set; } = new();
        }

        public class Root
        {
            public List<Result> results { get; set; } = new();
            public int currentPage { get; set; }
            public int pageCount { get; set; }
            public int pageSize { get; set; }
            public int rowCount { get; set; }
        }
    }
}
