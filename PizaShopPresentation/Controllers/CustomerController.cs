using Microsoft.AspNetCore.Mvc;
using PizzaShopRepository.Models;
using PizzaShopRepository.Services;

namespace PizaShopPresentation.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ICustomerService _customerService;

        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetCustomerList(string searchQuery, string sortColumn = "Name", string sortDirection = "asc", int page = 1, int pageSize = 5)
        {
            var viewModel = await _customerService.GetCustomersAsync(searchQuery, sortColumn, sortDirection, page, pageSize);
            return PartialView("_CustomerList", viewModel);
        }
    }
}