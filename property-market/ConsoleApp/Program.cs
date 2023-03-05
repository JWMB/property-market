using ConsoleApp;
using Crawling;
using Microsoft.Extensions.DependencyInjection;
using Parsers;
using Parsers.Providers;
using System.Net;

//var provider = new Booli();
//await provider.LoadListingPage();
//await provider.ParseListingPage(File.ReadAllText(@"C:\Users\uzk446\Desktop\Bool.html"));
//var item = await provider.ParseItemPage(File.ReadAllText(@"C:\Users\uzk446\Desktop\BooliItem.html"));

var serviceCollection = new ServiceCollection();
serviceCollection.AddHttpClient();
serviceCollection.AddHttpClient(HttpClientDataFetcher.HttpClientName)
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

var storagePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "property-market");

serviceCollection.AddSingleton<IRawDataRepository>(sp => new FileSystemRawDataRepository(folderPath: Path.Combine(storagePath, "Fetched")));
serviceCollection.AddSingleton<ICrawlQueueSender, InMemoryCrawlQueue>();
serviceCollection.AddSingleton<IListingsRepository, InMemoryListingsRepository>();
serviceCollection.AddSingleton<ICrawlStateRepository, InMemoryCrawlStateRepository>();
serviceCollection.AddSingleton<IEnumerable<IPropertyDataProvider>>(sp => new[] { sp.CreateInstance<Hemnet>() });

var serviceProvider = serviceCollection.BuildServiceProvider();

var crawler = serviceProvider.CreateInstance<Crawler>();

var until = DateTimeOffset.UtcNow.Add(TimeSpan.FromMinutes(10));
while (DateTimeOffset.UtcNow < until)
{
    await crawler.PerformDueSearches();
    await crawler.QueueOpenListings();
    await Task.Delay(1000);
}
//await Test();


async Task Test()
{
    IPropertyDataProvider provider;

    provider = serviceProvider.CreateInstance<Notar>(); // Lansfast Hemnet Fastighetsbyran

    //var aa = ((Hemnet)provider).ParseListingsPage(new Uri("http://a"), File.ReadAllText("Hemnet_list_230221_0854.dat"));
    string? listingId = "https://www.lansfast.se/till-salu/villa/blekinge/karlshamn/svangsta/nissavagen-11/cmvilla5aba5m3qk4vosdsb/";
    ////"https://www.hemnet.se/bostad/lagenhet-2rum-masta-eskilstuna-kommun-tomtebogatan-2a-19640255";
    if (true)
    {
        var listResult = await provider.SearchProvider!.FetchSearchListings();
        WriteFile($"{provider.Id}_list_{DateTime.Now:yyMMdd_HHmm}.dat", listResult.Content);
        listingId = provider.SearchProvider!.ParseSearchListings(listResult.Source, listResult.Content).FirstOrDefault()?.ListingId;
    }

    if (false)
    {
        var itemResult = await provider.ListingProvider!.FetchListing(listingId); // "ObjektpresentationAPI/api/Visning/2804947");
        WriteFile($"{provider.Id}_item_{DateTime.Now:yyMMdd_HHmm}.dat", itemResult.Content);
    }

    //provider.ListingProvider!.ParseListing(new Uri("http://a"), ReadFile("Lansfast_item_230222_0739.dat"));

    //var item = await provider.ParseItemPage(File.ReadAllText(@"C:\Users\uzk446\Desktop\fastighetsbyran-object.html"));
    //var item = await provider.ParseListingPage(File.ReadAllText(@"C:\Users\uzk446\Desktop\svenskfastList.html"));
    //var html = await provider.LoadItemPage(new Uri("https://www.svenskfast.se/bostadsratt/stockholm/nacka/nacka/sicklaon/saltsjoqvarn/ostra-finnbodavagen-13b/355940"));
    //var html = await provider.LoadItemPage(new Uri("https://www.svenskfast.se/fritidshus/vastra-gotaland/tanum/havstenssund/havstenssund/clasegrand-3/363219"));
    //var result = provider.ParseItemPage(new Uri("https://a/b"), File.ReadAllText(@"C:\Users\uzk446\Desktop\svenskfastItem.html"));
}

string ReadFile(string filename) => File.ReadAllText(Path.Combine(storagePath, filename));
void WriteFile(string filename, string content) => File.WriteAllText(Path.Combine(storagePath, filename), content);
