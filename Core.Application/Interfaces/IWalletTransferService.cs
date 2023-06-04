using Core.Application.ViewModels.System;
using Core.Data.Entities;
using Core.Data.Enums;
using Core.Utilities.Dtos;
using System.Collections.Generic;
using System.Linq;

namespace Core.Application.Interfaces
{
    public interface IWalletTransferService
    {
        PagedResult<WalletTransferViewModel> GetAllPaging(string keyword, int pageIndex, int pageSize);

        void Add(WalletTransferViewModel Model);

        void Update(WalletTransfer model);

        void Save();

        List<WalletTransfer> GetNotRecall(int take = 1);

        decimal GetTotalTransactionAmount();
    }
}
