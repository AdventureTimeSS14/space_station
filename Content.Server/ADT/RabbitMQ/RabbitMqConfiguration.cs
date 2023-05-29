using RabbitMQ.Client;

namespace Content.Server.ADT.RabbitMQ;

public sealed class RabbitMqConfiguration
{
    public ConnectionFactory CreateConnectionFactory(string url)
    {
        return new ConnectionFactory() {  DispatchConsumersAsync = true, Uri = new Uri(url) };
    }

    public string GetExchangeName()
    {
        return "SS14";
    }
}
