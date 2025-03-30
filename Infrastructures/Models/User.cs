using System.ComponentModel.DataAnnotations;

namespace OrderManagement.Infrastructures.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string Email { get; set; }

        public List<Order> Orders { get; set; } = new List<Order>();
    }
}
