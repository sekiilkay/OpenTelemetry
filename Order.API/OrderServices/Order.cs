using Microsoft.EntityFrameworkCore;
using System;

namespace Order.API.OrderServices
{
    public class Order
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!;
        public DateTime Created { get; set; }
        public int UserId { get; set; }
        public OrderStatus Status { get; set; }
        public List<OrderItem> Items { get; set; }
    }
}
