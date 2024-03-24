# DNS Updater
A dynamic DNS program that can perform multiple updates to different providers.  Supports both IPv4 & IPv6 addresses, both external and internal.

# Implemented Providers

* **RFC 2136**: Performs [RFC 2136](https://www.rfc-editor.org/rfc/rfc2136.txt) Dynamic DNS Updates for A & AAAA records
* **DigitalOcean**: Given a digital ocean key with read & write access, can perform create & updates of A & AAAA records


# How to use

Rename `providers.example.json` to `providers.json`, and populate the relevant sections for the providers you wish to use.
Rename `instructions-example.csv` to `instructions.csv`, and populate the instructions with provider, host & address source information.

## Address Sources

Note: in the event multiple IP addresses match the criteria (eg on Linux, if both a stable & temporary IPv6 address are assigned) the first found one matches

* **sysv4**: the specified interface's IPv4 address
* **pubv6**: the specified interface's public IPv6 address
* **llv6**: the specified interface's link-local IPv6 address (eg an address in the `fe80::/10` range)
* **ulv6**: the specified interface's unique-local IPv6 address (eg an address in the `fc00::/7` range)
* **extv4**: use the external IPv4 address (via https://ifconfig.me)
* **extv6**: use the external IPv6 address (via https://ifconfig.me)