using MassTransit;
using Shared.Events;

namespace Payment.API.Consumers
{
    public class StockReserveEventConsumer : IConsumer<StockReservedEvent>
    {
        private readonly IPublishEndpoint _endpoint;

        public StockReserveEventConsumer(IPublishEndpoint endpoint)
        {
            _endpoint = endpoint;
        }


        public Task Consume(ConsumeContext<StockReservedEvent> context)
        {
            if (true)
            {
                PaymentCompletedEvent paymentCompletedEvent = new PaymentCompletedEvent() { OrderId = context.Message.OrderId };
                _endpoint.Publish(paymentCompletedEvent);

                Console.WriteLine("Payment is succeeded");
            }

            else
            {
                PaymentFailedEvent paymentFailedEvent = new PaymentFailedEvent() { OrderId = context.Message.OrderId, Message = "PaymentFailed" };

                _endpoint.Publish(paymentFailedEvent);

                Console.WriteLine("Payment is failed");
            }
            return Task.CompletedTask;
        }
    }
}
