using PizzaShopRepository.Models;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace PizzaShopServices.Interfaces
{
    public interface IKotService
    {
        Task<List<Order>> GetOrdersByCategoryAndStatusAsync(string categoryName, string status);
    }
}