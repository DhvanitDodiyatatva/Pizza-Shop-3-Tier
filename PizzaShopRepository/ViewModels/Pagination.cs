using PizzaShopRepository.Models;

namespace PizzaShopRepository.ViewModels
{
    public class PaginatedItemViewModel
    {
        public List<Item> Items { get; set; }
        public int TotalItems { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
    }
}


