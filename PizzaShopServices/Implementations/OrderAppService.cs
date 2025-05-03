using PizzaShopRepository.Interfaces;
using PizzaShopRepository.Models;
using PizzaShopServices.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PizzaShopServices.Implementations
{
    public class OrderAppService : IOrderAppService
    {
        private readonly IOrderAppRepository _orderAppRepository;

        public OrderAppService(IOrderAppRepository orderAppRepository)
        {
            _orderAppRepository = orderAppRepository;
        }

        public async Task<(bool Success, string Message)> AssignTableAsync(
            int[] selectedTableIds,
            int sectionId,
            int? waitingTokenId,
            string email,
            string name,
            string phoneNumber,
            int numOfPersons)
        {
            if (selectedTableIds == null || !selectedTableIds.Any())
            {
                return (false, "No tables selected.");
            }

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(phoneNumber) || numOfPersons <= 0)
            {
                return (false, "Invalid customer details.");
            }

            try
            {
                // Check if all selected tables are available
                var tables = await _orderAppRepository.GetTablesByIdsAsync(selectedTableIds);
                if (tables.Any(t => t.Status != "available"))
                {
                    return (false, "One or more selected tables are no longer available.");
                }

                // Check if customer exists by email
                var customer = await _orderAppRepository.GetCustomerByEmailAsync(email);
                if (customer == null)
                {
                    // Create new customer
                    customer = new Customer
                    {
                        Name = name,
                        Email = email,
                        PhoneNo = phoneNumber,
                        NoOfPersons = numOfPersons,
                        Date = DateOnly.FromDateTime(DateTime.Now)
                    };
                    await _orderAppRepository.AddCustomerAsync(customer);
                }

                // Create new order
                var order = new Order
                {
                    CustomerId = customer.Id,
                    TotalAmount = 0,
                    InvoiceNo = $"DOM0{customer.Id}",
                    OrderType = "DineIn",
                    OrderStatus = "pending",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                await _orderAppRepository.AddOrderAsync(order);

                // Update WaitingToken if provided
                if (waitingTokenId.HasValue)
                {
                    var waitingToken = await _orderAppRepository.GetWaitingTokenByIdAsync(waitingTokenId.Value);
                    if (waitingToken != null)
                    {
                        waitingToken.IsAssigned = true;
                        await _orderAppRepository.UpdateWaitingTokenAsync(waitingToken);
                    }
                }

                // Update table statuses and create OrderTable entries
                foreach (var tableId in selectedTableIds)
                {
                    var table = await _orderAppRepository.GetTableByIdAsync(tableId);
                    if (table != null)
                    {
                        table.Status = "reserved";
                        await _orderAppRepository.UpdateTableAsync(table);

                        var orderTable = new OrderTable
                        {
                            OrderId = order.Id,
                            TableId = tableId
                        };
                        await _orderAppRepository.AddOrderTableAsync(orderTable);
                    }
                }

                return (true, "Tables assigned successfully.");
            }
            catch (Exception ex)
            {
                return (false, $"An error occurred: {ex.Message}");
            }
        }
    }
}