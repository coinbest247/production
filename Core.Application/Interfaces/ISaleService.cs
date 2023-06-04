using Core.Application.ViewModels.BlockChain;
using Core.Application.ViewModels.Dapp;
using Core.Utilities.Dtos;
using System.Threading.Tasks;

namespace Core.Application.Interfaces
{
    public interface ISaleService
    {
        #region Sale Defi
        PagedResult<SaleDefiViewModel> GetAllPaging(string keyword, string address , int pageIndex, int pageSize);

        Task<(GenericResult result, string transactionId)> InitializeTransactionProgress(
            SaleInitializationParams model );
        Task<GenericResult> ProcessVerificationBNBTransaction(
            string transactionHash);

        string GetLatestTransactionByWalletAddress(string dappTxnHash);
        #endregion

        #region Sale Manual
        Task<GenericResult> ProcessBuyToken(SaleManualViewModel model, string userId);
        #endregion
    }
}
