using Microsoft.EntityFrameworkCore;
using PizzaShopRepository.Data;
using PizzaShopRepository.Interfaces;
using PizzaShopRepository.Models;
using System.Collections.Generic;
using System.Linq;

namespace PizzaShopRepository.Implementations
{
    public class MenuRepository : IMenuRepository
    {
        private readonly PizzaShopContext _context;

        public MenuRepository(PizzaShopContext context)
        {
            _context = context;
        }

        // Category methods
        public IEnumerable<Category> GetAllCategories()
        {
            return _context.Categories.Where(c => !c.IsDeleted).ToList();
        }

        public Category GetCategoryById(int id)
        {
            return _context.Categories.FirstOrDefault(c => c.Id == id && !c.IsDeleted);
        }

        public void AddCategory(Category category)
        {
            _context.Categories.Add(category);
        }

        public void UpdateCategory(Category category)
        {
            _context.Categories.Update(category);
        }

        public void DeleteCategory(int id)
        {
            var category = GetCategoryById(id);
            if (category != null)
            {
                category.IsDeleted = true;
                _context.Categories.Update(category);
            }
        }

        // Item methods
        public IEnumerable<Item> GetAllItems()
        {
            return _context.Items.Where(i => !i.IsDeleted).ToList();
        }

        public Item GetItemById(int id)
        {
            return _context.Items.FirstOrDefault(i => i.Id == id && !i.IsDeleted);
        }

        public void AddItem(Item item)
        {
            _context.Items.Add(item);
        }

        public void UpdateItem(Item item)
        {
            _context.Items.Update(item);
        }

        public void DeleteItem(int id)
        {
            var item = GetItemById(id);
            if (item != null)
            {
                item.IsDeleted = true;
                _context.Items.Update(item);
            }
        }

        public void DeleteItems(List<int> ids)
        {
            var items = _context.Items.Where(i => ids.Contains(i.Id) && !i.IsDeleted);
            foreach (var item in items)
            {
                item.IsDeleted = true;
            }
        }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }
    }
}