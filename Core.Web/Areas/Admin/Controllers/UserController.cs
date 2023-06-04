using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Implementation;
using Core.Application.Interfaces;
using Core.Application.ViewModels.System;
using Core.Data.Entities;
using Core.Data.Enums;
using Core.Infrastructure.Interfaces;
using Core.Utilities.Constants;
using Core.Utilities.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace Core.Areas.Admin.Controllers
{
    public class UserController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRoleService _roleService;
        private readonly IWalletTransactionService _walletTransactionService;
        private readonly IUserService _userService;
        private readonly IAuthorizationService _authorizationService;
        private readonly ITRONService _tronService;
        private readonly IBlockChainService _blockChainService;
        private readonly UserManager<AppUser> _userManager;
        private readonly IImportManager _importManager;

        public UserController(IUnitOfWork unitOfWork,
                              IRoleService roleService,
                              IUserService userService,
                              IAuthorizationService authorizationService,
                              ITRONService tronService,
                              UserManager<AppUser> userManager,
                              IBlockChainService blockChainService,
                              IWalletTransactionService walletTransactionService,
                              IImportManager importManager)
        {
            _importManager = importManager;
            _blockChainService = blockChainService;
            _unitOfWork = unitOfWork;
            _roleService = roleService;
            _userService = userService;
            _authorizationService = authorizationService;
            _tronService = tronService;
            _userManager = userManager;
            _walletTransactionService = walletTransactionService;
        }

        public async Task<IActionResult> Index()
        {
            
            if (!IsAdmin)
                return Redirect("/logout");

            //var result = await _authorizationService.AuthorizeAsync(User, "USER", Operations.Read);
            //if (result.Succeeded == false)
            //    return new RedirectResult("/Admin/Account/Login");

            var roles = await _roleService.GetAllAsync();
            ViewBag.RoleId = new SelectList(roles, "Name", "Name");

            return View();
        }

        public IActionResult IndexTree()
        {
            if (!IsAdmin)
                return Redirect("/logout");

           
            return View();
        }

        //[HttpGet]
        //public IActionResult GetTreeAll()
        //{
        //    var model = _userService.GetTreeAll();
        //    return new ObjectResult(model);
        //}

        public IActionResult GetTreeNode(string parent)
        {
            //var roleName = User.GetSpecificClaim("RoleName");
            //if (roleName.ToLower() != "admin")
            //    return Redirect("/logout");

            //return View();

            var model = _userService.GetTreeAll(parent);
            return new ObjectResult(model);
        }

        [HttpGet]
        public IActionResult GetAllPaging(string keyword, int page, int pageSize)
        {
            var model = _userService.GetAllPagingAsync(keyword, page, pageSize);
            return new OkObjectResult(model);
        }

        public IActionResult Customers()
        {
            if (!IsAdmin)
                return Redirect("/logout");

            //var result = await _authorizationService.AuthorizeAsync(User, "USER", Operations.Read);
            //if (result.Succeeded == false)
            //    return new RedirectResult("/Admin/Account/Login");

            return View();
        }

        [HttpGet]
        public IActionResult GetAllCustomerPaging(string keyword, int page, int pageSize)
        {
            var model = _userService.GetAllCustomerPagingAsync(keyword, page, pageSize);
            return new OkObjectResult(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetCustomerSetting(string id)
        {
            var user = await _userService.GetById(id);
            return new OkObjectResult(user);
        }

        public IActionResult StatementAllUser()
        {
            if (!IsAdmin)
                return Redirect("/logout");

            //var result = await _authorizationService.AuthorizeAsync(User, "USER", Operations.Read);
            //if (result.Succeeded == false)
            //    return new RedirectResult("/Admin/Account/Login");

            return View();
        }

        [HttpGet]
        public IActionResult GetStatementAllUser(string keyword, int type)
        {
            var model = _userService.GetStatementUser(keyword, type);
            return new OkObjectResult(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetById(string id)
        {
            var model = await _userService.GetById(id);

            return new OkObjectResult(model);
        }

        [HttpPost]
        public async Task<IActionResult> SaveEntity(AppUserViewModel userVm)
        {
            if (!IsAdmin)
                return Redirect("/logout");

            if (!ModelState.IsValid)
            {
                IEnumerable<ModelError> allErrors = ModelState.Values.SelectMany(v => v.Errors);
                return new BadRequestObjectResult(allErrors);
            }
            else
            {
                if (userVm.Id == null)
                    await _userService.AddAsync(userVm);
                else
                    await _userService.UpdateAsync(userVm);

                return new OkObjectResult(userVm);
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCustomerSetting([FromBody] AppUserViewModel userVm)
        {
            var user = await _userManager.FindByIdAsync(userVm.Id.Value.ToString());

            if (!userVm.Enabled2FA)
            {
                user.TwoFactorEnabled = false;
            }

            _unitOfWork.Commit();

            return new OkObjectResult(new GenericResult(true, "Update Success!"));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCustomerLeaderSetting([FromBody] AppUserViewModel userVm)
        {
            var user = await _userManager.FindByIdAsync(userVm.Id.Value.ToString());
            
            if (userVm.IsLeader)
            {


                await _userManager.AddToRoleAsync(user, CommonConstants.LEADER_ROLE);

                var isCustomer = await _userManager.IsInRoleAsync(user, CommonConstants.CUSTOMER_ROLE);

                if (isCustomer)
                    await _userManager.RemoveFromRoleAsync(user, CommonConstants.CUSTOMER_ROLE);


            }
            else
            {
                var isLeader = await _userManager.IsInRoleAsync(user, CommonConstants.LEADER_ROLE);
                if (isLeader)
                    await _userManager.RemoveFromRoleAsync(user, CommonConstants.LEADER_ROLE);

                var isCustomer = await _userManager.IsInRoleAsync(user, CommonConstants.CUSTOMER_ROLE);

                if (!isCustomer)
                    await _userManager.AddToRoleAsync(user, CommonConstants.CUSTOMER_ROLE);

            }

            

            return new OkObjectResult(new GenericResult(true, "Update Success!"));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCustomer(string id)
        {
            try
            {
                if (!IsAdmin)
                    return Redirect("/logout");

                var model = await _userService.GetById(id);
                if (model.EmailConfirmed == true)
                {
                    return new OkObjectResult(new GenericResult(false, "Accounts confirmed email cannot be deleted."));
                }

                await _userService.DeleteAsync(id);

                return new OkObjectResult(new GenericResult(true, "Account deleted successfully"));
            }
            catch (Exception ex)
            {
                return new OkObjectResult(new GenericResult(false, ex.Message));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            if (!IsAdmin)
                return Redirect("/logout");

            if (!ModelState.IsValid)
                return new BadRequestObjectResult(ModelState);
            else
            {
                await _userService.DeleteAsync(id);

                return new OkObjectResult(id);
            }
        }

        [HttpPost]
        public async Task<IActionResult> ResetUserPassword(string id)
        {
            if (!IsAdmin)
                return Redirect("/logout");

            if (!ModelState.IsValid)
                return new BadRequestObjectResult(ModelState);
            else
            {
                var user = await _userManager.FindByIdAsync(id);

                if (user == null)
                    return new OkObjectResult(new { Success = false, Message = "User not found" });

                await _userManager.RemovePasswordAsync(user);
                await _userManager.AddPasswordAsync(user, "12345678#");


                return new OkObjectResult(new { Success = true, Message = "Reset password success" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UnlockUser(string id)
        {
            if (!IsAdmin)
                return Redirect("/logout");

            if (!ModelState.IsValid)
                return new BadRequestObjectResult(ModelState);
            else
            {
                var user = await _userManager.FindByIdAsync(id);

                if (user == null)
                    return new OkObjectResult(new { Success = false, Message = "User not found" });

                if (user.LockoutEnd != null)
                {
                    await _userManager.SetLockoutEnabledAsync(user, false);
                    await _userManager.SetLockoutEndDateAsync(user, null);
                }

                return new OkObjectResult(new { Success = true, Message = "Unlock user success" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> LockUser(string id)
        {
            if (!IsAdmin)
                return Redirect("/logout");

            if (!ModelState.IsValid)
                return new BadRequestObjectResult(ModelState);
            else
            {
                var user = await _userManager.FindByIdAsync(id);

                if (user == null)
                    return new OkObjectResult(new { Success = false, Message = "User not found" });

                await _userManager.SetLockoutEnabledAsync(user, true);
                await _userManager.SetLockoutEndDateAsync(user, DateTime.UtcNow.AddYears(50));

                return new OkObjectResult(new { Success = true, Message = "lock user success" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmEmail(string id)
        {
            if (id == null)
            {
                return new OkObjectResult(new { Success = false, Message = "User not found" });

            }

            if (!IsAdmin)
                return new OkObjectResult(new { Success = false, Message = "Permission not allow" });

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return new OkObjectResult(new { Success = false, Message = "User not found" });
            }

            if (user.EmailConfirmed)
            {
                return new OkObjectResult(new { Success = false, Message = "User activated already" });
            }

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var result = await _userManager.ConfirmEmailAsync(user, code);

            if (result.Succeeded)
            {
                var wallet = _blockChainService.CreateAccount();

                user.PublishKey = wallet.Address;
                user.PrivateKey = wallet.PrivateKey;

                var userUpdateConfirmEmail = await _userManager.UpdateAsync(user);

                return new OkObjectResult(new { Success = true, Message = "Confirm email success" });

            }


            return new OkObjectResult(new { Success = false, Message = "Confirm Email Failed" });
        }


        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> UpdateWallet(string modelJson)
        //{
        //    try
        //    {
        //        var model = JsonConvert.DeserializeObject<UpdateWalletBalanceViewModel>(modelJson);

        //        var appUser = await _userManager.FindByIdAsync(model.UserId);
        //        if (appUser == null)
        //            return new OkObjectResult(new GenericResult(false, "Account does not exist"));

        //        if (model.Amount <= 0)
        //            return new OkObjectResult(new GenericResult(false, "Amount is valid"));

        //        UpdateWalletBalanceActionType actionType = model.ActionType == 1 ?
        //            UpdateWalletBalanceActionType.Deposit : UpdateWalletBalanceActionType.Withdraw;

        //        bool isUpdated = false;

        //        var tokenConfig = _tokenConfigService.GetByCode(model.WalletType);

        //        if (tokenConfig == null)
        //            return new OkObjectResult(new GenericResult(false, "Wallet invalid"));

        //        if (actionType == UpdateWalletBalanceActionType.Deposit)
        //            isUpdated = _walletService.DepositToRegularWallet(appUser.Id, model.Amount, tokenConfig.Id);
        //        else
        //        {
        //            var walletBalance = _walletService.GetTokenBalance(appUser.Id, tokenConfig.Id);

        //            if (model.Amount > walletBalance)
        //                return new OkObjectResult(new GenericResult(false, "Amount withdraw not valid"));

        //            isUpdated = _walletService.WithdrawToRegularWallet(appUser.Id, model.Amount, tokenConfig.Id);
        //        }
                    

        //        if (isUpdated)
        //        {
        //            var txnHash = Guid.NewGuid().ToString("N");

        //            decimal priceInUSDT = _blockChainService.GetCurrentPrice(tokenConfig.TokenCode, "USD");

        //            decimal tokenUSDTValue = priceInUSDT * model.Amount;

        //            #region Add transaction
        //            if (actionType == UpdateWalletBalanceActionType.Deposit)
        //                _walletTransactionService.AddTransaction(
        //                    appUser,
        //                    model.Amount,
        //                    model.Amount,
        //                    WalletTransactionType.Deposit,
        //                    appUser.Email,
        //                    "SYSTEM",
        //                    $"Wallet {tokenConfig.TokenCode}",
        //                    0,
        //                    0,
        //                    txnHash,
        //                    $"Deposit from SYSTEM to {appUser.Email} by administrator",
        //                    tokenUSDTValue);
        //            else
        //                _walletTransactionService.AddTransaction(
        //                        appUser,
        //                        model.Amount,
        //                        model.Amount,
        //                        WalletTransactionType.Withdraw,
        //                        $"Wallet {tokenConfig.TokenCode}",
        //                        "SYSTEM",
        //                        tokenConfig.TokenCode,
        //                        0,
        //                        0,
        //                        txnHash,
        //                        $"Withdraw from {appUser.Email} to SYSTEM by administrator",
        //                        tokenUSDTValue);
        //            #endregion
        //        }
        //        else
        //            return new OkObjectResult(new GenericResult(false,
        //                "Update balance error"));

        //        return new OkObjectResult(
        //            new GenericResult(true, "Update wallet balance is successful."));
        //    }
        //    catch (Exception ex)
        //    {
        //        return new OkObjectResult(new GenericResult(false, ex.Message));
        //    }
        //}

        public IActionResult ImportShowUser()
        {

            if (!IsAdmin)
                return Redirect("/logout");

            
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ImportShowUser(IFormFile file)
        {
            //var roleName = User.GetSpecificClaim("RoleName");
            //if (roleName.ToLower() != "admin")
            //    return Redirect("/logout");
            if (file == null)
            {
                return View();
            }

            var importResult = await _importManager.ImportShowUsersFromXlsx(file.OpenReadStream());

            ViewBag.Msg = importResult.ErrorMsg;

            return View();
        }

        [HttpGet]
        public IActionResult GetAllUserShowPaging(string keyword, int page, int pageSize)
        {
            var model = _userService.GetAllUserShowPagingAsync(keyword, page, pageSize);
            return new OkObjectResult(model);
        }

        //public IActionResult GetWalletBalanceSummary(Guid appUserId)
        //{
        //    var wallet = _walletService.GetAllByUserId(appUserId);
        //    return View(wallet);
        //}

        [HttpPost]
        public async Task<IActionResult> UpdateCustomerWithdrawSetting([FromBody] AppUserViewModel userVm)
        {
            var user = await _userManager.FindByIdAsync(userVm.Id.Value.ToString());

            user.IsRejectWithdraw = userVm.IsRejectWithdraw;

            await _userManager.UpdateAsync(user);

            return new OkObjectResult(new GenericResult(true, "Update Success!"));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCustomerTransferSetting([FromBody] AppUserViewModel userVm)
        {
            var user = await _userManager.FindByIdAsync(userVm.Id.Value.ToString());

            user.IsRejectTransfer = userVm.IsRejectTransfer;

            await _userManager.UpdateAsync(user);

            return new OkObjectResult(new GenericResult(true, "Update Success!"));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCustomerSwapSetting([FromBody] AppUserViewModel userVm)
        {
            var user = await _userManager.FindByIdAsync(userVm.Id.Value.ToString());

            user.IsRejectSwap = userVm.IsRejectSwap;

            await _userManager.UpdateAsync(user);

            return new OkObjectResult(new GenericResult(true, "Update Success!"));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCustomerInterestSetting([FromBody] AppUserViewModel userVm)
        {
            AppUser user = await _userManager.FindByIdAsync(userVm.Id.Value.ToString());

            user.IsRejectInterest = userVm.IsRejectInterest;

            await _userManager.UpdateAsync(user);

            return new OkObjectResult(new GenericResult(true, "Update Success!"));
        }
    }
}
