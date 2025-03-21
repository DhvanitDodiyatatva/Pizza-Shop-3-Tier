using PizzaShopRepository.Interfaces;
using PizzaShopRepository.Models;
using PizzaShopRepository.Repositories;
using System.Collections.Generic;

namespace PizzaShopService
{
    public class ModifierGroupMappingService : IModifierGroupMappingService
    {
        private readonly IModifierGroupMappingRepository _mappingRepository;
        private readonly IModifierRepository _modifierRepository;

        public ModifierGroupMappingService(IModifierGroupMappingRepository mappingRepository, IModifierRepository modifierRepository)
        {
            _mappingRepository = mappingRepository;
            _modifierRepository = modifierRepository;
        }

        public async Task<ModifierGroupMapping> CreateMappingAsync(int modifierGroupId, int modifierId)
        {
            var modifier = await _modifierRepository.GetModifierByIdAsync(modifierId);
            if (modifier == null)
                throw new Exception("Modifier not found");

            return await _mappingRepository.CreateMappingAsync(modifierGroupId, modifierId);
        }

        public async Task<List<Modifier>> GetModifiersByGroupIdAsync(int modifierGroupId)
        {
            return await _mappingRepository.GetModifiersByModifierGroupIdAsync(modifierGroupId);
        }
    }
}
