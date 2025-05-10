using PizzaShopRepository.Models;

namespace PizzaShopRepository.Interfaces
{
    public interface IWaitingTokenRepository
    {
        Task AddWaitingTokenAsync(WaitingToken waitingToken);
        Task AddCustomerAsync(Customer customer);
        Task<List<WaitingToken>> GetAllWaitingTokensAsync();
        Task<Customer> GetCustomerByEmailAsync(string email);
        Task<WaitingToken> GetWaitingTokenByIdAsync(int id);
        Task UpdateWaitingTokenAsync(WaitingToken waitingToken);
    }
}