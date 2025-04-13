using PizzaShopRepository.ViewModels;

namespace PizzaShopServices.Interfaces
{
    public interface IWaitingTokenService
    {
        Task<(bool Success, string Message)> AddWaitingTokenAsync(WaitingTokenViewModel model);
    }
}