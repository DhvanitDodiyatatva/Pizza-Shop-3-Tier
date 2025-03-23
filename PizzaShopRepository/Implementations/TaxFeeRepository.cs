using PizzaShopRepository.Data;
using PizzaShopRepository.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PizzaShopRepository.Repositories
{
    public class TaxFeeRepository : ITaxFeeRepository
    {
        private readonly PizzaShopContext _context;

        public TaxFeeRepository(PizzaShopContext context)
        {
            _context = context;
        }

        public IEnumerable<TaxesFee> GetAllTaxesFees(string searchQuery, int page, int pageSize, out int totalRecords)
        {
            var query = _context.TaxesFees.Where(tf => !tf.IsDeleted);

            if (!string.IsNullOrEmpty(searchQuery))
            {
                var searchLower = searchQuery.ToLower(); // Convert search query to lowercase once
                query = query.Where(tf => tf.Name.ToLower().Contains(searchLower) || tf.Type.ToLower().Contains(searchLower));
            }

            totalRecords = query.Count();

            return query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        public TaxesFee GetTaxFeeById(int id)
        {
            return _context.TaxesFees.FirstOrDefault(tf => tf.Id == id && !tf.IsDeleted);
        }

        public TaxesFee GetTaxFeeByName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            return _context.TaxesFees
                .FirstOrDefault(tf => tf.Name.ToLower() == name.ToLower() && !tf.IsDeleted);
        }

        public void AddTaxFee(TaxesFee taxFee)
        {
            try
            {
                if (taxFee == null)
                {
                    throw new ArgumentNullException(nameof(taxFee), "TaxFee entity cannot be null.");
                }

                _context.TaxesFees.Add(taxFee);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                string errorMessage = $"Failed to add tax/fee to the database. Details: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $" Inner Exception: {ex.InnerException.Message}";
                }
                throw new Exception(errorMessage, ex);
            }
        }

        public void UpdateTaxFee(TaxesFee taxFee)
        {
            try
            {
                if (taxFee == null)
                {
                    throw new ArgumentNullException(nameof(taxFee), "TaxFee entity cannot be null.");
                }

                _context.TaxesFees.Update(taxFee);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                string errorMessage = $"Failed to update tax/fee in the database. Details: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $" Inner Exception: {ex.InnerException.Message}";
                }
                throw new Exception(errorMessage, ex);
            }
        }

        public void DeleteTaxFee(int id)
        {
            try
            {
                var taxFee = _context.TaxesFees.FirstOrDefault(tf => tf.Id == id && !tf.IsDeleted);
                if (taxFee == null)
                {
                    throw new Exception($"Tax/Fee with ID {id} not found or already deleted.");
                }

                taxFee.IsDeleted = true;
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                string errorMessage = "Failed to delete tax/fee in the database.";
                if (ex.InnerException != null)
                {
                    errorMessage += $" Inner Exception: {ex.InnerException.Message}";
                }
                throw new Exception(errorMessage, ex);
            }
        }
    }
}