using PizzaShopRepository.Models;
using PizzaShopRepository.ViewModels;
using System.Collections.Generic;

namespace PizzaShopRepository.Services
{
    public interface ITaxFeeService
    {
        IEnumerable<TaxFeeViewModel> GetAllTaxesFees(string searchQuery, int page, int pageSize, out int totalRecords);
        TaxFeeAddEditViewModel GetTaxFeeById(int id);
        void AddTaxFee(TaxFeeAddEditViewModel model);
        void UpdateTaxFee(TaxFeeAddEditViewModel model);
        void DeleteTaxFee(int id);
    }
}