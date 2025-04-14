using PizzaShopRepository.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PizzaShopRepository.Interfaces
{
    public interface IKotRepository
    {
        Task<List<KotViewModel>> GetKotDataAsync(string status, int categoryId);
    }
}