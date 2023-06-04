using System;
using System.Threading.Tasks;
using Core.Application.Interfaces;
using Core.Application.ViewModels.Transfer;
using Core.Data.Entities;
using Core.Data.Enums;
using Core.Utilities.Constants;
using Core.Web.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Core.Areas.Admin.Controllers
{
    public class AffiliateAgentController : BaseController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IBlockChainService _blockChainService;
        private readonly ILogger<AffiliateAgentController> _logger;
        private readonly IAffiliateAgentService _affiliateAgentService;

        public AffiliateAgentController(
            ILogger<AffiliateAgentController> logger,
            UserManager<AppUser> userManager,
            IBlockChainService blockChainService,
            IAffiliateAgentService affiliateAgentService
            )
        {
            _logger = logger;
            _userManager = userManager;
            _blockChainService = blockChainService;
            _affiliateAgentService = affiliateAgentService;
        }

        public IActionResult AgentActivation()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestFilter("ConfirmAgentActivation", NoOfRequest = 1, Seconds = 5)]
        public async Task<IActionResult> ConfirmAgentActivation(Unit unit)
        {
            var result = await _affiliateAgentService.ProcessAgentActivation(CurrentUserId, unit);
            return new OkObjectResult(result);
        }

        public IActionResult AgentCommission()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetAgentCommissionAllPaging(string keyword, int page, int pageSize)
        {
            var model = _affiliateAgentService.GetAgentCommissionPaging(keyword, CurrentUserId, page, pageSize);

            return new OkObjectResult(model);
        }

        public async Task<IActionResult> GetBalance(Unit unit)
        {
            var appUser = await _userManager.FindByIdAsync(CurrentUserId.ToString());

            decimal balance = 0;

            string errorMsg = "";

            bool isAllowPayment = true;

            switch (unit)
            {
                case Unit.Token:
                    break;
                case Unit.USDT:
                    balance = appUser.USDTAmount;
                    isAllowPayment = balance >= CommonConstants.AffiliateAgent;
                    if (!isAllowPayment)
                        errorMsg = $"Min payment is {CommonConstants.AffiliateAgent} USDT";
                    break;
                case Unit.BUSD:
                    balance = appUser.BUSDAmount;
                    isAllowPayment = balance >= CommonConstants.AffiliateAgent;
                    if (!isAllowPayment)
                        errorMsg = $"Min payment is {CommonConstants.AffiliateAgent} BUSD";
                    break;
                case Unit.SHIBAINU:
                    break;
                case Unit.BNB:
                    balance = appUser.BNBAmount;
                    if (balance > 0)
                    {
                        var bnbPrice = _blockChainService.GetCurrentPrice("BNB", "USD");
                        var bnbRequired = Math.Round(CommonConstants.AffiliateAgent / bnbPrice, 4);
                        isAllowPayment = balance >= bnbRequired;
                        if (!isAllowPayment)
                            errorMsg = $"Min payment is {bnbRequired} BNB";
                    }
                    else
                    {
                        isAllowPayment = false;
                        errorMsg = $"Balance is 0, cant payment";
                    }

                    break;
                default:
                    break;
            }

            var model = new TransferBalanceViewModel()
            {
                Balance = balance,
                IsAllowPayment = isAllowPayment,
                ErrorMsg = errorMsg
            };

            return new OkObjectResult(model);
        }
    }
}