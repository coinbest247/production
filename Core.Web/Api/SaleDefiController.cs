using Core.Application.ViewModels.Dapp;
using Core.Application.ViewModels.QueueTask;
using Core.Data.Entities;
using Core.Infrastructure.Interfaces;
using Core.Utilities.Dtos;
using Core.Web.Models.RequestParams;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;
using Newtonsoft.Json;
using Core.Data.Enums;
using Core.Application.Interfaces;
using Core.Data.IRepositories;
using Core.Utilities.Constants;
using Microsoft.CodeAnalysis.FlowAnalysis;

namespace Core.Web.Api
{
    public class SaleDefiController : Controller
    {
        private readonly IRepository<QueueTask, int> _queueRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SaleDefiController> _logger;
        private readonly ISaleService _saleService;
        private readonly ISaleDefiRepository _saleDefiRepository;
        private readonly UserManager<AppUser> _userManager;
        public const string DAppTransactionId = "DAppTransactionId";

        public string AccessAddress
        {
            get
            {
                if (!string.IsNullOrEmpty(Request.Headers["connectedaddress"]))
                    return Request.Headers["connectedaddress"];

                return string.Empty;
            }
        }

        public SaleDefiController(
            IUnitOfWork unitOfWork,
            ILogger<SaleDefiController> logger,
            ISaleService saleService,
            UserManager<AppUser> userManager,
            IRepository<QueueTask, int> queueRepository,
            ISaleDefiRepository saleDefiRepository
            )
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _saleService = saleService;
            _userManager = userManager;
            _queueRepository = queueRepository;
            _saleDefiRepository = saleDefiRepository;
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> InitializeTransactionProgress(
            [FromBody] SaleInitializationParams model)
        {
            try
            {
                if (model.BNBAmount < CommonConstants.BNBMinExchange)
                    return BadRequest(new GenericResult(false, $"Min buy {CommonConstants.BNBMinExchange} BNB"));

                _logger.LogInformation("Start call InitializeTransactionProgress with param: {@0}", model);

                var res = await _saleService.InitializeTransactionProgress(model);

                if (!res.result.Success)
                    return BadRequest(res.result);

                _logger.LogInformation($"End call InitializeTransactionMetaMask");

                return Ok(res.result);
            }
            catch (Exception e)
            {
                _logger.LogError("InitializeTransactionProgress: {@0}", e);
                return BadRequest(new GenericResult(false, e.Message));
            }
        }

        /// <summary>
        /// VerifyTransactionRequest
        /// </summary>
        /// <param name = "transactionHash" ></ param >
        /// < returns ></ returns >
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyTransactionRequest(
            [FromBody] DAppTransactionVerifyModel request)
        {

            if (string.IsNullOrEmpty(request.TransactionHash))
            {
                return BadRequest(new GenericResult(false,
                    "Somethings went wrong! Please Contact administrator for support."));
            }

            try
            {
                var result = await _saleService.ProcessVerificationBNBTransaction(request.TransactionHash);

                return Ok(new GenericResult(true,
                    "We are processing your claim request, kindly wait few minutes and check your wallet."));
            }
            catch (Exception e)
            {
                _logger.LogError("Internal Error: {@0}", e);

                try
                {
                    var temptransactionId = _saleService
                        .GetLatestTransactionByWalletAddress(request.DappTxnHash);

                    var metamaskTransaction = _saleDefiRepository.FindById(Guid.Parse(temptransactionId));

                    metamaskTransaction.Remarks = e.Message;
                    metamaskTransaction.BNBTransactionHash = request.TransactionHash;
                    metamaskTransaction.TransactionState = SaleDefiTransactionState.Failed;
                    _unitOfWork.Commit();
                }
                catch (Exception ex)
                {
                    _logger.LogError("Internal Error: {@0}", ex);
                }

                return BadRequest(new GenericResult(false,
                    "Somethings went wrong! Please Contact administrator for support."));
            }
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public IActionResult UpdateErrorMetaMask(MetaMaskErrorParams model)
        {

            _logger.LogInformation("UpdateErrorMetaMask: {@0}", JsonConvert.SerializeObject(model));

            var convertor = new Nethereum.Hex.HexConvertors.HexUTF8StringConvertor();

            var transactionId = convertor.ConvertFromHex(model.TransactionHex);

            _logger.LogInformation("UpdateErrorMetaMask: transactionId {@0}", transactionId);

            if (string.IsNullOrEmpty(model.TransactionHex))
                return Ok(new GenericResult(false, "Transaction failed"));

            var result = _saleDefiRepository.FindById(Guid.Parse(transactionId));

            switch (model.ErrorCode)
            {
                case "4001":
                    result.TransactionState = SaleDefiTransactionState.Rejected;
                    break;

                case "-32603":
                    result.TransactionState = SaleDefiTransactionState.Failed;
                    break;

                default:
                    result.TransactionState = SaleDefiTransactionState.Failed;
                    break;
            }

            _unitOfWork.Commit();

            return Ok(new GenericResult(true, "Successed to update."));
        }
    }
}
