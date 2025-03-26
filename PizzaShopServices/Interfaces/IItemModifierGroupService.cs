using PizzaShopRepository.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PizzaShopServices.Interfaces
{
    public interface IItemModifierGroupService
    {
        Task AddItemModifierGroupAsync(int itemId, int modifierGroupId, int? minLoad, int? maxLoad);
        Task<List<ItemModifierGroup>> GetItemModifierGroupsByItemIdAsync(int itemId);
        Task UpdateItemModifierGroupAsync(int itemId, int modifierGroupId, int? minLoad, int? maxLoad);
        Task DeleteItemModifierGroupAsync(int itemId, int modifierGroupId);
    }
}