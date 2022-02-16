using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nethereum.Util;
using Nethereum.Web3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace WalletMonitor
{
    internal interface IEthMonitor
    {
        Task StartWatchingAsync();
    }

    internal class EthMonitor : IEthMonitor
    {
        private readonly IWeb3 _Web3;
        private readonly ILogger _Logger;
        private readonly MonitorOptions _Options;

        public EthMonitor(IWeb3 web3, ILogger<EthMonitor> logger, IOptions<MonitorOptions> options)
        {
            _Web3 = web3;
            _Logger = logger;
            _Options = options.Value;
        }

        public async Task StartWatchingAsync()
        {
            var account = _Web3.Eth.TransactionManager.Account.Address;
            BigInteger ethBalance = await CheckAccountBalance(account);
            while (true)
            {
                try
                {
                    var currentBal = await CheckAccountBalance(account);

                    if (currentBal > ethBalance)
                    {
                        _Logger.LogInformation("Found new funds...Transfering");
                        await TransferFunds(account);
                        ethBalance = await CheckAccountBalance(account);
                    }              
                }
                catch(Exception ex)
                {
                    _Logger.LogError(ex, "Something failed");
                }
                await Task.Delay(5000);
            }
        }

        private async Task<BigInteger> CheckAccountBalance(string account)
        {
            var bal = await _Web3.Eth.GetBalance.SendRequestAsync(account);
            return bal.Value;
        }

        private async Task TransferFunds(string from)
        {
            var gasPriceWei = await _Web3.Eth.GasPrice.SendRequestAsync();
            var gasPriceGwei = UnitConversion.Convert.FromWei(gasPriceWei, UnitConversion.EthUnit.Gwei);

            var service = _Web3.Eth.GetEtherTransferService();
            
            var total = await service.CalculateTotalAmountToTransferWholeBalanceInEtherAsync(from, gasPriceGwei);
            var transaction = await service.TransferEtherAndWaitForReceiptAsync(_Options.AddressToSend, total, gasPriceGwei);
            _Logger.LogInformation($"Transfered {transaction.TransactionHash}");
        }
    }
}
