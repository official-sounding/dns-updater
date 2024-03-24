using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OfficialSounding.DnsUpdater.Providers;

public class ProviderFactory(ILogger<ProviderFactory> logger, IServiceProvider serviceProvider)
{
    public IProvider GetProvider(string slug)
    {
        return serviceProvider.GetKeyedService<IProvider>(slug) ?? throw new ArgumentException($"no provider with slug {slug}");
    }

    public void PrimeCache(IEnumerable<string> slugs)
    {
        foreach (var slug in slugs)
        {
            logger.LogDebug("Building Provider for {slug}", slug);
            GetProvider(slug);
        }
    }
}