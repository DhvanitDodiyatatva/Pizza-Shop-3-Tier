namespace PizzaShopRepository.ViewModels
{
    public class SectionDetailsViewModel
    {
        public int FloorId { get; set; }
        public string FloorName { get; set; }
        public List<TableDetailViewModel> TableDetails { get; set; } = new List<TableDetailViewModel>();
    }

    public class TableDetailViewModel
    {
        public int TableId { get; set; }
        public string TableName { get; set; }
        public int Capacity { get; set; }
        public string Availability { get; set; }  // Will map to Available, Running, Assigned
    }
}