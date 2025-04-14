using PizzaShopRepository.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PizzaShopServices.Interfaces
{
    public interface IKotService
    {
        Task<List<KotViewModel>> GetKotDataAsync(string status, int categoryId);
    }
}