# DNS Updater
A dynamic DNS program that can perform multiple updates to different providers.  Supports both IPv4 & IPv6 addresses, both external and internal.

# Implemented Providers

* **RFC 2136**: [RFC 2136](https://www.rfc-editor.org/rfc/rfc2136.txt) DNS Updates for A & AAAA records. Supports TSIG keys.
* **DigitalOcean**: DO API DNS updates of A & AAAA records. Requires a Digital Ocean key with read & write access to the domain in question.

# How to use

Releases are published with single-file, non-self-contained (aka runtime-dependent) executables for three Runtimes:

* win-x64
* linux-x64
* linux-musl-x64

With the executable + a dotnet 8.0.x runtime, use the following steps to run the program

Rename `providers.example.json` to `providers.json`, and populate the relevant sections for the providers you wish to use.  Alternatively, all provider configuration can be supplied by environment variables, prefixed with `dnsBuilder_`
Rename `instructions-example.csv` to `instructions.csv`, and populate the instructions with provider, host & address source information.

## Address Sources

Note: in the event multiple IP addresses match the criteria (eg on Linux, if both a stable & temporary IPv6 address are assigned) the first found one matches

* **sysv4**: the specified interface's IPv4 address
* **pubv6**: the specified interface's public IPv6 address
* **llv6**: the specified interface's link-local IPv6 address (eg an address in the `fe80::/10` range)
* **ulv6**: the specified interface's unique-local IPv6 address (eg an address in the `fc00::/7` range)
* **extv4**: use the external IPv4 address (via https://ifconfig.me)
* **extv6**: use the external IPv6 address (via https://ifconfig.me)