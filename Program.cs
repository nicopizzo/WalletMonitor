using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using WalletMonitor;

var config = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .AddJsonFile("appsettings.json", false)
    .AddUserSecrets<Program>()
    .Build();

var pk = config.GetSection("MonitorOptions:WalletPrivateKey").Value;
var chainid = config.GetSection("MonitorOptions:ChainId").Value;
var node = config.GetSection("MonitorOptions:NodeHTTP").Value;

var account = new Account(pk, int.Parse(chainid));

var serviceProvider = new ServiceCollection()
    .AddLogging(f => f.AddConsole())
    .AddOptions()
    .Configure<MonitorOptions>(config.GetSection("MonitorOptions"))
    .AddSingleton<IWeb3>(f => new Web3(account, node))
    .AddSingleton<IEthMonitor, EthMonitor>()
    .BuildServiceProvider();

var monitor = serviceProvider.GetRequiredService<IEthMonitor>();
await monitor.StartWatchingAsync();