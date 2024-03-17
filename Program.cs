using System.Net;
using System.Net.Sockets;

var host = Dns.GetHostEntry(Dns.GetHostName());
foreach (var ip in host.AddressList)
{
        Console.Write($"{ip.AddressFamily}\t");
        if(ip.AddressFamily == AddressFamily.InterNetworkV6) {
            Console.WriteLine($"{ip.ScopeId}\t");
        }
        Console.WriteLine(ip.ToString());
}

HttpClient _http = new HttpClient(new SocketsHttpHandler() {
    ConnectCallback = async (context, cancellationToken) => {
        // Use DNS to look up the IP address(es) of the target host
        IPHostEntry ipHostEntry = await Dns.GetHostEntryAsync(context.DnsEndPoint.Host);

        // Filter for IPv4 addresses only
        IPAddress? ipAddress = ipHostEntry
            .AddressList
            .FirstOrDefault(i => i.AddressFamily == AddressFamily.InterNetwork);

        // Fail the connection if there aren't any IPV4 addresses
        if (ipAddress == null) {
            throw new Exception($"No IP4 address for {context.DnsEndPoint.Host}");
        }

        // Open the connection to the target host/port
        TcpClient tcp = new();
        await tcp.ConnectAsync(ipAddress, context.DnsEndPoint.Port, cancellationToken);

        // Return the NetworkStream to the caller
        return tcp.GetStream();
    }
});

var extIp = await _http.GetStringAsync("http://ifconfig.me");

Console.WriteLine(extIp);