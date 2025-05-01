using Microsoft.AspNetCore.Http;
using PizzaShopRepository.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PizzaShopServices.Interfaces
{
    public interface IUserCrudService
    {
        // Task<(List<UserList> Users, int TotalUsers)> GetUsersAsync(string searchQuery, int page, int pageSize);
        Task<(List<UserList> Users, int TotalUsers)> GetUsersAsync(string searchQuery, int page, int pageSize, string sortBy, string sortOrder);
        Task<MyProfile> GetUserProfileAsync(string email);
        Task<(bool Success, string Message)> UpdateUserProfileAsync(string email, MyProfile model);
        Task ChangePasswordAsync(string email, ChangePassword model);
        Task<(bool Success, string Message)> AddNewUserAsync(AddEditUserVM model);
        Task<AddEditUserVM> GetUserForEditAsync(int id);
        Task<(bool Success, string Message)> UpdateUserAsync(AddEditUserVM model, IFormFile profileImg, string host);
        Task SoftDeleteUserAsync(int id);

    }
}