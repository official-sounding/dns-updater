public record Instruction(string providerSlug, string host, AddressSource addrSrc, int? ttl = null) {
 }

public enum AddressSource {
    sysv4,
    sysv6,
    ulav6,
    extv4,
    extv6
}

