namespace WalletMonitor
{
    internal class MonitorOptions
    {
        public int ChainId { get; set; }
        public string? AddressToSend { get; set; }
        public string? NodeHTTP { get; set; }
        public string? WalletPrivateKey { get; set; }
    }
}
