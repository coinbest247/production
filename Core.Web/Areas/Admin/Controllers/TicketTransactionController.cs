using System;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Interfaces;
using Core.Application.ViewModels.BotTelegram;
using Core.Application.ViewModels.Common;
using Core.Data.Entities;
using Core.Data.Enums;
using Core.Extensions;
using Core.Services;
using Core.Utilities.Dtos;
using Core.Utilities.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Core.Areas.Admin.Controllers
{
    public class TicketTransactionController : BaseController
    {
        private readonly ITicketTransactionService _ticketTransactionService;
        private readonly IBlockChainService _blockChainService;
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailSender _emailSender;
        public TicketTransactionController(
            ITicketTransactionService ticketTransactionService,
            UserManager<AppUser> userManager,
            IEmailSender emailSender,
            IBlockChainService blockChainService
            )
        {
            _ticketTransactionService = ticketTransactionService;
            _userManager = userManager;
            _emailSender = emailSender;
            _blockChainService = blockChainService;
        }

        public IActionResult Index()
        {
            if (!IsAdmin)
                return Redirect("/logout");

            var enumTypes = ((TicketTransactionStatus[])Enum.
                GetValues(typeof(TicketTransactionStatus)))
                .Select(c => new EnumModel()
                {
                    Value = (int)c,
                    Name = c.GetDescription()
                }).ToList();

            ViewBag.TicketTransactionStatus = new SelectList(enumTypes, "Value", "Name");

            return View();
        }

        [HttpGet]
        public IActionResult GetAllPaging(string keyword, int transactionStatus, int page, int pageSize)
        {
            var model = _ticketTransactionService.GetAllPaging(keyword, "", transactionStatus, false, page, pageSize);
            return new OkObjectResult(model);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveTicket(int id)
        {
            if (!IsAdmin)
                return Redirect("/logout");

            if (!ModelState.IsValid)
                return new BadRequestObjectResult(ModelState);
            else
            {
                try
                {
                    var result = await _ticketTransactionService.Approved(id);

                    //if (result.Success)
                    //{
                    //    try
                    //    {
                    //        var ticket = result.Data as TicketTransaction;

                    //        var appUser = await _userManager.FindByIdAsync(ticket.AppUserId.ToString());
                    //        if (appUser != null)
                    //        {
                    //            var parameters = new WithdrawMessageParam
                    //            {
                    //                Amount = ticket.AmountReceive,
                    //                CreatedDate = DateTime.UtcNow,
                    //                Email = appUser.Email,
                    //                UserId = appUser.Sponsor,
                    //                WalletFrom = ticket.AddressFrom,
                    //                WalletTo = ticket.AddressTo,
                    //                Currency = ticket.Unit.GetDescription(),
                    //                FeeAmount = ticket.FeeAmount
                    //            };

                    //            if (appUser.ReferralId.HasValue)
                    //            {
                    //                var refferal = await _userManager.FindByIdAsync(appUser.ReferralId.ToString());

                    //                parameters.SponsorEmail = refferal.Email;
                    //                parameters.SponsorId = refferal.Sponsor;
                    //            }

                    //            var message = _emailSender.BuildReportWithdrawMessage(parameters);

                    //            //await _emailSender.TrySendEmailAsync(appUser.Email,
                    //            //    $"Withdraw Successful {DateTime.UtcNow.ToddMMyyyyHHmmss()}(UTC)", message);
                    //        }
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        //Ignore error from sending email.
                    //    }
                    //}

                    return new OkObjectResult(new GenericResult(result.Success, result.Message));
                }
                catch (Exception ex)
                {
                    return new OkObjectResult(new GenericResult(false, ex.Message));
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> RejectTicket(int id)
        {
            if (!IsAdmin)
                return Redirect("/logout");

            if (!ModelState.IsValid)
                return new BadRequestObjectResult(ModelState);
            else
            {
                try
                {
                    var result = await _ticketTransactionService.Rejected(id);

                    return new OkObjectResult(new GenericResult(result.Success, result.Message));
                }
                catch (Exception ex)
                {
                    return new OkObjectResult(new GenericResult(false, ex.Message));
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> RollbackTicket(int id)
        {
            if (!IsAdmin)
                return Redirect("/logout");

            if (!ModelState.IsValid)
                return new BadRequestObjectResult(ModelState);
            else
            {
                try
                {
                    var result = await _ticketTransactionService.Rollback(id);

                    return new OkObjectResult(new GenericResult(result.Success, result.Message));
                }
                catch (Exception ex)
                {
                    return new OkObjectResult(new GenericResult(false, ex.Message));
                }
            }
        }

        [HttpPost]
        public IActionResult LockTicket(int id)
        {
            if (!IsAdmin)
                return Redirect("/logout");

            if (!ModelState.IsValid)
                return new BadRequestObjectResult(ModelState);
            else
            {
                try
                {
                    var result = _ticketTransactionService.Locked(id);
                    return new OkObjectResult(new GenericResult(result.Success, result.Message));
                }
                catch (Exception ex)
                {
                    return new OkObjectResult(new GenericResult(false, ex.Message));
                }
            }
        }

        [HttpGet]
        public IActionResult GetAllTodayPaging()
        {
            var model = _ticketTransactionService.GetAllPaging(string.Empty, "", 0, true, 1, 9999);
            return new OkObjectResult(model);
        }
    }
}
