using PizzaShopRepository.Models;
using System.Collections.Generic;

namespace PizzaShopRepository.Interfaces
{
    public interface IMenuRepository
    {
        // Category methods
        IEnumerable<Category> GetAllCategories();
        Category GetCategoryById(int id);
        void AddCategory(Category category);
        void UpdateCategory(Category category);
        void DeleteCategory(int id);

        // Item methods
        IEnumerable<Item> GetAllItems();
        Item GetItemById(int id);
        void AddItem(Item item);
        void UpdateItem(Item item);
        void DeleteItem(int id);
        void DeleteItems(List<int> ids);

        // Save changes
        void SaveChanges();
    }
}