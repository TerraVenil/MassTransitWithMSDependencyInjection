using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GreenPipes;
using GreenPipes.Specifications;
using MassTransit;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using MassTransit.Pipeline.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace MassTransitWithMSDependencyInjection.Middleware
{
    public class CorrelationFilter<T> : IFilter<T> where T : class, ConsumeContext
    {
        public void Probe(ProbeContext context)
        {
        }

        public async Task Send(T context, IPipe<T> next)
        {
            string correlationId = context.Headers.Get<string>("CorrelationId");
            if (context.TryGetPayload(out IServiceScope scope))
            {
                var correlationLogManager = scope.ServiceProvider.GetRequiredService<ICorrelationScoped>();
                correlationLogManager.CorrelationId = correlationId;
            }

            await next.Send(context).ConfigureAwait(false);
        }
    }

    public class CorrelationSpecification<T> : IPipeSpecification<T> where T : class, ConsumeContext
    {
        public void Apply(IPipeBuilder<T> builder)
        {
            builder.AddFilter(new CorrelationFilter<T>());
        }

        public IEnumerable<ValidationResult> Validate()
        {
            return Enumerable.Empty<ValidationResult>();
        }
    }

    public static class MiddlewareConfiguratorExtensions
    {
        public static void UseLifetimeScope(this IPipeConfigurator<ConsumeContext> configurator, IServiceProvider serviceProvider)
        {
            var scopeProvider = new DependencyInjectionConsumerScopeProvider(serviceProvider);
            var specification = new FilterPipeSpecification<ConsumeContext>(new ScopeFilter(scopeProvider));

            configurator.AddPipeSpecification(specification);
        }

        public static void UseCorrelationMiddleware(this IPipeConfigurator<ConsumeContext> configurator, IServiceProvider serviceProvider)
        {
            var scopeProvider = new DependencyInjectionConsumerScopeProvider(serviceProvider);

            configurator.AddPipeSpecification(new FilterPipeSpecification<ConsumeContext>(new ScopeFilter(scopeProvider)));
            configurator.AddPipeSpecification(new CorrelationSpecification<ConsumeContext>());
        }
    }
}