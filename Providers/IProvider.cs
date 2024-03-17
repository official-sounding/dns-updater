using System.Net;

public interface IProvider {
    string Slug { get; }
    Task Update(string host, IPAddress addr);
}