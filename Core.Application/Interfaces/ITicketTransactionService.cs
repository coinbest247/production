using Core.Application.ViewModels.System;
using Core.Utilities.Dtos;
using System;
using System.Threading.Tasks;

namespace Core.Application.Interfaces
{
    public interface ITicketTransactionService
    {
        PagedResult<TicketTransactionViewModel> GetAllPaging(
            string keyword, string username, int transactionStatus, bool isToday, int pageIndex, int pageSize);

        int Add(TicketTransactionViewModel Model);

        Task<GenericResult> Rejected(int id);

        GenericResult Locked(int id);
        Task<GenericResult> Approved(int id);

        void Save();

        Task<bool> IsPrimaryWalletEnoughBalanceAsync(int ticketTransactionId);

        bool IsValidDailyWithdrawAmount(int ticketTransactionId);

        bool IsValidDailyWithdrawTimes(int ticketTransactionId);

        bool IsAnyPendingWithdraw(int ticketTransactionId);

        decimal GetTotalPendingWithdrawUSDT(Guid userId);

        decimal GetTodayTotalCompletedWithdrawUSDT(Guid userId);

        Task<GenericResult> Rollback(int id);
    }
}
