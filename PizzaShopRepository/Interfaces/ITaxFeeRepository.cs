using PizzaShopRepository.Models;
using System.Collections.Generic;

namespace PizzaShopRepository.Repositories
{
    public interface ITaxFeeRepository
    {
        IEnumerable<TaxesFee> GetAllTaxesFees(string searchQuery, int page, int pageSize, out int totalRecords);
        TaxesFee GetTaxFeeById(int id);
        TaxesFee GetTaxFeeByName(string name);
        void AddTaxFee(TaxesFee taxFee);
        void UpdateTaxFee(TaxesFee taxFee);
        void DeleteTaxFee(int id);
    }
}