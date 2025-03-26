using PizzaShopRepository.Interfaces;
using PizzaShopRepository.Models;
using PizzaShopServices.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PizzaShopServices.Implementations
{
    public class ItemModifierGroupService : IItemModifierGroupService
    {
        private readonly IItemModifierGroupRepository _itemModifierGroupRepository;

        public ItemModifierGroupService(IItemModifierGroupRepository itemModifierGroupRepository)
        {
            _itemModifierGroupRepository = itemModifierGroupRepository;
        }

        public async Task AddItemModifierGroupAsync(int itemId, int modifierGroupId, int? minLoad, int? maxLoad)
        {
            var itemModifierGroup = new ItemModifierGroup
            {
                ItemId = itemId,
                ModifierGroupId = modifierGroupId,
                MinLoad = minLoad,
                MaxLoad = maxLoad,
                IsDeleted = false
            };
            await _itemModifierGroupRepository.AddItemModifierGroupAsync(itemModifierGroup);
        }

        public async Task<List<ItemModifierGroup>> GetItemModifierGroupsByItemIdAsync(int itemId)
        {
            return await _itemModifierGroupRepository.GetItemModifierGroupsByItemIdAsync(itemId);
        }

        public async Task UpdateItemModifierGroupAsync(int itemId, int modifierGroupId, int? minLoad, int? maxLoad)
        {
            var itemModifierGroup = (await _itemModifierGroupRepository.GetItemModifierGroupsByItemIdAsync(itemId))
                .FirstOrDefault(img => img.ModifierGroupId == modifierGroupId);
            if (itemModifierGroup == null)
            {
                throw new Exception("Item-ModifierGroup mapping not found.");
            }

            itemModifierGroup.MinLoad = minLoad;
            itemModifierGroup.MaxLoad = maxLoad;
            await _itemModifierGroupRepository.UpdateItemModifierGroupAsync(itemModifierGroup);
        }

        public async Task DeleteItemModifierGroupAsync(int itemId, int modifierGroupId)
        {
            await _itemModifierGroupRepository.DeleteItemModifierGroupAsync(itemId, modifierGroupId);
        }
    }
}