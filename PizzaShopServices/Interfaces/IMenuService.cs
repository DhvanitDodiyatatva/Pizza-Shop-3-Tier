using PizzaShopRepository.ViewModels;
using PizzaShopRepository.ViewModels;
using System.Collections.Generic;

namespace PizzaShop.Services.Interfaces
{
    public interface IMenuService
    {
        // Category methods
        IEnumerable<CategoryViewModel> GetAllCategories();
        CategoryViewModel GetCategoryById(int id);
        void CreateCategory(CreateCategoryViewModel model);
        void UpdateCategory(UpdateCategoryViewModel model);
        void DeleteCategory(int id);

        // Item methods
        IEnumerable<ItemVMViewModel> GetAllItems();
        ItemVMViewModel GetItemById(int id);
        void CreateItem(CreateItemVMViewModel model);
        void UpdateItem(UpdateItemVMViewModel model);
        void DeleteItem(int id);
        void DeleteItems(List<int> ids);
    }
}