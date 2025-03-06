namespace PizzaShopRepository.ViewModels
{
    public class MenuViewModel
    {
        public List<CategoryViewModel> Categories { get; set; } = new List<CategoryViewModel>();
        public List<ItemVMViewModel> Items { get; set; } = new List<ItemVMViewModel>();
    }
}