using System;
using GreenPipes;
using MassTransit;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using MassTransitWithMSDependencyInjection.Middleware;
using MassTransitWithMSDependencyInjection.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using IHostedService = Microsoft.Extensions.Hosting.IHostedService;

namespace MassTransitWithMSDependencyInjection
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<ICorrelationScoped, CorrelationScoped>();
            services.AddScoped<IEventService, EventService>();

            services.AddMassTransit(s =>
            {
                s.AddConsumer<EventsConsumer>();
            });

            services.AddSingleton<IHostedService, RabbitMqHostedService>();

            services.AddSingleton(s =>
            {
                var busControl = Bus.Factory.CreateUsingRabbitMq(configuration =>
                {
                    var queueUri = new Uri("rabbitmq://localhost/");

                    configuration.UseJsonSerializer();
                    configuration.UseCorrelationMiddleware(s);

                    var host = configuration.Host(queueUri, h =>
                    {
                        h.Username("guest");
                        h.Password("guest");
                    });

                    configuration.ReceiveEndpoint(host, "MassTransitWithMSDependencyInjection.EventsQueue", c =>
                    {
                        c.UseInMemoryOutbox();
                        c.UseRetry(configurator => { configurator.Interval(2, TimeSpan.FromSeconds(5)); });
                        c.LoadFrom(s);
                    });

                    configuration.ConfigurePublish(x => x.UseSendExecute(context =>
                    {
                        PublishContext publishContext;
                        SendContext sendContext;
                        if (context.TryGetPayload(out ConsumeContext _))
                        {
                            var correlationScoped = s.GetService<ICorrelationScoped>();
                            context.Headers.Set("CorrelationIdSсoped", correlationScoped.CorrelationId);
                        }

                        if (context.TryGetPayload(out publishContext))
                        {
                            var correlationScoped = s.GetService<ICorrelationScoped>();
                            context.Headers.Set("CorrelationIdSсoped", correlationScoped.CorrelationId);
                        }

                        if (context.TryGetPayload(out sendContext))
                        {
                            var correlationScoped = s.GetService<ICorrelationScoped>();
                            context.Headers.Set("CorrelationIdSсoped", correlationScoped.CorrelationId);
                        }
                    }));
                });

                return busControl;
            });

            services.AddSingleton<IBus>(serviceProvider => serviceProvider.GetRequiredService<IBusControl>());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Hello World!");
            });
        }
    }
}
