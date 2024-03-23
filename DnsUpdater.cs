public class DnsUpdater {
    private readonly ProviderFactory providerFactory;
    private readonly AddressWrapper addrWrapper;
    public DnsUpdater(ProviderFactory providerFactory, AddressWrapper addrWrapper) {
        this.providerFactory = providerFactory;
        this.addrWrapper = addrWrapper;
    }

    public async Task PerformUpdates(IEnumerable<Instruction> instructions) {
        foreach(var instruction in instructions) {
            var addr = await addrWrapper.GetAddress(instruction.addrSrc);
            await providerFactory.GetProvider(instruction.providerSlug).Update(instruction.host, addr);
        }
    }
}