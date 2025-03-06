using PizzaShopRepository.Interfaces;
using PizzaShopRepository.Models;
using PizzaShop.Services.Interfaces;
using PizzaShopRepository.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using PizzaShopRepository.ViewModels;

namespace PizzaShop.Services.Implementations
{
    public class MenuService : IMenuService
    {
        private readonly IMenuRepository _menuRepository;

        public MenuService(IMenuRepository menuRepository)
        {
            _menuRepository = menuRepository;
        }

        // Category methods
        public IEnumerable<CategoryViewModel> GetAllCategories()
        {
            return _menuRepository.GetAllCategories().Select(c => new CategoryViewModel
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                IsDeleted = c.IsDeleted
            });
        }

        public CategoryViewModel GetCategoryById(int id)
        {
            var category = _menuRepository.GetCategoryById(id);
            return category == null ? null : new CategoryViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IsDeleted = category.IsDeleted
            };
        }

        public void CreateCategory(CreateCategoryViewModel model)
        {
            var category = new Category
            {
                Name = model.Name,
                Description = model.Description,
                IsDeleted = false
            };
            _menuRepository.AddCategory(category);
            _menuRepository.SaveChanges();
        }

        public void UpdateCategory(UpdateCategoryViewModel model)
        {
            var category = _menuRepository.GetCategoryById(model.Id);
            if (category != null)
            {
                category.Name = model.Name;
                category.Description = model.Description;
                _menuRepository.UpdateCategory(category);
                _menuRepository.SaveChanges();
            }
        }

        public void DeleteCategory(int id)
        {
            _menuRepository.DeleteCategory(id);
            _menuRepository.SaveChanges();
        }

        // Item methods
        public IEnumerable<ItemVMViewModel> GetAllItems()
        {
            return _menuRepository.GetAllItems().Select(i => new ItemVMViewModel
            {
                Id = i.Id,
                CategoryId = i.CategoryId,
                Name = i.Name,
                Description = i.Description,
                Price = i.Price,
                ItemType = i.ItemType,
                Quantity = i.Quantity,
                Unit = i.Unit,
                IsAvailable = i.IsAvailable,
                ShortCode = i.ShortCode,
                ImageUrl = i.ImageUrl,
                IsDeleted = i.IsDeleted,
                CategoryName = i.Category?.Name
            });
        }

        public ItemVMViewModel GetItemById(int id)
        {
            var item = _menuRepository.GetItemById(id);
            return item == null ? null : new ItemVMViewModel
            {
                Id = item.Id,
                CategoryId = item.CategoryId,
                Name = item.Name,
                Description = item.Description,
                Price = item.Price,
                ItemType = item.ItemType,
                Quantity = item.Quantity,
                Unit = item.Unit,
                IsAvailable = item.IsAvailable,
                ShortCode = item.ShortCode,
                ImageUrl = item.ImageUrl,
                IsDeleted = item.IsDeleted,
                CategoryName = item.Category?.Name
            };
        }

        public void CreateItem(CreateItemVMViewModel model)
        {
            var item = new Item
            {
                CategoryId = model.CategoryId,
                Name = model.Name,
                Description = model.Description,
                Price = model.Price,
                ItemType = model.ItemType,
                Quantity = model.Quantity,
                Unit = model.Unit,
                IsAvailable = model.IsAvailable,
                ShortCode = model.ShortCode,
                ImageUrl = model.ImageUrl,
                IsDeleted = false,
                CreatedAt = DateTime.Now
            };
            _menuRepository.AddItem(item);
            _menuRepository.SaveChanges();
        }

        public void UpdateItem(UpdateItemVMViewModel model)
        {
            var item = _menuRepository.GetItemById(model.Id);
            if (item != null)
            {
                item.CategoryId = model.CategoryId;
                item.Name = model.Name;
                item.Description = model.Description;
                item.Price = model.Price;
                item.ItemType = model.ItemType;
                item.Quantity = model.Quantity;
                item.Unit = model.Unit;
                item.IsAvailable = model.IsAvailable;
                item.ShortCode = model.ShortCode;
                item.ImageUrl = model.ImageUrl;
                item.UpdatedAt = DateTime.Now;
                _menuRepository.UpdateItem(item);
                _menuRepository.SaveChanges();
            }
        }

        public void DeleteItem(int id)
        {
            _menuRepository.DeleteItem(id);
            _menuRepository.SaveChanges();
        }

        public void DeleteItems(List<int> ids)
        {
            _menuRepository.DeleteItems(ids);
            _menuRepository.SaveChanges();
        }
    }
}