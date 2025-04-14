using PizzaShopRepository.Interfaces;
using PizzaShopRepository.ViewModels;
using PizzaShopServices.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PizzaShopServices.Implementations
{
    public class KotService : IKotService
    {
        private readonly IKotRepository _kotRepository;

        public KotService(IKotRepository kotRepository)
        {
            _kotRepository = kotRepository;
        }

        public async Task<List<KotViewModel>> GetKotDataAsync(string status, int categoryId)
        {
            var data = await _kotRepository.GetKotDataAsync(status, categoryId);
            Console.WriteLine($"KotService returning {data.Count} items.");
            return data;
        }
    }
}