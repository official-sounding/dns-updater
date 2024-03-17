public record Instruction(AddressSource addrSrc, string providerSlug, int? ttl = null) {}

public enum AddressSource {
    sysv4,
    sysv6,
    ulav6,
    extv4,
    extv6
}

