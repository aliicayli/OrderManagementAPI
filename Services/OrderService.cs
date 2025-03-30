using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Infrastructures.DTOs;
using OrderManagement.Infrastructures.Models;
using OrderManagementApi.Data;

namespace OrderManagementApi.Services
{
    public interface IOrderService
    {
        Task<OrderDto> CreateOrderAsync(CreateOrderDto createOrderDto);
        Task<List<OrderDto>> GetUserOrdersAsync(int userId);
        Task<OrderDto> GetOrderByIdAsync(int orderId);
        Task<bool> DeleteOrderAsync(int orderId);
    }

    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;

        public OrderService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<OrderDto> CreateOrderAsync(CreateOrderDto createOrderDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var user = await _context.Users.FindAsync(createOrderDto.UserId);
                if (user == null)
                {
                    throw new Exception("User not found.");
                }

                var order = new Order
                {
                    UserId = createOrderDto.UserId,
                    OrderDate = DateTime.Now,
                    Status = "New"
                };

                decimal totalAmount = 0;

                foreach (var item in createOrderDto.Items)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product == null)
                    {
                        throw new Exception($"Product with ID {item.ProductId} not found.");
                    }

                    if (product.StockQuantity < item.Quantity)
                    {
                        throw new Exception($"Not enough stock for product {product.Name}. Available: {product.StockQuantity}");
                    }

                    var orderItem = new OrderItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = product.Price
                    };

                    product.StockQuantity -= item.Quantity;
                    _context.Products.Update(product);

                    totalAmount += orderItem.UnitPrice * orderItem.Quantity;

                    order.OrderItems.Add(orderItem);
                }

                order.TotalAmount = totalAmount;

                await _context.Orders.AddAsync(order);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return await MapOrderToDto(order);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<OrderDto>> GetUserOrdersAsync(int userId)
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.UserId == userId)
                .ToListAsync();

            var orderDtos = new List<OrderDto>();

            foreach (var order in orders)
            {
                orderDtos.Add(await MapOrderToDto(order));
            }

            return orderDtos;
        }

        public async Task<OrderDto> GetOrderByIdAsync(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return null;
            }

            return await MapOrderToDto(order);
        }

        public async Task<bool> DeleteOrderAsync(int orderId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                {
                    return false;
                }

                foreach (var item in order.OrderItems)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        product.StockQuantity += item.Quantity;
                        _context.Products.Update(product);
                    }
                }

                _context.OrderItems.RemoveRange(order.OrderItems);

                _context.Orders.Remove(order);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task<OrderDto> MapOrderToDto(Order order)
        {
            var orderDto = new OrderDto
            {
                Id = order.Id,
                UserId = order.UserId,
                OrderDate = order.OrderDate,
                Status = order.Status,
                TotalAmount = order.TotalAmount,
                Items = new List<OrderItemDetailDto>()
            };

            foreach (var item in order.OrderItems)
            {
                var product = await _context.Products.FindAsync(item.ProductId);

                orderDto.Items.Add(new OrderItemDetailDto
                {
                    ProductId = item.ProductId,
                    ProductName = product?.Name ?? "Unknown Product",
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    Subtotal = item.Quantity * item.UnitPrice
                });
            }

            return orderDto;
        }
    }
}