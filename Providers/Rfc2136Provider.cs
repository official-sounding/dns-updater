using System.Net;
using System.Net.Sockets;
using ARSoft.Tools.Net;
using ARSoft.Tools.Net.Dns;
using ARSoft.Tools.Net.Dns.DynamicUpdate;

namespace OfficialSounding.DnsUpdater.Providers;

public class Rfc2136Provider : IProvider
{
    public string Slug => "rfc2136";

    private readonly Rfc2136ProviderConfig _config;

    public Rfc2136Provider(Rfc2136ProviderConfig config) {
        _config = config;
    }

    public async Task Update(string host, IPAddress addr)
    {
        DnsUpdateMessage msg = new DnsUpdateMessage
        {
            ZoneName = DomainName.Parse(_config.Domain)
        };

        switch (addr.AddressFamily)
        {
            case AddressFamily.InterNetwork:
                msg.Updates.Add(new DeleteAllRecordsUpdate(DomainName.Parse("dyn.example.com"), RecordType.A));
                msg.Updates.Add(new AddRecordUpdate(new ARecord(DomainName.Parse("dyn.example.com"), 60, addr)));
                break;
            case AddressFamily.InterNetworkV6:
                msg.Updates.Add(new DeleteAllRecordsUpdate(DomainName.Parse("dyn.example.com"), RecordType.Aaaa));
                msg.Updates.Add(new AddRecordUpdate(new AaaaRecord(DomainName.Parse("dyn.example.com"), 60, addr)));
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
    public string ServerIp { get; set; }

    public int Ttl { get; set; } = 60;

    public TsigOptions? TsigOptions { get; set; }
}

public class TsigOptions {
    public string Name { get; set; }
    public string Key { get; set; }
    public TSigAlgorithm Algorithm { get; set; }
}