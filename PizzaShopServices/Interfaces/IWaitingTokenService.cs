using PizzaShopRepository.Models;
using PizzaShopRepository.ViewModels;

namespace PizzaShopServices.Interfaces
{
    public interface IWaitingTokenService
    {
        Task<(bool Success, string Message)> AddWaitingTokenAsync(WaitingTokenViewModel model);
        // Task<List<WaitingToken>> GetAllWaitingTokensAsync();
        Task<List<WaitingTokenListViewModel>> GetAllWaitingTokensAsync();
        Task<WaitingToken> GetWaitingTokenByIdAsync(int id);
        Task<(bool Success, string Message)> UpdateWaitingTokenAsync(WaitingTokenViewModel model);
        Task<bool> IsEmailExistsAsync(string email, int excludeWaitingTokenId);
        Task<(bool Success, string Message)> DeleteWaitingTokenAsync(int id);
    }
}