using Microsoft.AspNetCore.Mvc;

namespace PizaShopPresentation.Controllers;

public class OrderAppController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
