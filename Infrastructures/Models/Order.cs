using System.ComponentModel.DataAnnotations;

namespace OrderManagement.Infrastructures.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;

        public string Status { get; set; } = "New";

        public decimal TotalAmount { get; set; }

        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
