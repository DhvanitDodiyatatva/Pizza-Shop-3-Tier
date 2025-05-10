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

        public async Task<List<WaitingToken>> GetAllWaitingTokensAsync()
        {
            return await _context.WaitingTokens.ToListAsync();
        }

        public async Task<Customer> GetCustomerByEmailAsync(string email)
        {
            return await _context.Customers.FirstOrDefaultAsync(c => c.Email == email);
        }

        public async Task<WaitingToken> GetWaitingTokenByIdAsync(int id)
        {
            return await _context.WaitingTokens
                .Include(wt => wt.Section)
                .FirstOrDefaultAsync(wt => wt.Id == id);
        }

        public async Task UpdateWaitingTokenAsync(WaitingToken waitingToken)
        {
            _context.WaitingTokens.Update(waitingToken);
            await _context.SaveChangesAsync();
        }
    }
}