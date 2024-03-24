using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OfficialSounding.DnsUpdater.Providers;

public class ProviderFactory(IConfiguration configuration, ILoggerFactory loggerFactory)
{
    private readonly ILogger logger = loggerFactory.CreateLogger<ProviderFactory>();
    private readonly Dictionary<string, IProvider> providerCache = new Dictionary<string, IProvider>();

    public IProvider GetProvider(string slug)
    {
        if (providerCache.TryGetValue(slug, out var cached))
        {
            logger.LogDebug("Found provider for {slug} in cache", slug);
            return cached;
        }

        return BuildProvider(slug);
    }

    public void PrimeCache(IEnumerable<string> slugs)
    {
        foreach (var slug in slugs)
        {
            GetProvider(slug);
        }
    }

    private IProvider BuildProvider(string slug)
    {
        logger.LogDebug("Building provider for {slug}", slug);
        IProvider provider = slug switch
        {
            "rfc2136" => new Rfc2136Provider(loggerFactory.CreateLogger<Rfc2136Provider>(), configuration.GetSection("rfc2136").Get<Rfc2136ProviderConfig>()),
            "digitalOcean" => new DigitalOceanProvider(loggerFactory.CreateLogger<DigitalOceanProvider>(), configuration.GetSection("digitalOcean").Get<DigitalOceanProviderConfig>()),
            _ => throw new ArgumentException($"unknown provider slug {slug}")
        };

        logger.LogInformation("Provider for {slug} constructed successfully", slug);
        providerCache.Add(slug, provider);
        return provider;
    }
}