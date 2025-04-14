using Microsoft.EntityFrameworkCore;
using PizzaShopRepository.Data;
using PizzaShopRepository.Interfaces;
using PizzaShopRepository.Models;
using PizzaShopRepository.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PizzaShopRepository.Repositories
{
    public class KotRepository : IKotRepository
    {
        private readonly PizzaShopContext _dbContext;

        public KotRepository(PizzaShopContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<KotViewModel>> GetKotDataAsync(string status, int categoryId)
        {
            var currentDateTime = DateTime.Now;

            var orderItems = await _dbContext.Orders
                .Include(o => o.OrderTables)
                    .ThenInclude(ot => ot.Table)
                    .ThenInclude(t => t.Section)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Item)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.OrderItemModifiers)
                    .ThenInclude(om => om.Modifier)
                .SelectMany(o => o.OrderItems
                    .Where(oi => string.IsNullOrEmpty(oi.ItemStatus) || oi.ItemStatus.ToLower() == status.ToLower()))
                .ToListAsync();

            var kotData = orderItems.Select(oi => new KotViewModel
            {
                OrderId = oi.OrderId,
                TimeElapsed = currentDateTime - (oi.Order?.CreatedAt ?? DateTime.MinValue),
                SectionName = oi.Order?.OrderTables.FirstOrDefault()?.Table?.Section?.Name ?? "N/A",
                TableName = oi.Order?.OrderTables.FirstOrDefault()?.Table?.Name ?? "N/A",
                ItemName = oi.Item?.Name ?? "N/A",
                ModifierName = oi.OrderItemModifiers.FirstOrDefault()?.Modifier?.Name ?? "N/A",
                Quantity = (int)oi.Quantity,
                SpecialInstructions = oi.SpecialInstructions ?? "N/A"
            }).ToList();

            Console.WriteLine($"Retrieved {kotData.Count} KOT items from repository.");
            if (kotData.Any())
            {
                Console.WriteLine($"Sample: OrderId={kotData[0].OrderId}, ItemName={kotData[0].ItemName}, ItemStatus={orderItems[0].ItemStatus}");
            }

            if (categoryId != 0)
            {
                var categoryNames = await _dbContext.Categories
                    .Where(c => c.Id == categoryId)
                    .Select(c => c.Name)
                    .ToListAsync();
                kotData = kotData.Where(o => categoryNames.Any(cn => o.ItemName.Contains(cn))).ToList();
                Console.WriteLine($"After category filter (categoryId={categoryId}), {kotData.Count} items remain.");
            }

            return kotData;
        }
    }
}