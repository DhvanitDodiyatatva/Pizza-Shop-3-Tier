using PizzaShopRepository.Models;

namespace PizzaShopRepository.ViewModels
{
    public class CustomerHistoryViewModel
    {
        public Customer Customer { get; set; }
        public List<Order> Orders { get; set; }
    }
}