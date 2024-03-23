using System.Net;

public interface IProvider {
    Task Update(string host, IPAddress addr);
}