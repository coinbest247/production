using System;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Interfaces;
using Core.Application.ViewModels.BlockChain;
using Core.Application.ViewModels.Common;
using Core.Application.ViewModels.InvestPacakage;
using Core.Application.ViewModels.Transfer;
using Core.Data.Entities;
using Core.Data.Enums;
using Core.Extensions;
using Core.Services;
using Core.Utilities.Constants;
using Core.Utilities.Extensions;
using Core.Web.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nethereum.Util;

namespace Core.Areas.Admin.Controllers
{
    public class InvestPackageController : BaseController
    {
        private readonly IUserService _userService;
        private readonly UserManager<AppUser> _userManager;
        private readonly IBlockChainService _blockChainService;
        private readonly ILogger<InvestPackageController> _logger;
        private readonly AddressUtil _addressUtil = new AddressUtil();
        private readonly IEmailSender _emailSender;
        private readonly IInvestPackageService _investPackageService;
        private readonly IConfiguration _configuration;
        private readonly IInvestPackageRewardService _investPackageRewardService;

        public InvestPackageController(
            IConfiguration configuration,
            IInvestPackageService investPackageService,
            ILogger<InvestPackageController> logger,
            UserManager<AppUser> userManager,
            IUserService userService,
            IEmailSender emailSender,
            IBlockChainService blockChainService,
            IInvestPackageRewardService investPackageRewardService)
        {
            _logger = logger;
            _userManager = userManager;
            _userService = userService;
            _emailSender = emailSender;
            _configuration = configuration;
            _blockChainService = blockChainService;
            _investPackageService = investPackageService;
            _investPackageRewardService = investPackageRewardService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var enumStakingPackages = ((StakingPackage[])Enum
                .GetValues(typeof(StakingPackage)))
                .Select(c => new EnumModel()
                {
                    Value = (int)c,
                    Name = c.GetDescription()
                }).ToList();

            ViewBag.PackageType = new SelectList(enumStakingPackages, "Value", "Name");

            return View();
        }

        [HttpGet]
        public IActionResult History()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetAllPaging(string keyword, int page, int pageSize)
        {
            var model = _investPackageService.GetAllPaging(keyword, CurrentUserId, 0, page, pageSize);

            return new OkObjectResult(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Leaderboard()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Profit()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetProfitAllPaging(string keyword, int page, int pageSize)
        {
            string appUserId = string.Empty;

            if (!IsAdmin)
                appUserId = CurrentUserId.ToString();

            var model = _investPackageRewardService.GetAllPaging(keyword, appUserId, page, pageSize);

            return new OkObjectResult(model);
        }

        [HttpGet]
        public IActionResult AffiliateProfit()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetAffiliateProfitAllPaging(string keyword, int page, int pageSize)
        {

            var model = _investPackageService.GetAllInvestPackageAffiliatePaging(keyword, CurrentUserId, page, pageSize);

            return new OkObjectResult(model);
        }



        [HttpGet]
        public async Task<IActionResult> GetWalletBlance()
        {
            var userId = User.GetSpecificClaim("UserId");

            var appUser = await _userService.GetById(userId);

            var model = new WalletViewModel()
            {
                USDTAmount = appUser.USDTAmount,
                BCAmount = appUser.BCAmount
            };

            return new OkObjectResult(model);
        }

        public IActionResult StakingTransaction()
        {
            if (!IsAdmin)
                RedirectHome();

            return View();
        }

        //[AllowAnonymous]
        //public IActionResult ProcessDailyStakingProfit()
        //{
        //    _logger.LogInformation("ProcessDailyStakingProfit - Begin");

        //    _stakingService.PaymentProfit();

        //    _logger.LogInformation("ProcessDailyStakingProfit - Complete");

        //    return new OkObjectResult(true);
        //}

        //[AllowAnonymous]
        //public async Task<IActionResult> ProcessRandomStaking()
        //{
        //    _logger.LogInformation("ProcessDailyStakingProfit - Begin");

        //    await _stakingService.RandomStakingShowUser();

        //    _logger.LogInformation("ProcessDailyStakingProfit - Complete");

        //    return new OkObjectResult(true);
        //}


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
                    isAllowPayment = balance >= CommonConstants.MinInvestBotTrade;
                    if (!isAllowPayment)
                        errorMsg = $"Min payment is {CommonConstants.MinInvestBotTrade} USDT";
                    break;
                case Unit.BUSD:
                    balance = appUser.BUSDAmount;
                    isAllowPayment = balance >= CommonConstants.MinInvestBotTrade;
                    if (!isAllowPayment)
                        errorMsg = $"Min payment is {CommonConstants.MinInvestBotTrade} BUSD";
                    break;
                case Unit.SHIBAINU:
                    break;
                case Unit.BNB:
                    balance = appUser.BNBAmount;
                    if (balance > 0)
                    {
                        var bnbPrice = _blockChainService.GetCurrentPrice("BNB", "USD");
                        var bnbRequired = Math.Round(CommonConstants.MinInvestBotTrade / bnbPrice, 4);
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

        public IActionResult PaymentInvest()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestFilter("ConfirmPaymentInvest", NoOfRequest = 1, Seconds = 5)]

        public async Task<IActionResult> ConfirmPaymentInvest([FromBody] PaymentInvestModel model)
        {
            var result = await _investPackageService.ProcessBuyInvestPackage(model, CurrentUserId.ToString());

            return new OkObjectResult(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestFilter("CancelInvest", NoOfRequest = 1, Seconds = 5)]

        public async Task<IActionResult> CancelInvest(PaymentInvestModel model)
        {
            var result = await _investPackageService.ProcessCancelInvestPackage(model, CurrentUserId.ToString());

            return new OkObjectResult(result);
        }

        [AllowAnonymous]
        public async Task<IActionResult> ProcessDailyInvestProfit()
        {
            _logger.LogInformation("ProcessDailyInvestProfit - Begin");

            await _investPackageService.ProcessDailyInvestProfit();

            _logger.LogInformation("ProcessDailyInvestProfit - Complete");

            return new OkObjectResult(true);
        }
    }
}