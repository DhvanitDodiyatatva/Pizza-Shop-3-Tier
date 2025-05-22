namespace PizzaShopRepository.ViewModels
{
    public class ChartDataViewModel
    {
        public string Date { get; set; } // Date as string in "yyyy-MM-dd" format
        public decimal Value { get; set; } // Used for Revenue or CustomerCount
    }

    public class SellingItemViewModel
    {
        public string ItemName { get; set; }
        public int Quantity { get; set; }
        public int OrderCount { get; set; }
    }
}