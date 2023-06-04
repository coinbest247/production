using Core.Application.Interfaces;
using Core.Utilities.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Core.Areas.Admin.Controllers;
using Microsoft.Extensions.Configuration;
using Core.Application.ViewModels.BlockChain;
using Core.Utilities.Constants;
using Microsoft.AspNetCore.Identity;
using Core.Data.Enums;
using Core.Data.Entities;
using Core.Application.ViewModels.Exchange;
using Microsoft.AspNetCore.Authorization;
using System;

namespace Core.Web.Areas.Admin.Controllers
{
    public class ExchangeController : BaseController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IBlockChainService _blockChainService;
        private readonly IConfigService _configService;
        private readonly IWalletTransactionService _walletTransactionService;
        private readonly IUserService _userService;
        private readonly ISaleService _saleService;

        private IConfiguration _configuration;

        private readonly ILogger<ExchangeController> _logger;
        public ExchangeController(
            IBlockChainService blockChainService,
            IConfiguration configuration,
            IUserService userService,
            IWalletTransactionService walletTransactionService,
            ILogger<ExchangeController> logger,
            UserManager<AppUser> userManager,
            IConfigService configService,
            ISaleService saleService)
        {
            _configService = configService;
            _logger = logger;
            _userService = userService;
            _blockChainService = blockChainService;
            _configuration = configuration;
            _walletTransactionService = walletTransactionService;
            _userManager = userManager;
            _saleService = saleService;
        }

        public ActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public GenericResult GetTokenPrice()
        {
            var tokenPrice = _configService.GetTokenPrice();

            var bnbPrice = _blockChainService.GetCurrentPrice("BNB", "USD");

            var model = new ExchangeBalanceViewModel()
            {
                TokenPrice = tokenPrice,
                CoinPrice = bnbPrice,
                MinPayment = CommonConstants.BNBMinExchange
            };

            return new GenericResult(true, model);
        }

        public async Task<GenericResult> GetBalance(Unit unit)
        {
            var appUser = await _userManager.FindByIdAsync(CurrentUserId.ToString());

            var tokenPrice = _configService.GetTokenPrice();

            decimal balance = 0;

            decimal coinPrice = 1;

            decimal minPayment = CommonConstants.MinBuyToken;

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
                    minPayment = Math.Round(CommonConstants.MinBuyToken / bnbPrice, 2);
                    coinPrice = bnbPrice;
                    break;
                default:
                    break;
            }

            var model = new ExchangeBalanceViewModel()
            {
                Balance = balance,
                CoinPrice = coinPrice,
                TokenPrice = tokenPrice,
                MinPayment = minPayment
            };

            return new GenericResult(true, model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ConfirmBuyToken([FromBody] SaleManualViewModel model)
        {
            _logger.LogError($"ConfirmBuyToken Begin {CurrentUserId} - {model.Amount} - {model.Unit}");

            var result = await _saleService.ProcessBuyToken(model, CurrentUserId.ToString());

            _logger.LogError($"ConfirmBuyToken End {CurrentUserId} - {model.Amount} - {model.Unit} - Result {result.Message}");

            return new OkObjectResult(result);
        }

        [AllowAnonymous]
        [Route("exchangedefi")]
        public ActionResult ExchangeDefi()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult GetData()
        {
            decimal priceBNBBep20 = _blockChainService.GetCurrentPrice("BNB", "USD");
            if (priceBNBBep20 == 0)
                priceBNBBep20 = 0;

            decimal tokenPrice = _configuration.GetValue<decimal>("TokenConfig:TokenPrice");

            var model = new ExchangeTokenDefiViewModel()
            {
                BNBPrice = priceBNBBep20,
                TokenPrice = tokenPrice,
                TokenPriceString = tokenPrice.ToString()
            };

            return new OkObjectResult(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetAllPaging(string keyword, string address, int page, int pageSize)
        {

            var model = _saleService.GetAllPaging(keyword, address, page, pageSize);

            return new OkObjectResult(model);
        }
    }
}
