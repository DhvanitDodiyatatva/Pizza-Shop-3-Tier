using Microsoft.AspNetCore.Mvc.Rendering;
using PizzaShopRepository.Models;
using PizzaShopRepository.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PizzaShopServices.Interfaces
{
    public interface IOrderAppService
    {
        Task<(bool Success, string Message, int OrderId)> AssignTableAsync(int[] selectedTableIds, int sectionId, int? waitingTokenId, string email, string name, string phoneNumber, int numOfPersons);
        Task<List<SectionDetailsViewModel>> GetSectionDetailsAsync();
        Task<WaitingTokenViewModel> PrepareWaitingTokenModalAsync(int sectionId, string sectionName);
        Task<CustomerDetailsViewModel> PrepareCustomerDetailsOffcanvasAsync(string sectionIds, string sectionName, string selectedTableIds);
        Task<List<Category>> GetAllCategoriesAsync();
        Task<List<Item>> GetItemsAsync(string category, string search);
        Task<(List<WaitingToken> WaitingTokens, List<SelectListItem> Sections)> GetWaitingListDataAsync();
        Task<UpdateOrderItemStatusViewModel> PrepareKotDetailsModalAsync(int orderId, string status, string selectedCategory, string selectedStatus);
    }
}