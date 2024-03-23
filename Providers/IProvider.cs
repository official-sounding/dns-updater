using System.Net;
namespace OfficialSounding.DnsUpdater.Providers;

public interface IProvider {
    Task Update(string host, IPAddress addr);
}