using OrderManagementSystem.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using OrderManagementSystem.Infrastructure.DBContext;
using OrderManagementSystem.Domain.Entities;
namespace OrderManagementSystem.Application.Services
{
    public class OrderService //IOrderService
    {
        // DbContext
        private readonly AppDbContext _context;

        // Constructor 
        public OrderService(AppDbContext context)
        {
            _context = context;
        }

        //public async Task<List<Entity_Order>> GetOrders()
        //{
        //   // return await _context.Orders.ToListAsync();
        //}
    }
}
