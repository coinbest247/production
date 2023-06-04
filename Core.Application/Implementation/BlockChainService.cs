using Core.Application.Interfaces;
using Nethereum.Web3;
using Nethereum.RPC.Eth.DTOs;
using System;
using System.Threading.Tasks;
using System.Web;
using System.Net;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Core.Application.ViewModels.BlockChain;
using Nethereum.Web3.Accounts;
using Nethereum.Util;
using Core.Infrastructure.Interfaces;
using Nethereum.Hex.HexConvertors.Extensions;
using System.Numerics;
using Core.Utilities.Constants;
using Microsoft.AspNetCore.Identity;
using Core.Data.Entities;
using Microsoft.Extensions.Logging;
using CoinMarketCapPro.Types;
using CoinMarketCapPro;
using System.Collections.Generic;
using System.Linq;

namespace Core.Application.Implementation
{
    public class BlockChainService : IBlockChainService
    {
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;
        private readonly AddressUtil _addressUtil = new AddressUtil();
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<BlockChainService> _logger;

        public BlockChainService(
            ILogger<BlockChainService> logger,
            IConfiguration configuration,
            UserManager<AppUser> userManager,
            IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _userManager = userManager;
            _configuration = configuration;
            _unitOfWork = unitOfWork;
        }

        public Account CreateAccount()
        {
            var ecKey = Nethereum.Signer.EthECKey.GenerateKey();
            var privateKey = ecKey.GetPrivateKeyAsBytes().ToHex();
            var account = new Account(privateKey, CommonConstants.ChainId);
            return account;
        }

        public CoinMarKetCapInfoViewModel GetListingLatest(int startIndex,
            int limitItem, string unitConvertTo)
        {
            CoinMarKetCapInfoViewModel coinMarKetCapInfo = null;

            var queryString = HttpUtility.ParseQueryString(string.Empty);
            queryString["start"] = startIndex.ToString(); // "1"
            queryString["limit"] = limitItem.ToString();// "2"
            queryString["convert"] = unitConvertTo;// "USD";

            string urlCryptoCurrencyListingLatest = _configuration["BlockChain:CoinmarKetCapApi:CMC_PRO_API_URL:CryptoCurrency:Listings_Latest"];
            var URL = new UriBuilder(urlCryptoCurrencyListingLatest);
            URL.Query = queryString.ToString();

            var client = new WebClient();
            var cmcProApiKey = _configuration["BlockChain:CoinmarKetCapApi:CMC_PRO_API_KEY"];
            client.Headers.Add("X-CMC_PRO_API_KEY", cmcProApiKey);
            client.Headers.Add("Accepts", "application/json");
            var coinInfoString = client.DownloadString(URL.ToString());

            coinMarKetCapInfo = JsonConvert.DeserializeObject<CoinMarKetCapInfoViewModel>(coinInfoString);

            return coinMarKetCapInfo;
        }

        public decimal GetTokenPriceByTokenCode(string symbol, string unitConvertTo)
        {
            try
            {
                CoinMarKetCapPriceViewModel coinMarKetCapInfo = null;

                var queryString = HttpUtility.ParseQueryString(string.Empty);

                queryString["convert"] = unitConvertTo;// "USD";
                queryString["symbol"] = symbol;// "USD";

                string urlCryptoCurrencyPricePerformance = _configuration["BlockChain:CoinmarKetCapApi:CMC_PRO_API_URL:CryptoCurrency:OHLCV_Latest"];

                var URL = new UriBuilder(urlCryptoCurrencyPricePerformance);
                URL.Query = queryString.ToString();

                var client = new WebClient();
                var cmcProApiKey = _configuration["BlockChain:CoinmarKetCapApi:CMC_PRO_API_KEY"];
                client.Headers.Add("X-CMC_PRO_API_KEY", cmcProApiKey);
                client.Headers.Add("Accepts", "application/json");
                var content = client.DownloadString(URL.ToString());

                content = content.Replace(symbol, "tokenData");

                content = content.Replace(unitConvertTo, "CurrenyPriceInfo");

                coinMarKetCapInfo = JsonConvert.DeserializeObject<CoinMarKetCapPriceViewModel>(content);

                if (coinMarKetCapInfo != null)
                    return Convert.ToDecimal(coinMarKetCapInfo.data.tokenData[0].quote.CurrenyPriceInfo.price ?? 0);

                return 0;
            }
            catch (Exception ex)
            {

                return 0;
            }
        }

