using PizzaShopRepository.Models;

namespace PizzaShopRepository.ViewModels
{
    public class PagedItemViewModel
    {
        public List<Item> Items { get; set; } = new List<Item>();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int CategoryId { get; set; } // To track the current category
    }

    public class PagedModifierViewModel
    {
        public List<Modifier> Modifiers { get; set; } = new List<Modifier>();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalModifiers { get; set; }
        public int ModifierGroupId { get; set; } // To track the current modifier group
    }


}


