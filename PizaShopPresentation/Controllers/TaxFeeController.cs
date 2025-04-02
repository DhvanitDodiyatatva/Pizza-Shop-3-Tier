using Microsoft.AspNetCore.Mvc;
using PizzaShopRepository.Services;
using PizzaShopRepository.ViewModels;
using PizzaShopServices.Attributes;
using System;
using System.Linq;

namespace PizzaShopRepository.Controllers
{
    // [CustomAuthorize("super_admin, chef, account_manager")]
    [CustomAuthorize("TaxAndFee", PermissionType.View, "super_admin", "account_manager")]
    public class TaxFeeController : Controller
    {
        private readonly ITaxFeeService _taxFeeService;

        public TaxFeeController(ITaxFeeService taxFeeService)
        {
            _taxFeeService = taxFeeService;
        }

        // GET: TaxFee/TaxesFees
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
        public IActionResult AddNewTaxFee()
        {
            return PartialView("_AddTaxFee", new TaxFeeAddEditViewModel());
        }

        // POST: TaxFee/AddNewTaxFee (Handles the form submission via AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddNewTaxFee(TaxFeeAddEditViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToArray();
                    return Json(new { success = false, message = "Validation failed: " + string.Join(" ", errors) });
                }

                _taxFeeService.AddTaxFee(model);
                return Json(new { success = true, message = "Tax/Fee added successfully!" });
            }
            catch (Exception ex)
            {
                // Log the full exception details
                string errorMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMessage += $" Inner Exception: {ex.InnerException.Message}";
                }
                Console.WriteLine($"Error adding tax/fee: {errorMessage}\nStackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = errorMessage });
            }
        }

        // GET: TaxFee/EditTaxFee/5 (Returns the partial view for the "Edit Tax" modal)
        [HttpGet]
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
        [ValidateAntiForgeryToken]
        public IActionResult EditTaxFee(TaxFeeAddEditViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToArray();
                    return Json(new { success = false, message = "Validation failed: " + string.Join(" ", errors) });
                }

                _taxFeeService.UpdateTaxFee(model);
                return Json(new { success = true, message = "Tax/Fee updated successfully!" });
            }
            catch (Exception ex)
            {
                // Log the full exception details
                string errorMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMessage += $" Inner Exception: {ex.InnerException.Message}";
                }
                Console.WriteLine($"Error updating tax/fee: {errorMessage}\nStackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = errorMessage });
            }
        }

        // POST: TaxFee/DeleteTaxFee/5 (Handles the delete operation via AJAX)
        [HttpPost]
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
    }
}