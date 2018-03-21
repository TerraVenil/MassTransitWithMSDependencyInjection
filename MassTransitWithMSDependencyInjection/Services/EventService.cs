using System.Threading.Tasks;
using MassTransit;
using MassTransitWithMSDependencyInjection.Models;

namespace MassTransitWithMSDependencyInjection.Services
{
    public interface IEventService
    {
        Task Publish(EventCreated eventCreated);
    }

    public class EventService : IEventService
    {
        private readonly IBus _bus;

        public EventService(IBus bus)
        {
            _bus = bus;
        }

        public async Task Publish(EventCreated eventCreated)
        {
            await _bus.Publish(new CalculateCommand { Id = 3 });
        }
    }
}