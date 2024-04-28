using Microsoft.EntityFrameworkCore;

namespace Order.API.OrderServices
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int Count { get; set; }

        [Precision(18, 2)]
        public decimal UnitPrice { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; }
    }
}
