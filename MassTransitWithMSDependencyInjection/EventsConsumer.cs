using System.Threading.Tasks;
using MassTransit;
using MassTransitWithMSDependencyInjection.Models;

namespace MassTransitWithMSDependencyInjection
{
    public class EventsConsumer : IConsumer<EventCreated>
    {
        private readonly ICorrelationScoped _correlationScoped;

        public EventsConsumer(ICorrelationScoped correlationScoped)
        {
            _correlationScoped = correlationScoped;
        }

        public async Task Consume(ConsumeContext<EventCreated> context)
        {
            if (!string.IsNullOrEmpty(_correlationScoped.CorrelationId))
                await context.Publish(new CalculateCommand { Id = 1 });
        }
    }
}