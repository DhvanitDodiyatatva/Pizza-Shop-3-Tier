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



        public async Task<(List<UserList> Users, int TotalUsers)> GetUsersAsync(string searchQuery, int page, int pageSize, string sortBy, string sortOrder)
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

            // Apply sorting
            switch (sortBy.ToLower())
            {
                case "name":
                    query = sortOrder.ToLower() == "desc"
                        ? query.OrderByDescending(u => u.FirstName).ThenByDescending(u => u.LastName)
                        : query.OrderBy(u => u.FirstName).ThenBy(u => u.LastName);
                    break;
                case "role":
                    query = sortOrder.ToLower() == "desc"
                        ? query.OrderByDescending(u => u.Role)
                        : query.OrderBy(u => u.Role);
                    break;
                default:
                    query = query.OrderBy(u => u.Id); // Default sorting by Id
                    break;
            }

            int totalUsers = await query.CountAsync();
            var users = await query
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
                    Status = u.Status,
                    ProfileImage = u.ProfileImage
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

        public async Task<(bool Success, string Message)> UpdateUserProfileAsync(string email, MyProfile model)
        {
            var user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null)
            {
                throw new Exception("User not found.");
            }

            if (user.Username != model.Username && await _userRepository.UserExistsAsync(model.Username, null))
            {
                throw new Exception("Username is already taken.");
            }

            // Update user fields
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Username = model.Username;
            user.Phone = model.Phone;
            user.Country = model.Country;
            user.State = model.State;
            user.City = model.City;
            user.Address = model.Address;
            user.Zipcode = model.Zipcode;

            // Handle profile image
            if (model.RemoveImage && !string.IsNullOrEmpty(user.ProfileImage))
            {
                // Delete the existing image file
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.ProfileImage.TrimStart('/'));
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                user.ProfileImage = null; // Clear the image URL in the database
            }
            else if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                // Delete the old image if it exists
                if (!string.IsNullOrEmpty(user.ProfileImage))
                {
                    string oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.ProfileImage.TrimStart('/'));
                    if (File.Exists(oldFilePath))
                    {
                        File.Delete(oldFilePath);
                    }
                }

                // Upload the new image
                string? profilePath = await UploadFile(model.ImageFile, user.Username);
                if (profilePath == null)
                {
                    return (false, "Failed to upload profile image.");
                }
                user.ProfileImage = profilePath; // Update the database with the new image URL
            }
            // If neither RemoveImage nor ImageFile is provided, retain the existing ProfileImage

            try
            {
                await _userRepository.UpdateUserAsync(user);
                return (true, "Profile updated successfully.");
            }
            catch (Exception ex)
            {
                return (false, "Failed to update profile: " + ex.Message);
            }
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

        private async Task<string?> UploadFile(IFormFile file, string userName)
        {
            try
            {
                if (file != null && file.Length > 0)
                {
                    string uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/uploadimages");
                    if (!Directory.Exists(uploadFolder))
                    {
                        Directory.CreateDirectory(uploadFolder);
                    }

                    string fileExtension = Path.GetExtension(file.FileName);
                    string fileName = $"{userName}_{Guid.NewGuid()}{fileExtension}";
                    string filePath = Path.Combine(uploadFolder, fileName);

                    using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    return Path.Combine("/images/uploadimages", fileName).Replace("\\", "/");
                }
            }
            catch (Exception)
            {
                return null;
            }

            return null;
        }

        public async Task<(bool Success, string Message)> AddNewUserAsync(AddEditUserVM model)
        {
            if (await _userRepository.UserExistsAsync(null, model.Email.ToLower().Trim()))
            {
                throw new Exception("Email already exists.");
            }

            if (await _userRepository.UserExistsAsync(model.Username, null))
            {
                throw new Exception("Username already exists.");
            }

            string temporaryPassword = model.Password;
            string? profilePath = null;

            if (model.ImageFile != null && model.ImageFile.Length > 0) // Changed to ImageFile
            {
                profilePath = await UploadFile(model.ImageFile, model.Username);
                if (profilePath == null)
                {
                    return (false, "Failed to upload profile image.");
                }
            }

            var newUser = new User
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Username = model.Username,
                Email = model.Email.ToLower().Trim(),
                Password = BCrypt.Net.BCrypt.HashPassword(temporaryPassword),
                Role = model.Role,
                Phone = model.Phone,
                Address = model.Address,
                City = model.City,
                State = model.State,
                Country = model.Country,
                Zipcode = model.Zipcode,
                ProfileImage = profilePath, // Use the uploaded file path
                CreatedAt = DateTime.Now,
                Status = true
            };

            try
            {
                await _userRepository.AddUserAsync(newUser);
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
                ProfileImage = string.IsNullOrEmpty(user.ProfileImage) ? "/images/default-pfp.png" : user.ProfileImage, // Set the existing image path
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

        public async Task<(bool Success, string Message)> UpdateUserAsync(AddEditUserVM model, IFormFile ImageFile, string host)
        {
            var user = await _userRepository.GetUserByIdAsync(model.Id);
            if (user == null)
            {
                throw new Exception("User not found.");
            }

            if (user.Username != model.Username && await _userRepository.UserExistsAsync(model.Username, null))
            {
                throw new Exception("Username is already taken.");
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

            if (model.RemoveImage && !string.IsNullOrEmpty(user.ProfileImage))
            {
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.ProfileImage.TrimStart('/'));
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                user.ProfileImage = null;
            }
            else if (ImageFile != null && ImageFile.Length > 0)
            {
                string? profilePath = await UploadFile(ImageFile, model.Username);
                if (profilePath == null)
                {
                    throw new Exception("Failed to upload profile image.");
                }
                user.ProfileImage = profilePath; // Update with new image path
            }
            // If neither RemoveImage nor ImageFile is provided, retain existing ProfileImage



            try
            {
                await _userRepository.UpdateUserAsync(user);
                return (true, "User Edited successfully.");
            }
            catch (Exception ex)
            {
                return (false, "Failed to add user: " + ex.Message);
            }


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