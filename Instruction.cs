public record Instruction(string providerSlug, string host, AddressSource addrSrc, string ifName, int? ttl = null) {
 }

public enum AddressSource {
    sysv4,
    pubv6,
    llv6,
    ulav6,
    extv4,
    extv6
}

