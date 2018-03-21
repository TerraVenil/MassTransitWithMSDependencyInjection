namespace MassTransitWithMSDependencyInjection
{
    public interface ICorrelationScoped
    {
        string CorrelationId { get; set; }
    }

    public class CorrelationScoped : ICorrelationScoped
    {
        public string CorrelationId { get; set; }
    }
}