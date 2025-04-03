using Microsoft.AspNetCore.Mvc;
using PizzaShopRepository.ViewModels;
using PizzaShopServices.Interfaces;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Drawing;
using PizzaShopRepository.Services;
using PizzaShopServices.Attributes;

namespace PizaShopPresentation.Controllers
{
     [CustomAuthorize("Customers", PermissionType.View, "super_admin", "account_manager")]
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
        public async Task<IActionResult> GetCustomerList(string searchQuery, string sortColumn = "Name",
            string sortDirection = "asc", int page = 1, int pageSize = 5,
            string timeFilter = "", string fromDate = "", string toDate = "")
        {
            var viewModel = await _customerService.GetCustomersAsync(searchQuery, sortColumn, sortDirection,
                page, pageSize, timeFilter, fromDate, toDate);
            return PartialView("_CustomerList", viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetCustomerHistory(int customerId)
        {
            var viewModel = await _customerService.GetCustomerHistoryAsync(customerId);

            if (viewModel == null)
                return NotFound();

            return PartialView("_CustomerHistoryModal", viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> ExportCustomers(string searchQuery, string timeFilter,
            string fromDate, string toDate, string sortColumn, string sortDirection)
        {
            // Fetch all customers without pagination for export
            var viewModel = await _customerService.GetCustomersAsync(searchQuery, sortColumn, sortDirection,
                page: 1, pageSize: int.MaxValue, timeFilter, fromDate, toDate);

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Customers");
                var currentRow = 3;
                var currentCol = 2;

                // First Row: Account and Search Text
                // Account Label
                worksheet.Cells[currentRow, currentCol, currentRow + 1, currentCol + 1].Merge = true;
                worksheet.Cells[currentRow, currentCol].Value = "Account: ";
                using (var headingCells = worksheet.Cells[currentRow, currentCol, currentRow + 1, currentCol + 1])
                {
                    headingCells.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    headingCells.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#0066A7"));
                    headingCells.Style.Font.Bold = true;
                    headingCells.Style.Font.Color.SetColor(Color.White);
                    headingCells.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);
                    headingCells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    headingCells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                }

                // Account Value (leaving it empty as in the image)
                currentCol += 2;
                worksheet.Cells[currentRow, currentCol, currentRow + 1, currentCol + 3].Merge = true;
                worksheet.Cells[currentRow, currentCol].Value = "";
                using (var headingCells = worksheet.Cells[currentRow, currentCol, currentRow + 1, currentCol + 3])
                {
                    headingCells.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    headingCells.Style.Fill.BackgroundColor.SetColor(Color.White);
                    headingCells.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);
                    headingCells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    headingCells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                }

                // Search Text Label
                currentCol += 5;
                worksheet.Cells[currentRow, currentCol, currentRow + 1, currentCol + 1].Merge = true;
                worksheet.Cells[currentRow, currentCol].Value = "Search Text: ";
                using (var headingCells = worksheet.Cells[currentRow, currentCol, currentRow + 1, currentCol + 1])
                {
                    headingCells.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    headingCells.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#0066A7"));
                    headingCells.Style.Font.Bold = true;
                    headingCells.Style.Font.Color.SetColor(Color.White);
                    headingCells.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);
                    headingCells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    headingCells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                }

                // Search Text Value
                currentCol += 2;
                worksheet.Cells[currentRow, currentCol, currentRow + 1, currentCol + 3].Merge = true;
                worksheet.Cells[currentRow, currentCol].Value = string.IsNullOrEmpty(searchQuery) ? "" : searchQuery;
                using (var headingCells = worksheet.Cells[currentRow, currentCol, currentRow + 1, currentCol + 3])
                {
                    headingCells.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    headingCells.Style.Fill.BackgroundColor.SetColor(Color.White);
                    headingCells.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);
                    headingCells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    headingCells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                }

                // Logo
                currentCol += 5;
                worksheet.Cells[currentRow, currentCol, currentRow + 4, currentCol + 1].Merge = true;
                var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "logos", "pizzashop_logo.png");

                if (System.IO.File.Exists(imagePath))
                {
                    var picture = worksheet.Drawings.AddPicture("Image", new FileInfo(imagePath));
                    picture.SetPosition(currentRow - 1, 1, currentCol - 1, 1);
                    picture.SetSize(125, 95);
                }
                else
                {
                    worksheet.Cells[currentRow, currentCol].Value = "Image not found";
                }

                // Second Row: Date and No. of Records
                currentRow += 3;
                currentCol = 2;

