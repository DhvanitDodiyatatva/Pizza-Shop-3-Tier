// PizzaShopPresentation/Controllers/OrdersController.cs
using Microsoft.AspNetCore.Mvc;
using PizzaShopRepository.ViewModels;
using PizzaShopService.Interfaces;

namespace PizzaShopPresentation.Controllers;

public class OrdersController : Controller
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetOrderList(string searchQuery, string statusFilter, string timeFilter, string fromDate, string toDate, string sortColumn, string sortDirection, int page = 1, int pageSize = 5)
    {
        var viewModel = await _orderService.GetOrdersAsync(searchQuery, statusFilter, timeFilter, fromDate, toDate, sortColumn, sortDirection, page, pageSize);
        return PartialView("_OrderList", viewModel);
    }
}