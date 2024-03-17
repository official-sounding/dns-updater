using System.Net;
using DigitalOcean.API;
using DigitalOcean.API.Models.Requests;

public class DigitalOceanProvider : IProvider
{
    public string Slug => "digitalOcean";

    private readonly DigitalOceanConfig _config;
    private readonly IDigitalOceanClient _client;

    public DigitalOceanProvider(DigitalOceanConfig config) {
        _config = config;
        _client = new DigitalOceanClient(config.ApiKey);
    }

    public async Task Update(string host, IPAddress addr)
    {
        var recordType = addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork ? "A" : "AAAA";
        var records = await _client.DomainRecords.GetAll(_config.Domain);
        var record = records.FirstOrDefault(r => r.Name == host && r.Type.Equals(recordType, StringComparison.OrdinalIgnoreCase));

        if(record == null) {
            await _client.DomainRecords.Create(_config.Domain, new DomainRecord(){ Name = host, Type = recordType, Ttl = _config.Ttl, Data = addr.ToString() });
        } else if(addr.ToString() != record.Data) {
            await _client.DomainRecords.Update(_config.Domain, record.Id, new UpdateDomainRecord() { Data = addr.ToString() });
        }
    }
}

public class DigitalOceanConfig : ProviderConfig {
    public string ApiKey { get; set; }
    public int Ttl { get; set; } = 3600;
}