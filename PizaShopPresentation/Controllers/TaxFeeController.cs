using Microsoft.AspNetCore.Mvc;
using PizzaShopRepository.Services;
using PizzaShopRepository.ViewModels;
using PizzaShopServices.Attributes;
using System;
using System.Linq;

namespace PizzaShopRepository.Controllers
{
    // [CustomAuthorize("super_admin, chef, account_manager")]
    public class TaxFeeController : Controller
    {
        private readonly ITaxFeeService _taxFeeService;

        public TaxFeeController(ITaxFeeService taxFeeService)
        {
            _taxFeeService = taxFeeService;
        }

        // GET: TaxFee/TaxesFees
        [CustomAuthorize("TaxAndFee", PermissionType.View, "super_admin", "account_manager")]
        public IActionResult TaxesFees()
        {
            return View();
        }

        // GET: TaxFee/GetTaxesFees (Returns the partial view for the tax list)
        [HttpGet]
        public IActionResult GetTaxesFees(string searchQuery, int page = 1, int pageSize = 5)
        {
            ViewBag.SearchQuery = searchQuery;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;

            int totalRecords;
            var taxesFees = _taxFeeService.GetAllTaxesFees(searchQuery, page, pageSize, out totalRecords);

            ViewBag.TotalTaxesFees = totalRecords;

            return PartialView("_TaxFeeListPartial", taxesFees);
        }

        // GET: TaxFee/AddNewTaxFee (Returns the partial view for the "Add New Tax" modal)
        [HttpGet]
        [CustomAuthorize("TaxAndFee", PermissionType.Alter, "super_admin", "account_manager")]
        public IActionResult AddNewTaxFee()
        {
            return PartialView("_AddTaxFee", new TaxFeeAddEditViewModel());
        }


        [HttpPost]
        [CustomAuthorize("TaxAndFee", PermissionType.Alter, "super_admin", "account_manager")]
        [ValidateAntiForgeryToken]
        public IActionResult AddNewTaxFee(TaxFeeAddEditViewModel model)
        {
            try
            {
                _taxFeeService.AddTaxFee(model);
                return Json(new { success = true, message = "Tax/fee added successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        [HttpGet]
        [CustomAuthorize("TaxAndFee", PermissionType.Alter, "super_admin", "account_manager")]
        public IActionResult EditTaxFee(int id)
        {
            var taxFee = _taxFeeService.GetTaxFeeById(id);
            if (taxFee == null)
            {
                return NotFound("Tax/Fee not found.");
            }
            return PartialView("_EditTaxFee", taxFee);
        }

        [HttpPost]
        [CustomAuthorize("TaxAndFee", PermissionType.Alter, "super_admin", "account_manager")]
        [ValidateAntiForgeryToken]
        public IActionResult EditTaxFee(TaxFeeAddEditViewModel model)
        {
            try
            {
                _taxFeeService.UpdateTaxFee(model);
                return Json(new { success = true, message = "Tax/fee updated successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: TaxFee/DeleteTaxFee
        [HttpPost]
        [CustomAuthorize("TaxAndFee", PermissionType.Delete, "super_admin", "account_manager")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteTaxFee(int id)
        {
            try
            {
                _taxFeeService.DeleteTaxFee(id);
                return Json(new { success = true, message = "Tax/Fee deleted successfully!" });
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMessage += $" Inner Exception: {ex.InnerException.Message}";
                }
                Console.WriteLine($"Error deleting tax/fee: {errorMessage}\nStackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = errorMessage });
            }
        }


        // POST: TaxFee/ToggleTaxFeeEnabled
        [HttpPost]
        [CustomAuthorize("TaxAndFee", PermissionType.Alter, "super_admin", "account_manager")]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleTaxFeeEnabled(int id, bool isEnabled)
        {
            try
            {
                var taxFee = _taxFeeService.GetTaxFeeById(id);
                if (taxFee == null)
                {
                    return Json(new { success = false, message = "Tax/Fee not found." });
                }

                taxFee.IsEnabled = isEnabled;
                _taxFeeService.UpdateTaxFee(taxFee);
                return Json(new { success = true, message = $"Tax {(isEnabled ? "enabled" : "disabled")} successfully!" });
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMessage += $" Inner Exception: {ex.InnerException.Message}";
                }
                Console.WriteLine($"Error toggling tax enabled status: {errorMessage}\nStackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = errorMessage });
            }
        }
    }
}