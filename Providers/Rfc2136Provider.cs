using System.Net;
using System.Net.Sockets;
using ARSoft.Tools.Net;
using ARSoft.Tools.Net.Dns;
using ARSoft.Tools.Net.Dns.DynamicUpdate;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OfficialSounding.DnsUpdater.Providers;

public class Rfc2136Provider : IProvider
{
    private readonly ILogger _logger;
    private readonly Rfc2136ProviderConfig _config;

    public Rfc2136Provider(ILogger<Rfc2136Provider> logger, IOptions<Rfc2136ProviderConfig> config) {
        _logger = logger;
        _config = config?.Value ?? throw new ArgumentNullException(nameof(config));
    }

    public async Task Update(string host, IPAddress addr)
    {
        
        var fqdn = DomainName.Parse($"{host}.{_config.Domain}");
        _logger.LogDebug("Attempting RFC 2136 update of {fqdn} to {addr}", fqdn, addr);
        _logger.LogDebug("Using config: {config}", _config);
        DnsUpdateMessage msg = new DnsUpdateMessage
        {
            ZoneName = DomainName.Parse(_config.Domain)
        };

        switch (addr.AddressFamily)
        {
            case AddressFamily.InterNetwork:
                msg.Updates.Add(new DeleteAllRecordsUpdate(fqdn, RecordType.A));
                msg.Updates.Add(new AddRecordUpdate(new ARecord(fqdn, _config.Ttl, addr)));
                break;
            case AddressFamily.InterNetworkV6:
                msg.Updates.Add(new DeleteAllRecordsUpdate(fqdn, RecordType.Aaaa));
                msg.Updates.Add(new AddRecordUpdate(new AaaaRecord(fqdn, _config.Ttl, addr)));
                break;

            default:
                throw new InvalidOperationException($"Cannot perform an update with {addr.AddressFamily}");
        }

        if(_config.TsigOptions != null) {
            _logger.LogDebug("TSIG options found, using key named {name} and algorithm {algo}", _config.TsigOptions.Name, _config.TsigOptions.Algorithm);
            msg.TSigOptions = new TSigRecord(DomainName.Parse(_config.TsigOptions.Name), _config.TsigOptions.Algorithm, DateTime.Now, new TimeSpan(0, 5, 0), msg.TransactionID, ReturnCode.NoError, null, Convert.FromBase64String(_config.TsigOptions.Key));
        }
            
        var result = await new DnsClient(IPAddress.Parse(_config.ServerIp), 5000).SendUpdateAsync(msg);
        _logger.LogInformation("Result from DNS Server after RFC 2136 update: {returnCode}", result?.ReturnCode);
    }
}

public class Rfc2136ProviderConfig : ProviderConfig
{
    public required string ServerIp { get; init; }

    public int Ttl { get; set; } = 60;

    public TsigOptions? TsigOptions { get; set; }

    public override string ToString() => $"Domain: {Domain}, ServerIp: {ServerIp}, Ttl: {Ttl}, TSig: {TsigOptions != null}";
}

public record TsigOptions(string Name, string Key, TSigAlgorithm Algorithm) {}