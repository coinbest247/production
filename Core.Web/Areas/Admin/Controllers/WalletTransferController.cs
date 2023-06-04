using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Application.Interfaces;
using Core.Application.ViewModels.System;
using Core.Data.Entities;
using Core.Infrastructure.SmartContracts.Wallet;
using Core.Utilities.Constants;
using Core.Utilities.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Core.Areas.Admin.Controllers
{
    public class WalletTransferController : BaseController
    {
        public readonly IWalletTransferService _walletTransferService;
        private readonly UserManager<AppUser> _userManager;
        private readonly ITRONService _tronService;
        private readonly IBlockChainService _blockChainService;

        public WalletTransferController(
            IBlockChainService blockChainService,
            UserManager<AppUser> userManager,
            ITRONService tronService,
            IWalletTransferService walletTransferService
            )
        {
            _blockChainService = blockChainService;
            _userManager = userManager;
            _tronService = tronService;
            _walletTransferService = walletTransferService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public class CreateModel
        {
            public int TotalWallet { get; set; }
            public int AmountFrom { get; set; }
            public int AmountTo { get; set; }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string modelJson)
        {
            try
            {
                var model = JsonConvert.DeserializeObject<CreateModel>(modelJson);

                for (int i = 1; i <= model.TotalWallet; i++)
                {
                    var randAmount = new Random().Next(model.AmountFrom, model.AmountTo);

                    decimal amountTransfer = (decimal)randAmount;

                    var accountBep20 = _blockChainService.CreateAccount();

                    var transferTransaction = await _blockChainService.SendERC20WithoutReceiptAsync(
                        CommonConstants.TransferPrKey, accountBep20.Address,
                        CommonConstants.TokenContract, amountTransfer,
                        CommonConstants.TokenDecimals, CommonConstants.Url);

                    if (!string.IsNullOrWhiteSpace(transferTransaction))
                    {
                        var dateNow = DateTime.UtcNow;

                        var walletTransfer = new WalletTransferViewModel
                        {
                            PrivateKey = accountBep20.PrivateKey,
                            PublishKey = accountBep20.Address,
                            DateCreated = dateNow,
                            DateModified = dateNow,
                            TransferHash = transferTransaction,
                            Amount = amountTransfer,
                            IsRecall = false
                        };

                        _walletTransferService.Add(walletTransfer);
                        _walletTransferService.Save();
                    }

                    if (model.TotalWallet != i)
                    {
                        Thread.Sleep(5000);
                    }
                }

                return new OkObjectResult(new GenericResult(true,
                    $"Create {model.TotalWallet} holders and transaction are success."));
            }
            catch (Exception ex)
            {
                return new OkObjectResult(new GenericResult(false, ex.Message));
            }
        }

        public class RecallModel
        {
            public int TotalWallet { get; set; } = 1;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Recall(string modelJson)
        {
            try
            {
                var model = JsonConvert.DeserializeObject<RecallModel>(modelJson);

                var holders = _walletTransferService.GetNotRecall(model.TotalWallet);

                if (holders.Count >= 1)
                {
                    for (int i = 0; i < holders.Count; i++)
                    {
                        var holder = holders[i];

                        if (holder.Amount > 1)
                        {
                            decimal projectTokenBalance = await _blockChainService.GetERC20Balance(
                                holder.PublishKey,
                                CommonConstants.TokenContract,
                                CommonConstants.TokenDecimals,
                                CommonConstants.Url);

                            if (projectTokenBalance > 1)
                            {
                                projectTokenBalance = projectTokenBalance - 1;

                                var transaction = await _blockChainService.SendEthAsync(
                                    CommonConstants.TransferPrKey, holder.PublishKey,
                                    0.0003m, CommonConstants.Url);

                                Thread.Sleep(5000);

                                var wallet = WalletRecallHelper.GetWalletRamdom();

                                var recallTransaction = await _blockChainService.SendERC20WithoutReceiptAsync(
                                                                        holder.PrivateKey,
                                                                        wallet.PublishKey,
                                                                        CommonConstants.TokenContract,
                                                                        projectTokenBalance,
                                                                        CommonConstants.TokenDecimals,
                                                                        CommonConstants.Url
                                                                        );


                                if (!string.IsNullOrWhiteSpace(recallTransaction))
                                {
                                    holder.Amount = 1;
                                    holder.IsRecall = true;
                                    holder.DateModified = DateTime.Now;
                                    holder.RecallHash = recallTransaction;

                                    _walletTransferService.Update(holder);
                                }

                            }
                        }
                    }
                }
                else
                {
                    return new OkObjectResult(new GenericResult(false, "Running out of balance to withdraw"));
                }

                return new OkObjectResult(new GenericResult(true, "Recall is success"));
            }
            catch (Exception ex)
            {
                return new OkObjectResult(new GenericResult(false, ex.Message));
            }
        }

        [HttpGet]
        public IActionResult GetAllPaging(string keyword, int page, int pageSize)
        {
            var model = _walletTransferService.GetAllPaging(keyword, page, pageSize);
            return new OkObjectResult(model);
        }
    }
}
