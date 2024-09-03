using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Order.API.Models;
using Order.API.Models.Entities;
using Order.API.Models.Enums;
using Order.API.ViewModels;
using Shared.Events;

namespace Order.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly OrderAPIDbContext _orderAPIDbContext;
        private readonly IPublishEndpoint _publishEndpoint;
        public OrdersController(OrderAPIDbContext orderAPIDbContext, IPublishEndpoint publishEndpoint)
        {
            _orderAPIDbContext = orderAPIDbContext;
            _publishEndpoint = publishEndpoint;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder(CreateOrderVM createOrder)
        {
            Models.Entities.Order order = new Models.Entities.Order()
            {
                Id = Guid.NewGuid(),
                BuyerId = createOrder.BuyerId,
                CreatedDate = DateTime.Now,
                OrderStatus = OrderStatus.Suspend,
            };

            order.OrderItems = createOrder.OrderItems.Select(oi => new OrderItem
            {
                Count = oi.Count,
                Price = oi.Price,
                ProductId = oi.ProductId,
            }).ToList();

            order.TotalPrice = createOrder.OrderItems.Sum(oi => (oi.Price * oi.Count));

            await _orderAPIDbContext.Orders.AddAsync(order);
            await _orderAPIDbContext.SaveChangesAsync();

            OrderCreatedEvent orderCreatedEvent = new OrderCreatedEvent
            {
                BuyerId = order.BuyerId,
                OrderId = order.Id,
                OrderItems = order.OrderItems.Select(oi => new Shared.Messages.OrderItemMessage
                {
                    Count = oi.Count,
                    ProductId = oi.ProductId
                }).ToList(),
                TotalPrice = order.TotalPrice,
            };

            await _publishEndpoint.Publish(orderCreatedEvent);

            return Ok();
        }
    }
}
