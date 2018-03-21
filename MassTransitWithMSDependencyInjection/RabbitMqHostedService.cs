using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using IHostedService = Microsoft.Extensions.Hosting.IHostedService;

namespace MassTransitWithMSDependencyInjection
{
    public class RabbitMqHostedService : IHostedService
    {
        private readonly IBusControl _busControl;

        public RabbitMqHostedService(IBusControl busControl)
        {
            _busControl = busControl;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _busControl.StartAsync(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _busControl.StopAsync(cancellationToken);
        }
    }
}