using System.Collections.Generic;

namespace PizzaShopRepository.ViewModels
{
    public class UpdateOrderItemStatusViewModel
    {
        public int OrderId { get; set; }
        public List<OrderItemDetail> OrderItems { get; set; } = new List<OrderItemDetail>();
    }

    public class OrderItemDetail
    {
        public int OrderItemId { get; set; }
        public string? ItemName { get; set; }
        public int Quantity { get; set; }
        public int ReadyQuantity { get; set; }
        public string? Status { get; set; }
        public List<string> Modifiers { get; set; } = new List<string>();
        public bool IsSelected { get; set; } // Checkbox to select item for status update
        public int AdjustedQuantity { get; set; } // Adjusted quantity from modal
        public string? CategoryName { get; set; }
    }
}