        public async Task<decimal> GetTokenPriceByTokenId(int id, string unitConvertTo)
        {
            try
            {
                var cmcProApiKey = _configuration["BlockChain:CoinmarKetCapApi:CMC_PRO_API_KEY"];

                var apikey = cmcProApiKey; // <-- Your API Key here   
                                 //var apikey = System.IO.File.ReadAllLines($"{Environment.CurrentDirectory}\\ApiKey.txt")[0];

                var clients = new CoinMarketCapClient(ApiSchema.Pro, apikey);

                var tokenInfo = await clients.GetCurrencyMarketQuotesAsync(new List<int> { id }, unitConvertTo);

                if (tokenInfo !=null)
                {
                    return (decimal)tokenInfo.Data.Values.First().Quote.Values.Last().Price;
                }

                return 0;
            }
            catch (Exception ex)
            {

                return 0;
            }
        }

        public decimal GetCurrentPrice(string unit, string unitConverto)
        {
            return GetTokenPriceByTokenCode(unit, unitConverto);
        }

        public async Task<decimal> GetPiNetworkPrice()
        {
            return await GetTokenPriceByTokenId(16193, "USD");
        }
        

        public decimal GetCurrentPriceByTokenCode(string unit, string unitConverto)
        {
            var coinMarKetCapInfo = GetTokenPriceByTokenCode(unit, unitConverto);
            return coinMarKetCapInfo;
        }

        public async Task<Transaction> GetTransactionByTransactionID(
            string transactionID, string urlRPC)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12
               | SecurityProtocolType.Tls11
               | SecurityProtocolType.Tls
               | SecurityProtocolType.SystemDefault;

            var web3 = new Web3(urlRPC);

            Transaction transactionInfo = await web3.Eth.Transactions
                .GetTransactionByHash.SendRequestAsync(transactionID).ConfigureAwait(true);

            return transactionInfo;
        }

        public async Task<TransactionReceipt> GetTransactionReceiptByTransactionID(
           string transactionID, string urlRPC)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12
               | SecurityProtocolType.Tls11
               | SecurityProtocolType.Tls
               | SecurityProtocolType.SystemDefault;

            var web3 = new Web3(urlRPC);

            var transactionInfo = await web3.Eth.Transactions
                .GetTransactionReceipt.SendRequestAsync(transactionID).ConfigureAwait(true);