                // Date Label
                worksheet.Cells[currentRow, currentCol, currentRow + 1, currentCol + 1].Merge = true;
                worksheet.Cells[currentRow, currentCol].Value = "Date: ";
                using (var headingCells = worksheet.Cells[currentRow, currentCol, currentRow + 1, currentCol + 1])
                {
                    headingCells.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    headingCells.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#0066A7"));
                    headingCells.Style.Font.Bold = true;
                    headingCells.Style.Font.Color.SetColor(Color.White);
                    headingCells.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);
                    headingCells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    headingCells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                }

                // Date Value
                currentCol += 2;
                worksheet.Cells[currentRow, currentCol, currentRow + 1, currentCol + 3].Merge = true;
                worksheet.Cells[currentRow, currentCol].Value = string.IsNullOrEmpty(timeFilter) && string.IsNullOrEmpty(fromDate) && string.IsNullOrEmpty(toDate)
                    ? "ALL TIME"
                    : timeFilter switch
                    {
                        "today" => "TODAY",
                        "this_week" => "THIS WEEK",
                        "this_month" => "THIS MONTH",
                        "custom" => $"{fromDate} to {toDate}",
                        _ => "ALL TIME"
                    };
                using (var headingCells = worksheet.Cells[currentRow, currentCol, currentRow + 1, currentCol + 3])
                {
                    headingCells.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    headingCells.Style.Fill.BackgroundColor.SetColor(Color.White);
                    headingCells.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);
                    headingCells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    headingCells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                }

                // No. of Records Label
                currentCol += 5;
                worksheet.Cells[currentRow, currentCol, currentRow + 1, currentCol + 1].Merge = true;
                worksheet.Cells[currentRow, currentCol].Value = "No. of Records: ";
                using (var headingCells = worksheet.Cells[currentRow, currentCol, currentRow + 1, currentCol + 1])
                {
                    headingCells.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    headingCells.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#0066A7"));
                    headingCells.Style.Font.Bold = true;
                    headingCells.Style.Font.Color.SetColor(Color.White);
                    headingCells.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);
                    headingCells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    headingCells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                }

                // No. of Records Value
                currentCol += 2;
                worksheet.Cells[currentRow, currentCol, currentRow + 1, currentCol + 3].Merge = true;
                worksheet.Cells[currentRow, currentCol].Value = viewModel.Customers.Count;
                using (var headingCells = worksheet.Cells[currentRow, currentCol, currentRow + 1, currentCol + 3])
                {
                    headingCells.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    headingCells.Style.Fill.BackgroundColor.SetColor(Color.White);
                    headingCells.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);
                    headingCells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    headingCells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                }

                // Table Headers
                int headingRow = currentRow + 4;
                int headingCol = 2;

                // Id (1 cell)
                worksheet.Cells[headingRow, headingCol].Value = "Id";
                headingCol++;

                // Name (3 cells)
                worksheet.Cells[headingRow, headingCol, headingRow, headingCol + 2].Merge = true;
                worksheet.Cells[headingRow, headingCol].Value = "Name";
                headingCol += 3;

                // Email (4 cells)
                worksheet.Cells[headingRow, headingCol, headingRow, headingCol + 3].Merge = true;
                worksheet.Cells[headingRow, headingCol].Value = "Email";
                headingCol += 4;

                // Date (3 cells)
                worksheet.Cells[headingRow, headingCol, headingRow, headingCol + 2].Merge = true;
                worksheet.Cells[headingRow, headingCol].Value = "Date";
                headingCol += 3;

                // Mobile Number (3 cells)
                worksheet.Cells[headingRow, headingCol, headingRow, headingCol + 2].Merge = true;
                worksheet.Cells[headingRow, headingCol].Value = "Mobile Number";
                headingCol += 3;

                // Total Order (1 cell)
                worksheet.Cells[headingRow, headingCol, headingRow, headingCol + 1].Merge = true;
                worksheet.Cells[headingRow, headingCol].Value = "Total Order";
                headingCol++;

                // Style the table headers
                using (var headingCells = worksheet.Cells[headingRow, 2, headingRow, headingCol])
                {
                    headingCells.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    headingCells.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#0066A7"));
                    headingCells.Style.Font.Bold = true;
                    headingCells.Style.Font.Color.SetColor(Color.White);
                    headingCells.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);
                    headingCells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    headingCells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                }

                // Populate the table data
                int row = headingRow + 1;
                foreach (var customer in viewModel.Customers)
                {
                    int startCol = 2;

                    // Id (1 cell)
                    worksheet.Cells[row, startCol].Value = customer.Id;
                    startCol += 1;

                    // Name (3 cells)
                    worksheet.Cells[row, startCol, row, startCol + 2].Merge = true;
                    worksheet.Cells[row, startCol].Value = customer.Name;
                    startCol += 3;

                    // Email (4 cells)
                    worksheet.Cells[row, startCol, row, startCol + 3].Merge = true;
                    worksheet.Cells[row, startCol].Value = customer.Email;
                    startCol += 4;

                    // Date (3 cells)
                    worksheet.Cells[row, startCol, row, startCol + 2].Merge = true;
                    worksheet.Cells[row, startCol].Value = customer.Date?.ToString("dd-MM-yyyy");
                    startCol += 3;

                    // Mobile Number (3 cells)
                    worksheet.Cells[row, startCol, row, startCol + 2].Merge = true;
                    worksheet.Cells[row, startCol].Value = customer.PhoneNo;
                    startCol += 3;

                    // Total Order (1 cell)
                    worksheet.Cells[row, startCol, row, startCol + 1].Merge = true;
                    worksheet.Cells[row, startCol].Value = customer.TotalOrders;
                    startCol++;

                    // Style the data row
                    using (var rowCells = worksheet.Cells[row, 2, row, startCol])
                    {
                        // Apply alternating row colors (light green for odd rows as in the image)
                        if (row % 2 != 0)
                        {
                            rowCells.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            rowCells.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                        }

                        // Apply black borders to each row
                        rowCells.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);
                        rowCells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        rowCells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    }

                    row++;
                }

                // Auto-fit columns for better readability
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // Convert the Excel package to a byte array
                var fileBytes = package.GetAsByteArray();

                // Return the file as a download using FileContentResult
                return new FileContentResult(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    FileDownloadName = "Customers.xlsx"
                };
            }
        }
    }
}