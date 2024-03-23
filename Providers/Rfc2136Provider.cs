using System.Net;
using System.Net.Sockets;
using ARSoft.Tools.Net;
using ARSoft.Tools.Net.Dns;
using ARSoft.Tools.Net.Dns.DynamicUpdate;

namespace OfficialSounding.DnsUpdater.Providers;

public class Rfc2136Provider : IProvider
{
    private readonly Rfc2136ProviderConfig _config;

    public Rfc2136Provider(Rfc2136ProviderConfig? config) {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    public async Task Update(string host, IPAddress addr)
    {
        var fqdn = DomainName.Parse($"{host}.{_config.Domain}");
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
            msg.TSigOptions = new TSigRecord(DomainName.Parse(_config.TsigOptions.Name), _config.TsigOptions.Algorithm, DateTime.Now, new TimeSpan(0, 5, 0), msg.TransactionID, ReturnCode.NoError, null, Convert.FromBase64String(_config.TsigOptions.Key));
        }
            
        await new DnsClient(IPAddress.Parse(_config.ServerIp), 5000).SendUpdateAsync(msg);
    }
}

public class Rfc2136ProviderConfig : ProviderConfig
{
    public required string ServerIp { get; init; }

    public int Ttl { get; set; } = 60;

    public TsigOptions? TsigOptions { get; set; }
}

public record TsigOptions(string Name, string Key, TSigAlgorithm Algorithm) {}