using PizzaShopRepository.Models;
using System.Threading.Tasks;

namespace PizzaShopServices.Interfaces
{
    public interface IOrderAppService
    {
        Task<(bool Success, string Message)> AssignTableAsync(
            int[] selectedTableIds, 
            int sectionId, 
            int? waitingTokenId, 
            string email, 
            string name, 
            string phoneNumber, 
            int numOfPersons);
    }
}