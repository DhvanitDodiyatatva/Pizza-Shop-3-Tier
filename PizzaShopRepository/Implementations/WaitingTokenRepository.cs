using Microsoft.EntityFrameworkCore;
using PizzaShopRepository.Data;
using PizzaShopRepository.Interfaces;
using PizzaShopRepository.Models;

namespace PizzaShopRepository.Repositories
{
    public class WaitingTokenRepository : IWaitingTokenRepository
    {
        private readonly PizzaShopContext _context;

        public WaitingTokenRepository(PizzaShopContext context)
        {
            _context = context;
        }

        public async Task AddWaitingTokenAsync(WaitingToken waitingToken)
        {
            _context.WaitingTokens.Add(waitingToken);
            await _context.SaveChangesAsync();
        }

        public async Task AddCustomerAsync(Customer customer)
        {
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
        }
    }
}