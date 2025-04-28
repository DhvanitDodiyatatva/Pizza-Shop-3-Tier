namespace PizzaShopRepository.ViewModels
{
    public class CustomerDetailsViewModel
    {
        public int SectionId { get; set; }
        public string SectionName { get; set; } = null!;
        public List<TableDetailsViewModel> SelectedTables { get; set; } = new List<TableDetailsViewModel>();
        public List<WaitingTokensViewModel> WaitingTokens { get; set; } = new List<WaitingTokensViewModel>();
    }

    public class WaitingTokensViewModel
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = null!;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public int NumOfPersons { get; set; }
    }

    public class TableDetailsViewModel
    {
        public int TableId { get; set; }
        public string TableName { get; set; } = null!;
        public int Capacity { get; set; }
        public string Availability { get; set; } = null!;
    }
}