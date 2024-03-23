using System.Diagnostics;
using System.Net;
using System.Net.Sockets;


public static class SocketBuilder
{
    
}
public class AddressWrapper
{

    private readonly HttpClient _httpV4 = new HttpClient(new SocketsHttpHandler() { ConnectCallback = BuildCallback(AddressFamily.InterNetwork)});
    private readonly HttpClient _httpV6 = new HttpClient(new SocketsHttpHandler() { ConnectCallback = BuildCallback(AddressFamily.InterNetworkV6)});

    public async Task<IPAddress> GetAddress(AddressSource src)
    {
        return src switch
        {
            AddressSource.sysv4 => GetInternalAddress(AddressFamily.InterNetwork),
            AddressSource.sysv6 => GetInternalAddress(AddressFamily.InterNetworkV6),
            AddressSource.extv4 => await GetExternalAddress(_httpV4),
            AddressSource.extv6 => await GetExternalAddress(_httpV6),
            _ => throw new ArgumentException($"unknown address type {src}")
        };
    }

    private IPAddress GetInternalAddress(AddressFamily family)
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        return host.AddressList.First(addr => addr.AddressFamily == family);
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