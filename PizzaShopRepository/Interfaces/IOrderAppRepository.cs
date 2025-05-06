using PizzaShopRepository.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PizzaShopRepository.Interfaces
{
    public interface IOrderAppRepository
    {
        Task<Customer> GetCustomerByEmailAsync(string email);
        Task AddCustomerAsync(Customer customer);
        Task<WaitingToken> GetWaitingTokenByIdAsync(int waitingTokenId);
        Task UpdateWaitingTokenAsync(WaitingToken waitingToken);
        Task AddOrderAsync(Order order);
        Task AddOrderTableAsync(OrderTable orderTable);
        Task<Table> GetTableByIdAsync(int tableId);
        Task UpdateTableAsync(Table table);
        Task<List<Table>> GetTablesByIdsAsync(int[] tableIds);

        Task<Order?> GetOrderByIdAsync(int id);

        Task<OrderTable?> GetOrderTableByTableIdAsync(int tableId);
        Task<Item?> GetItemWithModifiersAsync(int itemId);
    }
}