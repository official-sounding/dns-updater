using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OfficialSounding.DnsUpdater.Providers;

var path = Directory.GetCurrentDirectory();

IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(path)
                .AddJsonFile("providers.json", optional: true)
                .AddEnvironmentVariables(prefix: "dnsBuilder_")
                .Build();

var services = new ServiceCollection();

services.AddLogging(config => config.AddConsole().SetMinimumLevel(LogLevel.Information));
services.AddSingleton(provider => configuration);
services.AddSingleton<ProviderFactory>();
services.AddSingleton<AddressWrapper>();
services.AddSingleton<DnsUpdater>();

services.Configure<DigitalOceanProviderConfig>(configuration.GetSection("digitalOcean"));
services.Configure<Rfc2136ProviderConfig>(configuration.GetSection("rfc2136"));

services.AddKeyedSingleton<IProvider, Rfc2136Provider>("rfc2136");
services.AddKeyedSingleton<IProvider, DigitalOceanProvider>("digitalOcean");

var sp = services.BuildServiceProvider();

var options = sp.GetService<IOptions<Rfc2136ProviderConfig>>();

var updater = sp.GetRequiredService<DnsUpdater>();
var logger = sp.GetRequiredService<ILogger<Program>>();

try
{
    var instructions = await updater.ReadInstructions(Path.Combine(path, "instructions.csv"));
    await updater.PerformUpdates(instructions);
}
catch (Exception e)
{
    logger.LogError(e, "Caught Exception, exiting");
}