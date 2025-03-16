using Microsoft.AspNetCore.Mvc;

public class ErrorController : Controller
{
    [Route("Error/404")]
    public IActionResult PageNotFound()
    {
        return View();
    }

    public IActionResult AccessDenied()
    {
        return View();
    }
}
