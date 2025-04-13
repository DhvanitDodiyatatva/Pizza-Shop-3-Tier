using PizzaShopRepository.Models;

namespace PizzaShopRepository.Interfaces
{
    public interface IWaitingTokenRepository
    {
        Task AddWaitingTokenAsync(WaitingToken waitingToken);
        Task AddCustomerAsync(Customer customer);
    }
}