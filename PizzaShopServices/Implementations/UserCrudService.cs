using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PizzaShopRepository.Interfaces;
using PizzaShopRepository.Models;
using PizzaShopRepository.ViewModels;
using PizzaShopServices.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace PizzaShopServices.Implementations
{
    public class UserCrudService : IUserCrudService
    {
        private readonly IUserRepository _userRepository;

        private readonly IEmailService _emailService;

        public UserCrudService(IUserRepository userRepository, IEmailService emailService)
        {
            _userRepository = userRepository;
            _emailService = emailService;
        }

        public async Task<(List<UserList> Users, int TotalUsers)> GetUsersAsync(string searchQuery, int page, int pageSize)
        {
            var query = await _userRepository.GetUsersQueryableAsync();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                searchQuery = searchQuery.ToLower();
                query = query.Where(u =>
                    u.FirstName.ToLower().Contains(searchQuery) ||
                    u.LastName.ToLower().Contains(searchQuery) ||
                    u.Email.ToLower().Contains(searchQuery) ||
                    u.Phone.Contains(searchQuery));
            }

            int totalUsers = await query.CountAsync();
            var users = await query
                .OrderBy(u => u.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserList
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    Phone = u.Phone,
                    Role = u.Role,
                    Status = u.Status
                })
                .ToListAsync();

            return (users, totalUsers);
        }

        public async Task<MyProfile> GetUserProfileAsync(string email)
        {
            var user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null)
            {
                return null;
            }

            return new MyProfile
            {
                Id = user.Id,
                ProfileImage = user.ProfileImage,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Username = user.Username,
                Email = user.Email,
                Phone = user.Phone,
                Country = user.Country,
                State = user.State,
                City = user.City,
                Address = user.Address,
                Zipcode = user.Zipcode,
                Role = user.Role
            };
        }

        public async Task UpdateUserProfileAsync(string email, MyProfile model)
        {
            var user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null)
            {
                throw new Exception("User not found.");
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Username = model.Username;
            user.Phone = model.Phone;
            user.Country = model.Country;
            user.State = model.State;
            user.City = model.City;
            user.Address = model.Address;
            user.Zipcode = model.Zipcode;

            await _userRepository.UpdateUserAsync(user);
        }

        public async Task ChangePasswordAsync(string email, ChangePassword model)
        {
            var user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null)
            {
                throw new Exception("User not found.");
            }

            if (!BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.Password))
            {
                throw new Exception("Current password is incorrect.");
            }

            if (model.NewPassword != model.ConfirmPassword)
            {
                throw new Exception("Passwords do not match.");
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword, 12);
            await _userRepository.UpdateUserAsync(user);
        }

        public async Task<(bool Success, string Message)> AddNewUserAsync(AddEditUserVM model)
        {
            if (await _userRepository.UserExistsAsync(model.Username, model.Email))
            {
                throw new Exception("Username or Email already exists.");
            }

            String temporaryPassword = model.Password;

            var newUser = new User
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Username = model.Username,
                Email = model.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(temporaryPassword),
                Role = model.Role,
                Phone = model.Phone,
                Address = model.Address,
                City = model.City,
                State = model.State,
                Country = model.Country,
                Zipcode = model.Zipcode,
                ProfileImage = model.ProfileImage,
                CreatedAt = DateTime.Now,
                Status = true
            };




            try
            {
                await _userRepository.AddUserAsync(newUser);
                // Send an email with the username and temporary password.
                await _emailService.SendEmailAsync(newUser.Email, newUser.Username, temporaryPassword);
                return (true, "User added successfully.");
            }
            catch (Exception ex)
            {
                return (false, "Failed to add user: " + ex.Message);
            }

        }

        public async Task<AddEditUserVM> GetUserForEditAsync(int id)
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null)
            {
                return null;
            }

            return new AddEditUserVM
            {
                Id = user.Id,
                ProfileImage = string.IsNullOrEmpty(user.ProfileImage) ? "/images/default-pfp.png" : user.ProfileImage,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Username = user.Username,
                Email = user.Email,
                Phone = user.Phone,
                Country = user.Country,
                State = user.State,
                City = user.City,
                Address = user.Address,
                Zipcode = user.Zipcode,
                Role = user.Role,
                Status = user.Status
            };
        }

        public async Task UpdateUserAsync(AddEditUserVM model, IFormFile profileImg, string host)
        {
            var user = await _userRepository.GetUserByIdAsync(model.Id);
            if (user == null)
            {
                throw new Exception("User not found.");
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Username = model.Username;
            user.Phone = model.Phone;
            user.Country = model.Country;
            user.State = model.State;
            user.City = model.City;
            user.Address = model.Address;
            user.Zipcode = model.Zipcode;
            user.Status = model.Status;
            user.Role = model.Role;

            if (profileImg != null && profileImg.Length > 0)
            {
                var fileNameWithoutExt = Path.GetFileNameWithoutExtension(profileImg.FileName);
                var extension = Path.GetExtension(profileImg.FileName);
                var uniqueFileName = $"{fileNameWithoutExt}_{Guid.NewGuid()}{extension}";
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");

                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                var filePath = Path.Combine(uploadPath, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await profileImg.CopyToAsync(stream);
                }

                user.ProfileImage = "/images/" + uniqueFileName;
            }

            await _userRepository.UpdateUserAsync(user);
        }

        public async Task SoftDeleteUserAsync(int id)
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null)
            {
                throw new Exception("User not found.");
            }

            user.IsDeleted = true;
            await _userRepository.UpdateUserAsync(user);
        }



    }
}