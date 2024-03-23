using Microsoft.Extensions.Configuration;
using OfficialSounding.DnsUpdater.Providers;

public class ProviderFactory {

    private readonly Dictionary<string, IProvider> providerCache = new Dictionary<string, IProvider>();
    private readonly IConfiguration configuration;

    public ProviderFactory(IConfiguration configuration) {
        this.configuration = configuration;
    }

    public IProvider GetProvider(string slug) {
        if(providerCache.TryGetValue(slug, out var cached)) {
            return cached;
        }

        return BuildProvider(slug);
    }

    private IProvider BuildProvider(string slug) {
        IProvider provider = slug switch {
            "rfc2136" => new Rfc2136Provider(configuration.Get<Rfc2136ProviderConfig>()),
            "digitalOcean" => new DigitalOceanProvider(configuration.Get<DigitalOceanProviderConfig>()),
            _ => throw new ArgumentException($"unknown provider slug {slug}")
        };

        providerCache.Add(slug, provider);
        return provider;
    }
}