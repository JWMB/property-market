using ConsoleApp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Microsoft.Playwright;
using Parsers;
using Parsers.Providers;
using System.Net;

//var provider = new Booli();
//await provider.LoadListingPage();
//await provider.ParseListingPage(File.ReadAllText(@"C:\Users\uzk446\Desktop\Bool.html"));
//var item = await provider.ParseItemPage(File.ReadAllText(@"C:\Users\uzk446\Desktop\BooliItem.html"));

var serviceCollection = new ServiceCollection();
serviceCollection.AddHttpClient();
serviceCollection.AddHttpClient("Default")
    .ConfigureHttpClient((sp, httpClient) =>
    {
        //httpClient.BaseAddress = options.Url;
        //httpClient.Timeout = options.RequestTimeout;
    })
    .SetHandlerLifetime(TimeSpan.FromMinutes(5))
    .ConfigurePrimaryHttpMessageHandler(x => new HttpClientHandler { AutomaticDecompression = DecompressionMethods.Brotli | DecompressionMethods.GZip | DecompressionMethods.Deflate });
//.AddHttpMessageHandler(sp => sp.GetService<SomeCustomHandler>().CreateAuthHandler())
//.AddPolicyHandlerFromRegistry(PollyPolicyName.HttpRetry)

serviceCollection.AddSingleton<IDataFetcher, HttpClientDataFetcher>();

var serviceProvider = serviceCollection.BuildServiceProvider();
var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();


var storagePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "property-market");

IPropertyDataProvider provider;

provider = serviceProvider.CreateInstance<Fastighetsbyran>(); // Lansfast Hemnet Fastighetsbyran

//var aa = ((Hemnet)provider).ParseListingsPage(new Uri("http://a"), File.ReadAllText("Hemnet_list_230221_0854.dat"));
string? listingId = "https://www.lansfast.se/till-salu/villa/blekinge/karlshamn/svangsta/nissavagen-11/cmvilla5aba5m3qk4vosdsb/"; 
////"https://www.hemnet.se/bostad/lagenhet-2rum-masta-eskilstuna-kommun-tomtebogatan-2a-19640255";
if (true)
{
    var listResult = await provider.SearchProvider!.FetchSearchListings();
    WriteFile($"{provider.Id}_list_{DateTime.Now:yyMMdd_HHmm}.dat", listResult.Content);
    listingId = provider.SearchProvider!.ParseSearchListings(listResult.Source, listResult.Content).FirstOrDefault()?.ListingId;
}
//var aa = ((Lansfast)provider).ParsePropertyListings(new Uri("http://a"), ReadFile("Lansfast.dat")); //Hemnet_item_230221_1227.dat

if (false)
{
    var itemResult = await provider.ListingProvider!.FetchListing(listingId); // "ObjektpresentationAPI/api/Visning/2804947");
    WriteFile($"{provider.Id}_item_{DateTime.Now:yyMMdd_HHmm}.dat", itemResult.Content);
}

//((Lansfast)provider).ParsePropertyListing(new Uri("http://a"), File.ReadAllText("Lansfast_item_230222_0739.dat"));
provider.ListingProvider!.ParseListing(new Uri("http://a"), ReadFile("Lansfast_item_230222_0739.dat"));

//var item = await provider.ParseItemPage(File.ReadAllText(@"C:\Users\uzk446\Desktop\fastighetsbyran-object.html"));

//var provider = new SvenskFast(httpClientFactory);
//var item = await provider.ParseListingPage(File.ReadAllText(@"C:\Users\uzk446\Desktop\svenskfastList.html"));
//var html = await provider.LoadItemPage(new Uri("https://www.svenskfast.se/bostadsratt/stockholm/nacka/nacka/sicklaon/saltsjoqvarn/ostra-finnbodavagen-13b/355940"));
//var html = await provider.LoadItemPage(new Uri("https://www.svenskfast.se/fritidshus/vastra-gotaland/tanum/havstenssund/havstenssund/clasegrand-3/363219"));
//var result = provider.ParseItemPage(new Uri("https://a/b"), File.ReadAllText(@"C:\Users\uzk446\Desktop\svenskfastItem.html"));

return;

string ReadFile(string filename) => File.ReadAllText(Path.Combine(storagePath, filename));
void WriteFile(string filename, string content) => File.WriteAllText(Path.Combine(storagePath, filename), content);

using var pw = await Playwright.CreateAsync();
await using var browser = await pw.Chromium.LaunchAsync();

var page = await browser.NewPageAsync();

//page.RequestFinished += Page_RequestFinished;
//page.RouteAsync("/api/**", async r => {
//    r.Request.ResponseAsync
//    })
page.Response += async (sender, response) =>
{
    if (response.Ok)
    {
        var contentType = (await response.HeaderValueAsync("content-type") ?? "").ToLower().Trim();
        Console.WriteLine($"{response.Request.Url}: {contentType}");
        //response.Request.Url.ToString().ToLower().Contains("/api/")
        if (contentType.StartsWith("application/json"))
        {
            try
            {
                var json = await response.JsonAsync();
                var len = json?.ToString().Length;
            }
            catch (Exception ex)
            { }
        }
        else if (contentType.StartsWith("text/") || (contentType.StartsWith("application/") && contentType.Contains("javascript")))
        {
            //text/html
            //text/css
            //text/javascript
            var text = await response.TextAsync();
            if (contentType.StartsWith("text/html"))
            {
            }
        }
        else if (contentType == "application/x-protobuf")
        { }
        else if (contentType.StartsWith("font/"))
        { }
        else if (contentType.StartsWith("image/"))
        { }
        else if (contentType.Any() == false)
        { }
        else
        {
        }

    }
};

//void Page_RequestFinished(object? sender, IRequest e)
//{
//    var xx = HandleResponse(e).Result;
//    async Task<string?> HandleResponse(IRequest r)
//    {
//        try
//        {
//            var response = await e.ResponseAsync();
//            if (response?.Ok == true)
//            {
//                try
//                {
//                    var json = response.JsonAsync();
//                    return json?.ToString();
//                }
//                catch (Exception ex)
//                {
//                }
//            }
//        }
//        catch (Exception x)
//        { }

//        return null;
//    }
//}

//var response = await page.WaitForResponse(response => response.url().includes('/some_url/') && response.status() === 200);
//console.log('RESPONSE ' + (await response.body()));
await page.GotoAsync("https://www.booli.se/sverige/77104"); // http://webcode.me");

var title = await page.TitleAsync();
Console.WriteLine(title);