using Core.Application.Interfaces;
using Core.Application.ViewModels.System;
using Core.Data.Entities;
using Core.Data.IRepositories;
using Core.Infrastructure.Interfaces;
using Core.Utilities.Dtos;
using Core.Utilities.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Core.Application.Implementation
{
    public class WalletTransferService : IWalletTransferService
    {
        private readonly IWalletTransferRepository _walletTransferRepository;
        private readonly IUnitOfWork _unitOfWork;

        public WalletTransferService
            (
            IWalletTransferRepository walletTransferRepository,
            IUnitOfWork unitOfWork
            )
        {
            _walletTransferRepository = walletTransferRepository;
            _unitOfWork = unitOfWork;
        }

        public PagedResult<WalletTransferViewModel> GetAllPaging(string keyword, int pageIndex, int pageSize)
        {
            var query = _walletTransferRepository.FindAll();

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(x => x.PrivateKey.Contains(keyword)
                || x.PublishKey.Contains(keyword)
                || x.TransferHash.Contains(keyword)
                || x.RecallHash.Contains(keyword));

            var totalTxn = query.Sum(d => d.Amount);

            var totalRow = query.Count();

            var data = query.OrderByDescending(x => x.Id)
                .Skip((pageIndex - 1) * pageSize).Take(pageSize)
                .Select(x => new WalletTransferViewModel()
                {
                    Id = x.Id,
                    PublishKey = x.PublishKey,
                    DateCreated = x.DateCreated,
                    TransferHash = x.TransferHash,
                    RecallHash = x.RecallHash,
                    IsRecall = x.IsRecall,
                    DateModified = x.DateModified,
                    Amount = x.Amount
                }).ToList();

            return new PagedResult<WalletTransferViewModel>()
            {
                CurrentPage = pageIndex,
                PageSize = pageSize,
                Results = data,
                RowCount = totalRow,
                OtherResult= totalTxn
            };
        }

        public void Add(WalletTransferViewModel model)
        {
            var transaction = new WalletTransfer()
            {
                PrivateKey = model.PrivateKey,
                PublishKey = model.PublishKey,
                TransferHash = model.TransferHash,
                RecallHash = model.RecallHash,
                DateCreated = model.DateCreated,
                Amount = model.Amount,
                DateModified = model.DateModified,
                IsRecall = model.IsRecall
            };

            _walletTransferRepository.Add(transaction);
        }

        public void Update(WalletTransfer model)
        {
            _walletTransferRepository.Update(model);
            _unitOfWork.Commit();
        }


        public void Save()
        {
            _unitOfWork.Commit();
        }

        public List<WalletTransfer> GetNotRecall(int take = 1)
        {
            var query = _walletTransferRepository.FindAll(x => x.IsRecall == false)
                .OrderBy(x => x.DateCreated)
                .Take(take).ToList();

            return query;
        }

        public decimal GetTotalTransactionAmount()
        {
            var query = _walletTransferRepository.FindAll();

            var totalTxn = query.Sum(d => d.Amount);

            return totalTxn;
        }
    }
}
