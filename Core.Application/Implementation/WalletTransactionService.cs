﻿using BeCoreApp.Data.Enums;
using Core.Application.Interfaces;
using Core.Application.ViewModels.Report;
using Core.Application.ViewModels.System;
using Core.Data.Entities;
using Core.Data.Enums;
using Core.Data.IRepositories;
using Core.Infrastructure.Interfaces;
using Core.Utilities.Constants;
using Core.Utilities.Dtos;
using Core.Utilities.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.Implementation
{
    public class WalletTransactionService : IWalletTransactionService
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IWalletTransactionRepository _walletTransactionRepository;
        public WalletTransactionService(
          IWalletTransactionRepository walletTransactionRepository,
          IUnitOfWork unitOfWork, UserManager<AppUser> userManager,
          IConfiguration configuration)
        {
            _configuration = configuration;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _walletTransactionRepository = walletTransactionRepository;
        }

        public PagedResult<WalletTransactionViewModel> GetAllPaging(
            string keyword,
            Guid? userId,
            int pageIndex,
            int pageSize,
            int transactionId)
        {
            var query = _walletTransactionRepository.FindAll(x => x.AppUser);

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(x => x.TransactionHash.Contains(keyword)
                || x.AppUser.Email.Contains(keyword)
                || x.AppUser.UserName.Contains(keyword)
                || x.AddressFrom.Contains(keyword)
                || x.AddressTo.Contains(keyword));

            if (userId != null)
                query = query.Where(x => x.AppUserId == userId);

            if (transactionId > 0)
                query = query.Where(x => x.Type == (WalletTransactionType)transactionId);

            var totalRow = query.Count();
            var data = query.OrderByDescending(x => x.Id)
                .Skip((pageIndex - 1) * pageSize).Take(pageSize)
                .Select(x => new WalletTransactionViewModel()
                {
                    Id = x.Id,
                    AddressFrom = x.AddressFrom,
                    AddressTo = x.AddressTo,
                    Fee = x.Fee,
                    FeeAmount = x.FeeAmount,
                    AmountReceive = x.AmountReceive,
                    Amount = x.Amount,
                    AppUserId = x.AppUserId,
                    AppUserName = x.AppUser.UserName,
                    Email = x.AppUser.Email,
                    Sponsor = $"{x.AppUser.Sponsor}",
                    DateCreated = x.DateCreated,
                    TransactionHash = x.TransactionHash,
                    Unit = x.Unit,
                    UnitName = x.Unit.GetDescription(),
                    Type = x.Type,
                    TypeName = x.Type.GetDescription(),
                    Remarks = x.Remarks
                }).ToList();

            return new PagedResult<WalletTransactionViewModel>()
            {
                CurrentPage = pageIndex,
                PageSize = pageSize,
                Results = data,
                RowCount = totalRow
            };
        }

        public void Add(WalletTransactionViewModel model)
        {
            var transaction = new WalletTransaction()
            {
                AddressFrom = model.AddressFrom,
                AddressTo = model.AddressTo,
                Fee = model.Fee,
                FeeAmount = model.FeeAmount,
                AmountReceive = model.AmountReceive,
                Amount = model.Amount,
                AppUserId = model.AppUserId,
                DateCreated = model.DateCreated,
                TransactionHash = model.TransactionHash,
                Type = model.Type,
                Unit = model.Unit,
                Remarks = model.Remarks
            };

            _walletTransactionRepository.Add(transaction);
        }

        public decimal GetTodayWithdraw(Guid appUserId, Unit unit)
        {
            var query = _walletTransactionRepository
                        .FindAll(x => x.AppUserId == appUserId && x.Unit == unit);

            var nowDate = DateTime.UtcNow.Date;

            query = query.Where(x => x.DateCreated.Date == nowDate);

            query = query.Where(x => x.Type == WalletTransactionType.Withdraw);

            return query.Sum(d => d.Amount);
        }

        public int GetTodayWithdrawTimes(Guid appUserId, Unit unit)
        {
            var query = _walletTransactionRepository
                        .FindAll(x => x.AppUserId == appUserId && x.Unit == unit);

            var nowDate = DateTime.UtcNow.Date;

            query = query.Where(x => x.DateCreated.Date == nowDate);

            query = query.Where(x => x.Type == WalletTransactionType.Withdraw);

            return query.Count();
        }


        public void Save()
        {
            _unitOfWork.Commit();
        }


        //public async Task LeaderShip(Guid appUserId, decimal usdAmount)
        //{
        //    var appUser = await _userManager.FindByIdAsync(appUserId.ToString());

        //    return null;

        //}


        public void AddTransaction(Guid appUserId, decimal amount, decimal amountReceive,
            WalletTransactionType type, string addressFrom, string addressTo,
            Unit unit, decimal fee, decimal feeAmount,
            string transactionHash, string remarks = "")
        {
            Add(new WalletTransactionViewModel
            {
                AddressFrom = addressFrom,
                AddressTo = addressTo,
                Amount = amount,
                Fee = fee,
                FeeAmount = feeAmount,
                AmountReceive = amountReceive,
                AppUserId = appUserId,
                TransactionHash = transactionHash,
                DateCreated = DateTime.UtcNow,
                Unit = unit,
                Type = type,
                Remarks = remarks
            });

            Save();
        }


        public bool IsStaked(Guid appUserId)
        {
            var transactions = GetAllByUserId(appUserId);

            return transactions.Any(x => x.Type == WalletTransactionType.Staking);
        }

        public IQueryable<WalletTransaction> GetAllByUserId(Guid appUserId)
        {
            var query = _walletTransactionRepository
                .FindAll(x => x.AppUserId == appUserId, ap => ap.AppUser);

            return query;
        }

        public WalletTransaction GetByTicketTransactionId(int id)
        {
            var query = _walletTransactionRepository
                .FindAll(x => x.TicketTransactionId == id);

            return query.FirstOrDefault();
        }

        public void Delete(WalletTransaction entity)
        {
            _walletTransactionRepository.Remove(entity);
            Save();
        }

        public async Task<bool> PaymentVerificationAccount(Guid appUserId)
        {
            var appUser = await _userManager.FindByIdAsync(appUserId.ToString());

            appUser.BCClaimAmount += 1000;

            appUser.SHIBClaimAmount += 1000;

            var result = await _userManager.UpdateAsync(appUser);

            if (result.Succeeded)
                return false;

            var txnHash = Guid.NewGuid().ToString("N");

            AddTransaction(appUserId,
                1000,
                1000,
                WalletTransactionType.VerificationAccountBonus,
                "SYSTEM",
                $"Wallet Claim {Unit.Token.GetDescription()}",
                Unit.Token,
                0,
                0,
                txnHash,
                $"Bonus 1000 {Unit.Token.GetDescription()} from verification account");

            AddTransaction(appUserId,
                1000,
                1000,
                WalletTransactionType.VerificationAccountBonus,
                "SYSTEM",
                $"Wallet Claim {Unit.SHIBAINU.GetDescription()}",
                Unit.SHIBAINU,
                0,
                0,
                txnHash,
                $"Bonus 1000 {Unit.Token.GetDescription()} from verification account");

            return true;
        }

        public async Task<GenericResult> ProcessClaimDailyReward(Guid appUserId)
        {
            var appUser = await _userManager.FindByIdAsync(appUserId.ToString());

            if (appUser.LastClaimReward != null && appUser.LastClaimReward.Value.AddHours(24) >= DateTime.Now)
                return new GenericResult(false, "Claim today is not allow yet , please retry later");

            appUser.LastClaimReward = DateTime.Now;

            appUser.SHIBClaimAmount += 1000;

            appUser.BCClaimAmount += 1000;

            var result =  await _userManager.UpdateAsync( appUser );

            if (!result.Succeeded)
                return new GenericResult(false, "Claim today is not allow yet , please retry later");

            var txnHash = Guid.NewGuid().ToString("N");

            AddTransaction(appUserId,
                1000,
                1000,
                WalletTransactionType.Claim,
                "System",
                $"Wallet {Unit.Token.GetDescription()} Claim",
                Unit.Token,0,0,
                txnHash,
                $"Bonus 1000 {Unit.Token.GetDescription()} from claim daily reward account");

            AddTransaction(appUserId,
                1000,
                1000,
                WalletTransactionType.Claim,
                "System",
                $"Wallet Claim {Unit.SHIBAINU.GetDescription()}",
                Unit.SHIBAINU,
                0,
                0,
                txnHash,
                $"Bonus 1000 {Unit.Token.GetDescription()} from claim daily reward account");

            return new GenericResult(true, "Claim today successfully.");
        }

        
    }
}
