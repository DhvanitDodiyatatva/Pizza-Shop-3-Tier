// using Microsoft.AspNetCore.Mvc;

// namespace PizaShopPresentation.Controllers;


// public class OrderAppController : Controller
// {
//     public IActionResult Table()
//     {
//         return View();
//     }
// }
using Microsoft.AspNetCore.Mvc;
using PizzaShopServices.Interfaces;
using PizzaShopRepository.ViewModels;

namespace PizzaShop.Controllers
{
    public class OrderAppController : Controller
    {
        private readonly ISectionService _sectionService;
        private readonly ITableService _tableService;

        public OrderAppController(ISectionService sectionService, ITableService tableService)
        {
            _sectionService = sectionService;
            _tableService = tableService;
        }

        public async Task<IActionResult> Table()
        {
            var sections = await _sectionService.GetAllSectionsAsync();
            var viewModel = new List<SectionDetailsViewModel>();

            foreach (var section in sections)
            {
                var tables = await _tableService.GetTablesBySectionAsync(section.Id);
                var sectionViewModel = new SectionDetailsViewModel
                {
                    FloorId = section.Id,
                    FloorName = section.Name,
                    TableDetails = tables.Select(t => new TableDetailViewModel
                    {
                        TableId = t.Id,
                        TableName = t.Name,
                        Capacity = t.Capacity,
                        Availability = t.Status switch
                        {
                            "available" => "Available",
                            "occupied" => "Running",
                            "reserved" => "Assigned",
                            _ => "Available"
                        }
                    }).ToList()
                };
                viewModel.Add(sectionViewModel);
            }

            return View(viewModel);
        }
    }
}