using System.Net;
using DigitalOcean.API;
using DigitalOcean.API.Models.Requests;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OfficialSounding.DnsUpdater.Providers;
public class DigitalOceanProvider : IProvider
{
    private readonly ILogger _logger;
    private readonly DigitalOceanProviderConfig _config;
    private readonly IDigitalOceanClient _client;

    public DigitalOceanProvider(ILogger<DigitalOceanProvider> logger, IOptions<DigitalOceanProviderConfig> config) {
        _logger = logger;
        _config = config?.Value ?? throw new ArgumentNullException(nameof(config));
        _client = new DigitalOceanClient(_config.ApiKey);
    }

    public async Task Update(string host, IPAddress addr)
    {
        _logger.LogDebug("Checking for DO API records for host {host} in domain {domain} (supplied address {addr})", host, _config.Domain, addr);
        var recordType = addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork ? "A" : "AAAA";
        
        var records = await _client.DomainRecords.GetAll(_config.Domain);
        _logger.LogDebug("Found {count} records for domain", records.Count);
        var record = records.FirstOrDefault(r => r.Name == host && r.Type.Equals(recordType, StringComparison.OrdinalIgnoreCase));


        if(record == null) {
            _logger.LogInformation("No DO API response for {recordType} record for host {host} in domain {domain}, creating...", recordType, host, _config.Domain);
            await _client.DomainRecords.Create(_config.Domain, new DomainRecord(){ Name = host, Type = recordType, Ttl = _config.Ttl, Data = addr.ToString() });
        } else if(addr.ToString() != record.Data) {
            _logger.LogInformation("DO API response for {recordType} record for host {host} in domain {domain} found, updating...", recordType, host, _config.Domain);
            _logger.LogDebug("Record ID: {recordId}, Data: {data}", record.Id, record.Data);
            await _client.DomainRecords.Update(_config.Domain, record.Id, new UpdateDomainRecord() { Data = addr.ToString() });
        } else {
            _logger.LogInformation("Records in DO API are identitcal, no changes made");
        }
    }
}

public class DigitalOceanProviderConfig : ProviderConfig {
    public required string ApiKey { get; set; }
    public int Ttl { get; set; } = 3600;
}