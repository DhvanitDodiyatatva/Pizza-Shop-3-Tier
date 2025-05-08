using PizzaShopRepository.Models;

namespace PizzaShopRepository.ViewModels
{
    public class SaveOrderViewModel
    {
        public int OrderId { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; }
        public List<CartItemViewModel> CartItems { get; set; }
        public Dictionary<string, bool> TaxSettings { get; set; }
    }

    public class CartItemViewModel
    {
        public int ItemId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public List<CartModifierViewModel> Modifiers { get; set; }
    }

    public class CartModifierViewModel
    {
        public int ModifierId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}