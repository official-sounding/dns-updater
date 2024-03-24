using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var path = Directory.GetCurrentDirectory();

IConfiguration configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables(prefix: "dnsBuilder_")
                .AddJsonFile(Path.Combine(path, "providers.json"))
                .Build();

var services = new ServiceCollection();

services.AddLogging(config => config.AddConsole().SetMinimumLevel(LogLevel.Trace));
services.AddSingleton(provider => configuration);
services.AddSingleton<ProviderFactory>();
services.AddSingleton<AddressWrapper>();
services.AddSingleton<DnsUpdater>();

var sp = services.BuildServiceProvider();

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