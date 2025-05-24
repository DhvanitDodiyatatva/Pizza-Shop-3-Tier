using System;
using System.Collections.Generic;

namespace PizzaShopRepository.ViewModels
{
    public class KotOrderViewModel
    {
        public int Id { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string OrderInstructions { get; set; }
        public List<KotOrderItemViewModel> OrderItems { get; set; } = new List<KotOrderItemViewModel>();
        public List<KotOrderTableViewModel> OrderTables { get; set; } = new List<KotOrderTableViewModel>();
    }

    public class KotOrderItemViewModel
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public int ReadyQuantity { get; set; }
        public string SpecialInstructions { get; set; }
        public string ItemName { get; set; }
        public string CategoryName { get; set; }
        public List<string> ModifierNames { get; set; } = new List<string>();
    }

    public class KotOrderTableViewModel
    {
        public string TableName { get; set; }
        public string SectionName { get; set; }
    }

    // Update existing UpdateOrderItemStatusViewModel to match required fields
    public class UpdateOrderItemStatusViewModel
    {
        public int OrderId { get; set; }
        public List<OrderItemDetail> OrderItems { get; set; } = new List<OrderItemDetail>();
    }

    public class OrderItemDetail
    {
        public int OrderItemId { get; set; }
        public string ItemName { get; set; }
        public int Quantity { get; set; }
        public int AdjustedQuantity { get; set; }
        public List<string> Modifiers { get; set; } = new List<string>();
        public bool IsSelected { get; set; }
    }
}