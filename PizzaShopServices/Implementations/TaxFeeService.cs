using PizzaShopRepository.Models;
using PizzaShopRepository.Repositories;
using PizzaShopRepository.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PizzaShopRepository.Services
{
    public class TaxFeeService : ITaxFeeService
    {
        private readonly ITaxFeeRepository _taxFeeRepository;

        public TaxFeeService(ITaxFeeRepository taxFeeRepository)
        {
            _taxFeeRepository = taxFeeRepository;
        }

        public IEnumerable<TaxFeeViewModel> GetAllTaxesFees(string searchQuery, int page, int pageSize, out int totalRecords)
        {
            var taxesFees = _taxFeeRepository.GetAllTaxesFees(searchQuery, page, pageSize, out totalRecords);
            return taxesFees.Select(tf => new TaxFeeViewModel
            {
                Id = tf.Id,
                Name = tf.Name,
                Type = tf.Type,
                Value = tf.Value,
                IsEnabled = tf.IsEnabled,
                IsDefault = tf.IsDefault
            }).ToList();
        }

        public TaxFeeAddEditViewModel GetTaxFeeById(int id)
        {
            var taxFee = _taxFeeRepository.GetTaxFeeById(id);
            if (taxFee == null)
            {
                return null;
            }

            return new TaxFeeAddEditViewModel
            {
                Id = taxFee.Id,
                Name = taxFee.Name,
                Type = taxFee.Type,
                Value = taxFee.Value,
                IsEnabled = taxFee.IsEnabled,
                IsDefault = taxFee.IsDefault
            };
        }

        public void AddTaxFee(TaxFeeAddEditViewModel model)
        {
            try
            {
                if (model == null)
                {
                    throw new ArgumentNullException(nameof(model), "TaxFee model cannot be null.");
                }

                // Check for duplicate name
                var existingTax = _taxFeeRepository.GetTaxFeeByName(model.Name);
                if (existingTax != null)
                {
                    throw new Exception("A tax/fee with this name already exists.");
                }

                // Validate percentage
                if (model.Type == "percentage" && model.Value > 100)
                {
                    throw new Exception("Percentage cannot exceed 100%.");
                }

                var taxFee = new TaxesFee
                {
                    Name = model.Name,
                    Type = model.Type,
                    Value = model.Value,
                    IsEnabled = model.IsEnabled,
                    IsDefault = model.IsDefault,
                    IsDeleted = false
                };
                _taxFeeRepository.AddTaxFee(taxFee);
            }
            catch (Exception ex)
            {
                string errorMessage = $" {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $" Inner Exception: {ex.InnerException.Message}";
                }
                throw new Exception(errorMessage, ex);
            }
        }

        public void UpdateTaxFee(TaxFeeAddEditViewModel model)
        {
            try
            {
                if (model == null)
                {
                    throw new ArgumentNullException(nameof(model), "TaxFee model cannot be null.");
                }

                var taxFee = _taxFeeRepository.GetTaxFeeById(model.Id);
                if (taxFee == null)
                {
                    throw new Exception($"Tax/Fee with ID {model.Id} not found.");
                }

                // Validate percentage
                if (model.Type == "percentage" && model.Value > 100)
                {
                    throw new Exception("Percentage cannot exceed 100%.");
                }

                taxFee.Name = model.Name;
                taxFee.Type = model.Type;
                taxFee.Value = model.Value;
                taxFee.IsEnabled = model.IsEnabled;
                taxFee.IsDefault = model.IsDefault;
                _taxFeeRepository.UpdateTaxFee(taxFee);
            }
            catch (Exception ex)
            {
                string errorMessage = $" {ex.Message}";
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
                _taxFeeRepository.DeleteTaxFee(id);
            }
            catch (Exception ex)
            {
                string errorMessage = "Failed to delete tax/fee in service layer.";
                if (ex.InnerException != null)
                {
                    errorMessage += $" Inner Exception: {ex.InnerException.Message}";
                }
                throw new Exception(errorMessage, ex);
            }
        }
    }
}