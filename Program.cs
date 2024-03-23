using System.Globalization;
using CsvHelper;
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables(prefix: "dnsBuilder_")
                .AddJsonFile("appsettings.json")
                .Build();

var providerFactory = new ProviderFactory(configuration);
var updater = new DnsUpdater(providerFactory, new AddressWrapper());

using var stream = new StreamReader("instructions.csv");
using var csvReader = new CsvReader(stream, CultureInfo.InvariantCulture);

var records = new List<Instruction>();
var providerSet = new HashSet<string>();
await foreach(var record in csvReader.GetRecordsAsync<Instruction>()) {
    records.Add(record);
    providerSet.Add(record.providerSlug);
}

// pre-build providers to ensure that configuration is valid
// before actually running instructions
foreach(var slug in providerSet) {
    providerFactory.GetProvider(slug);
}

await updater.PerformUpdates(records);