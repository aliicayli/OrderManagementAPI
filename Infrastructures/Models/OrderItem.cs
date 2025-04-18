﻿using System.ComponentModel.DataAnnotations;

namespace OrderManagement.Infrastructures.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public decimal UnitPrice { get; set; }

        public Product Product { get; set; }
    }
}
