using System.ComponentModel.DataAnnotations;

namespace OrderManagement.Infrastructures.DTOs
{
    public class CreateOrderDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
    }
}
