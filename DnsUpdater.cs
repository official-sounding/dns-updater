using System.Globalization;
using CsvHelper;
using Microsoft.Extensions.Logging;

public class DnsUpdater
{
    private readonly ProviderFactory providerFactory;
    private readonly AddressWrapper addrWrapper;
    private readonly ILogger logger;
    public DnsUpdater(ILogger<DnsUpdater> logger, ProviderFactory providerFactory, AddressWrapper addrWrapper)
    {
        this.logger = logger;
        this.providerFactory = providerFactory;
        this.addrWrapper = addrWrapper;
    }

    public async Task<IEnumerable<Instruction>> ReadInstructions(string filename)
    {
        logger.LogInformation("loading instructions from {filename}", filename);
        using var stream = new StreamReader(filename);
        using var csvReader = new CsvReader(stream, CultureInfo.InvariantCulture);

        var records = new List<Instruction>();
        var providerSet = new HashSet<string>();
        await foreach (var record in csvReader.GetRecordsAsync<Instruction>())
        {
            records.Add(record);
            providerSet.Add(record.providerSlug);
        }

        logger.LogInformation("found {count} instructions", records.Count);
        logger.LogInformation("found {count} distinct provider slugs", providerSet.Count);

        providerFactory.PrimeCache(providerSet);
        return records;
    }


    public async Task PerformUpdates(IEnumerable<Instruction> instructions)
    {
        foreach (var instruction in instructions)
        {
            logger.LogDebug("Starting {instruction}", instruction);
            var addr = await addrWrapper.GetAddress(instruction.addrSrc, instruction.ifName);
            await providerFactory.GetProvider(instruction.providerSlug).Update(instruction.host, addr);
            logger.LogDebug("Completing {instruction}", instruction);
        }
    }
}