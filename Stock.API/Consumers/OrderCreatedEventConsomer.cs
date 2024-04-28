using Common.Shared.Events;
using MassTransit;
using System.Diagnostics;
using System.Text.Json;

namespace Stock.API.Consumers
{
    public class OrderCreatedEventConsomer : IConsumer<OrderCreatedEvent>
    {
        public Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            Thread.Sleep(2000);
            Activity.Current.SetTag("message.body", JsonSerializer.Serialize(context.Message));

            return Task.CompletedTask;
        }
    }
}
