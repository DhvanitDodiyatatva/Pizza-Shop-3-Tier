using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace PizzaShopService.Hubs
{
    public class KotHub : Hub
    {
        // Notify all clients of a new or updated order
        public async Task SendOrderUpdate(int orderId)
        {
            await Clients.All.SendAsync("ReceiveOrderUpdate", orderId);
        }

        // Notify all clients of an order status change
        public async Task SendOrderStatusUpdate(int orderId, string newStatus)
        {
            await Clients.All.SendAsync("ReceiveOrderStatusUpdate", orderId, newStatus);
        }
    }
}