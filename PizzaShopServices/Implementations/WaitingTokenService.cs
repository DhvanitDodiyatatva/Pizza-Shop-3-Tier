using Microsoft.EntityFrameworkCore;
using PizzaShopRepository.Interfaces;
using PizzaShopRepository.Models;
using PizzaShopRepository.ViewModels;
using PizzaShopServices.Interfaces;

namespace PizzaShopServices.Implementations
{
    public class WaitingTokenService : IWaitingTokenService
    {
        private readonly IWaitingTokenRepository _waitingTokenRepository;
        private readonly ISectionRepository _sectionRepository;
        private readonly PizzaShopRepository.Data.PizzaShopContext _context;

        public WaitingTokenService(IWaitingTokenRepository waitingTokenRepository, ISectionRepository sectionRepository, PizzaShopRepository.Data.PizzaShopContext context)
        {
            _waitingTokenRepository = waitingTokenRepository;
            _sectionRepository = sectionRepository;
            _context = context;
        }

        public async Task<(bool Success, string Message)> AddWaitingTokenAsync(WaitingTokenViewModel model)
        {
            var section = await _sectionRepository.GetSection(model.SectionName);
            if (section == null)
            {
                return (false, "Section not found.");
            }

            try
            {
                // Check if customer exists by email
                var existingCustomer = await _waitingTokenRepository.GetCustomerByEmailAsync(model.Email);

                if (existingCustomer == null)
                {
                    // Customer does not exist, create new customer
                    var customer = new Customer
                    {
                        Name = model.CustomerName,
                        Email = model.Email,
                        PhoneNo = model.PhoneNumber,
                        NoOfPersons = model.NumOfPersons,
                        Date = DateOnly.FromDateTime(DateTime.Now)
                    };
                    await _waitingTokenRepository.AddCustomerAsync(customer);
                }

                // Create waiting token (whether customer existed or was just created)
                var waitingToken = new WaitingToken
                {
                    CustomerName = model.CustomerName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    NumOfPersons = model.NumOfPersons,
                    SectionId = section.Id,
                    Status = "waiting",
                    CreatedAt = DateTime.Now,
                    IsDeleted = false
                };

                await _waitingTokenRepository.AddWaitingTokenAsync(waitingToken);
                return (true, "Waiting token assigned successfully.");
            }
            catch (Exception ex)
            {
                return (false, "Failed to assign waiting token: " + ex.Message);
            }
        }


        public async Task<List<WaitingToken>> GetAllWaitingTokensAsync()
        {
            try
            {
                var waitingTokens = await _waitingTokenRepository.GetAllWaitingTokensAsync();
                return waitingTokens.Where(t => !t.IsDeleted && !t.IsAssigned).ToList(); // Filter out deleted tokens
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Error fetching waiting tokens: {ex.Message}");
                return new List<WaitingToken>(); // Return empty list on error
            }
        }

        public async Task<WaitingToken> GetWaitingTokenByIdAsync(int id)
        {
            return await _waitingTokenRepository.GetWaitingTokenByIdAsync(id);
        }

        public async Task<(bool Success, string Message)> UpdateWaitingTokenAsync(WaitingTokenViewModel model)
        {
            var section = await _sectionRepository.GetSection(model.SectionName);
            if (section == null)
            {
                return (false, "Section not found.");
            }

            try
            {
                var waitingToken = await _waitingTokenRepository.GetWaitingTokenByIdAsync(model.Id);
                if (waitingToken == null)
                {
                    return (false, "Waiting token not found.");
                }

                // Check if the new email exists in Customers table (excluding the current waiting token's customer)
                if (await IsEmailExistsAsync(model.Email, model.Id))
                {
                    return (false, "This email already exists. Please choose a different email.");
                }

                // Update waiting token fields
                waitingToken.CustomerName = model.CustomerName;
                waitingToken.Email = model.Email;
                waitingToken.PhoneNumber = model.PhoneNumber;
                waitingToken.NumOfPersons = model.NumOfPersons;
                waitingToken.SectionId = section.Id;

                await _waitingTokenRepository.UpdateWaitingTokenAsync(waitingToken);
                return (true, "Waiting token edited successfully.");
            }
            catch (Exception ex)
            {
                return (false, "Failed to update waiting token: " + ex.Message);
            }
        }

        public async Task<bool> IsEmailExistsAsync(string email, int excludeWaitingTokenId)
        {
            var customer = await _waitingTokenRepository.GetCustomerByEmailAsync(email);
            if (customer == null)
            {
                return false;
            }

            // Check if the customer is associated with a different waiting token
            var existingToken = await _context.WaitingTokens
                .Where(wt => wt.Email == email && wt.Id != excludeWaitingTokenId && !wt.IsDeleted)
                .FirstOrDefaultAsync();
            return existingToken != null;
        }

        public async Task<(bool Success, string Message)> DeleteWaitingTokenAsync(int id)
        {
            try
            {
                var waitingToken = await _waitingTokenRepository.GetWaitingTokenByIdAsync(id);
                if (waitingToken == null)
                {
                    return (false, "Waiting token not found.");
                }

                if (waitingToken.IsDeleted)
                {
                    return (false, "Waiting token is already deleted.");
                }

                waitingToken.IsDeleted = true;
                await _waitingTokenRepository.UpdateWaitingTokenAsync(waitingToken);
                return (true, "Waiting token deleted successfully.");
            }
            catch (Exception ex)
            {
                return (false, "Failed to delete waiting token: " + ex.Message);
            }
        }
    }
}