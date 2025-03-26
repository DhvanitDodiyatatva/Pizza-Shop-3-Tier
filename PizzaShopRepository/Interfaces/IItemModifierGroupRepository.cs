using PizzaShopRepository.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PizzaShopRepository.Interfaces
{
    public interface IItemModifierGroupRepository
    {
        Task AddItemModifierGroupAsync(ItemModifierGroup itemModifierGroup);
        Task<List<ItemModifierGroup>> GetItemModifierGroupsByItemIdAsync(int itemId);
        Task UpdateItemModifierGroupAsync(ItemModifierGroup itemModifierGroup);
        Task DeleteItemModifierGroupAsync(int itemId, int modifierGroupId);
    }
}