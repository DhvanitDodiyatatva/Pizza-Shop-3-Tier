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

        public WaitingTokenService(IWaitingTokenRepository waitingTokenRepository, ISectionRepository sectionRepository)
        {
            _waitingTokenRepository = waitingTokenRepository;
            _sectionRepository = sectionRepository;
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
    }
}