using Core.Application.ViewModels.Exchange;
using Core.Areas.Admin.Controllers;
using Core.Utilities.Constants;
using Core.Utilities.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using Core.Data.Enums;
using Core.Application.Interfaces;
using Core.Data.Entities;
using Core.Application.Implementation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Core.Application.ViewModels.BlockChain;
using Core.Application.ViewModels.Staking;
using Core.Application.ViewModels.InvestPacakage;
using Core.Web.Authorization;

namespace Core.Web.Areas.Admin.Controllers
{
    public class StakingController : BaseController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IBlockChainService _blockChainService;
        private readonly IConfigService _configService;
        private readonly IWalletTransactionService _walletTransactionService;
        private readonly IStakingService _stakingService;

        private IConfiguration _configuration;

        private readonly ILogger<StakingController> _logger;
        public StakingController(
            IBlockChainService blockChainService,
            IConfiguration configuration,
            IUserService userService,
            IWalletTransactionService walletTransactionService,
            ILogger<StakingController> logger,
            UserManager<AppUser> userManager,
            IConfigService configService,
            IStakingService stakingService)
        {
            _configService = configService;
            _logger = logger;
            _blockChainService = blockChainService;
            _configuration = configuration;
            _walletTransactionService = walletTransactionService;
            _userManager = userManager;
            _stakingService = stakingService;
        }

        public IActionResult Index()
        {
            return View();
        }


        public async Task<GenericResult> GetBalance(Unit unit)
        {
            var appUser = await _userManager.FindByIdAsync(CurrentUserId.ToString());

            var tokenPrice = await _blockChainService.GetPiNetworkPrice();

            decimal balance = 0;

            decimal coinPrice = 1;

            decimal minPayment = CommonConstants.MinStakingPI;

            switch (unit)
            {
                case Unit.USDT:
                    balance = appUser.USDTAmount;
                    coinPrice = 1;
                    break;
                case Unit.BUSD:
                    balance = appUser.BUSDAmount;
                    coinPrice = 1;
                    break;
                case Unit.BNB:
                    balance = appUser.BNBAmount;
                    var bnbPrice = _blockChainService.GetCurrentPrice("BNB", "USD");
                    minPayment = Math.Round(CommonConstants.MinStakingPI / bnbPrice, 2);
                    coinPrice = bnbPrice;
                    break;
                default:
                    break;
            }

            var model = new ExchangeBalanceViewModel()
            {
                Balance = balance,
                CoinPrice = coinPrice,
                TokenPrice = Math.Round(tokenPrice,4),
                MinPayment = minPayment
            };

            return new GenericResult(true, model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ConfirmStaking([FromBody] BuyStakingViewModel model)
        {
            _logger.LogError($"ConfirmStaking Begin {CurrentUserId} - {model.AmountPayment} - {model.Unit}");

            var result = await _stakingService.PaymentStakingAsync(model, CurrentUserId);

            _logger.LogError($"ConfirmStaking End {CurrentUserId} - {model.AmountPayment} - {model.Unit} - Result {result.Message}");

            return new OkObjectResult(result);
        }

        [HttpGet]
        public IActionResult History()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetAllPaging(string keyword, int page, int pageSize)
        {
            var fromDate = DateTime.MinValue;
            var toDate = DateTime.MaxValue;  
            var model = _stakingService.GetAllPaging(keyword, CurrentUserId.ToString(), fromDate, toDate, page, pageSize);

            return new OkObjectResult(model);
        }

        [HttpGet]
        public IActionResult GetProfitAllPaging(string keyword, int page, int pageSize)
        {
            var fromDate = DateTime.MinValue;
            var toDate = DateTime.MaxValue;
            var model = _stakingService.GetAllProfitPaging(keyword, CurrentUserId.ToString(), fromDate, toDate, page, pageSize);

            return new OkObjectResult(model);
        }

        

        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> CancelStaking(PaymentInvestModel model)
        {
            var result = await _stakingService.CancelStaking(CurrentUserId, model.Id);

            return new OkObjectResult(result);
        }

        [HttpGet]
        public IActionResult Profit()
        {
            return View();
        }
    }
}