            return transactionInfo;
        }

        public async Task<string> SendTokenAsync(string privateKeyERC20, string receiverAddress,
            string contractAddress, decimal tokenAmount, int decimalPlaces, string urlRPC)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12
               | SecurityProtocolType.Tls11
               | SecurityProtocolType.Tls
               | SecurityProtocolType.SystemDefault;


            var account = new Account(privateKeyERC20, CommonConstants.ChainId);

            var web3 = new Web3(account, urlRPC);

            web3.TransactionManager.UseLegacyAsDefault = true;

            var transferHandler = web3.Eth.GetContractTransactionHandler<TransferFunctionViewModel>();
            var transferFunction = new TransferFunctionViewModel()
            {
                To = receiverAddress,
                FromAddress = account.Address,
                TokenAmount = Web3.Convert.ToWei(tokenAmount, decimalPlaces),
            };

            var transactionReceipt = string.Empty;

            try
            {
                transactionReceipt = await transferHandler
                    .SendRequestAsync(contractAddress, transferFunction).ConfigureAwait(true);

            }
            catch (Exception ex)
            {
                _logger.LogError($"BlockChainService_SendTokenAsync_{receiverAddress}: {ex}");
            }

            return transactionReceipt;
        }

        public async Task<TransactionReceipt> SendERC20Async(string privateKeyERC20, string receiverAddress,
            string contractAddress, decimal tokenAmount, int decimalPlaces, string urlRPC)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12
               | SecurityProtocolType.Tls11
               | SecurityProtocolType.Tls
               | SecurityProtocolType.SystemDefault;

            var account = new Account(privateKeyERC20, CommonConstants.ChainId);

            var web3 = new Web3(account, urlRPC);

            web3.TransactionManager.UseLegacyAsDefault = true;

            var transferHandler = web3.Eth.GetContractTransactionHandler<TransferFunctionViewModel>();
            var transferFunction = new TransferFunctionViewModel()
            {
                To = receiverAddress,
                FromAddress = account.Address,
                TokenAmount = Web3.Convert.ToWei(tokenAmount, decimalPlaces),
            };

            var estimate = await transferHandler.EstimateGasAsync(contractAddress, transferFunction);

            transferFunction.Gas = estimate.GetValue();

            transferFunction.Gas += 5000;

            transferFunction.GasPrice = new BigInteger(7000000000);

            var transactionReceipt = new TransactionReceipt();

            try
            {
                transactionReceipt = await transferHandler
                .SendRequestAndWaitForReceiptAsync(contractAddress, transferFunction)
                .ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                _logger.LogError($"BlockChainService_SendERC20Async_{receiverAddress}: {ex}");
            }

            return transactionReceipt;
        }

        public async Task<string> SendERC20WithoutReceiptAsync(string privateKeyERC20, string receiverAddress,
            string contractAddress, decimal tokenAmount, int decimalPlaces, string urlRPC)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12
               | SecurityProtocolType.Tls11
               | SecurityProtocolType.Tls
               | SecurityProtocolType.SystemDefault;

            var account = new Account(privateKeyERC20, CommonConstants.ChainId);

            var web3 = new Web3(account, urlRPC);

            web3.TransactionManager.UseLegacyAsDefault = true;

            var transferHandler = web3.Eth.GetContractTransactionHandler<TransferFunctionViewModel>();
            var transferFunction = new TransferFunctionViewModel()
            {
                To = receiverAddress,
                FromAddress = account.Address,
                TokenAmount = Web3.Convert.ToWei(tokenAmount, decimalPlaces),
            };

            var transactionReceipt = string.Empty;

            try
            {
                transactionReceipt = await transferHandler.SendRequestAsync(contractAddress, transferFunction);
            }
            catch (Exception ex)
            {
                _logger.LogError($"BlockChainService_SendERC20WithoutReceiptAsync_{receiverAddress}: {ex}");
            }

            return transactionReceipt;
        }

        public async Task<TransactionReceipt> SendEthAsync(string privateKey,
            string receiverAddress, decimal tokenAmount, string urlRPC)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12
                | SecurityProtocolType.Tls11
                | SecurityProtocolType.Tls
                | SecurityProtocolType.SystemDefault;

            var account = new Account(privateKey, CommonConstants.ChainId);

            var web3 = new Web3(account, urlRPC);

            web3.TransactionManager.UseLegacyAsDefault = true;

            var transactionReceipt = new TransactionReceipt();

            try
            {
                transactionReceipt = await web3.Eth.GetEtherTransferService()
                       .TransferEtherAndWaitForReceiptAsync(receiverAddress, tokenAmount);
            }
            catch (Exception ex)
            {
                _logger.LogError($"BlockChainService_SendEthAsync_{receiverAddress}: {ex}");
            }

            return transactionReceipt;
        }

        public async Task<string> SendEthWithoutReceiptAsync(string privateKey,
            string receiverAddress, decimal tokenAmount, string urlRPC)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12
                | SecurityProtocolType.Tls11
                | SecurityProtocolType.Tls
                | SecurityProtocolType.SystemDefault;

            var account = new Account(privateKey, CommonConstants.ChainId);

            var web3 = new Web3(account, urlRPC);

            web3.TransactionManager.UseLegacyAsDefault = true;

            var transaction = string.Empty;

            try
            {
                transaction = await web3.Eth.GetEtherTransferService()
                   .TransferEtherAsync(receiverAddress, tokenAmount);
            }
            catch (Exception ex)
            {
                _logger.LogError($"BlockChainService_SendEthWithoutReceiptAsync_{receiverAddress}: {ex}");
            }

            return transaction;
        }

        public async Task<decimal> GetERC20Balance(string owner,
            string smartContractAddress, int decimalPlaces, string urlRPC)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12
                | SecurityProtocolType.Tls11
                | SecurityProtocolType.Tls
                | SecurityProtocolType.SystemDefault;

            var web3 = new Web3(urlRPC);

            var balanceOfMessage = new BalanceOfFunction() { Owner = owner };

            var queryHandler = web3.Eth.GetContractQueryHandler<BalanceOfFunction>();

            var balance = await queryHandler
                .QueryAsync<BigInteger>(smartContractAddress, balanceOfMessage)
                .ConfigureAwait(true);

            decimal balanceUsdt = Web3.Convert.FromWei(balance, decimalPlaces);

            return balanceUsdt;
        }

        public async Task<decimal> GetEtherBalance(string publishKey, string urlRPC)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12
                | SecurityProtocolType.Tls11
                | SecurityProtocolType.Tls
                | SecurityProtocolType.SystemDefault;

            var web3 = new Web3(urlRPC);

            var balance = await web3.Eth.GetBalance.SendRequestAsync(publishKey);

            decimal balanceEther = Web3.Convert.FromWei(balance);

            return balanceEther;
        }
    }
}
