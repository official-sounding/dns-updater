using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

public class AddressWrapper(ILogger<AddressWrapper> logger)
{
    private readonly HttpClient _httpV4 = new HttpClient(new SocketsHttpHandler() { ConnectCallback = BuildCallback(AddressFamily.InterNetwork) });
    private readonly HttpClient _httpV6 = new HttpClient(new SocketsHttpHandler() { ConnectCallback = BuildCallback(AddressFamily.InterNetworkV6) });

    public async Task<IPAddress> GetAddress(AddressSource src, string? ifName)
    {
        if (!Enum.IsDefined(typeof(AddressSource), src))
        {
            throw new ArgumentException($"{src} is not a valid AddressSource");
        }

        if (src == AddressSource.extv4)
        {
            return await GetExternalAddress(_httpV4);
        }
        else if (src == AddressSource.extv6)
        {
            return await GetExternalAddress(_httpV6);
        }

        return GetInternalAddress(src, ifName);
    }

    private IPAddress GetInternalAddress(AddressSource src, string? ifName)
    {
        if (ifName is null)
        {
            throw new ArgumentException($"ifName cannot be null for {src}");
        }

        var iface = InterfaceByName(ifName) ?? throw new ArgumentException($"{ifName} does not correspond to a network interface on this device");
        var addrs = iface.GetIPProperties().UnicastAddresses;

        logger.LogTrace("for interface {ifName}, found {count} possible addresses", ifName, addrs.Count);

        var addr = addrs
            .Select(u => u.Address)
            .FirstOrDefault(addr =>
            src switch
            {
                AddressSource.sysv4 => addr.AddressFamily == AddressFamily.InterNetwork,
                AddressSource.pubv6 => addr.AddressFamily == AddressFamily.InterNetworkV6 && !addr.IsIPv6UniqueLocal && !addr.IsIPv6SiteLocal && !addr.IsIPv6LinkLocal,
                AddressSource.ulav6 => addr.AddressFamily == AddressFamily.InterNetworkV6 && addr.IsIPv6UniqueLocal,
                AddressSource.llv6 => addr.AddressFamily == AddressFamily.InterNetworkV6 && addr.IsIPv6LinkLocal,
                _ => false
            }
        ) ?? throw new ArgumentException($"{ifName} does not have a valid {src} address");

        logger.LogDebug("for source {src} and interface {ifName} found address {addr}", src, ifName, addr);

        return addr;
    }

    private NetworkInterface? InterfaceByName(string name)
    {
        var interfaces = NetworkInterface.GetAllNetworkInterfaces();
        logger.LogTrace("Found {count} possible network interfaces", interfaces.Length);
        return NetworkInterface.GetAllNetworkInterfaces().FirstOrDefault(iface => iface.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    private async Task<IPAddress> GetExternalAddress(HttpClient client)
    {
        var addrStr = await client.GetStringAsync("http://ifconfig.me");
        return IPAddress.Parse(addrStr);
    }

    private static Func<SocketsHttpConnectionContext, CancellationToken, ValueTask<Stream>> BuildCallback(AddressFamily family)
    {
        return async (context, cancellationToken) =>
        {
            // Use DNS to look up the IP address(es) of the target host
            IPHostEntry ipHostEntry = await Dns.GetHostEntryAsync(context.DnsEndPoint.Host);

            // Filter for IPv4 addresses only
            IPAddress? ipAddress = ipHostEntry
                .AddressList
                .FirstOrDefault(i => i.AddressFamily == family);

            // Fail the connection if there aren't any IPV4 addresses
            if (ipAddress == null)
            {
                throw new Exception($"No {family} address for {context.DnsEndPoint.Host}");
            }

            // Open the connection to the target host/port
            TcpClient tcp = new();
            await tcp.ConnectAsync(ipAddress, context.DnsEndPoint.Port, cancellationToken);
 
            // Return the NetworkStream to the caller
            return tcp.GetStream();
        };
    }
}