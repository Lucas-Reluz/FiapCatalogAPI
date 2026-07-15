using System.Text;
using System.Text.Json;
using CatalogAPI.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace CatalogAPI.Infrastructure.Messaging;

public class RabbitMqPublisher : IEventPublisher
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<RabbitMqPublisher> _logger;

    public RabbitMqPublisher(IConfiguration configuration, ILogger<RabbitMqPublisher> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task PublishAsync<T>(T eventMessage) where T : class
    {
        try
        {
            var rabbitMqSettings = _configuration.GetSection("RabbitMQ");
            var hostName = rabbitMqSettings["Host"] ?? "localhost";
            var port = int.Parse(rabbitMqSettings["Port"] ?? "5672");
            var userName = rabbitMqSettings["UserName"] ?? "guest";
            var password = rabbitMqSettings["Password"] ?? "guest";
            var exchangeName = rabbitMqSettings["CatalogExchange"] ?? "catalog.exchange";

            var factory = new ConnectionFactory
            {
                HostName = hostName,
                Port = port,
                UserName = userName,
                Password = password
            };

            await using var connection = await factory.CreateConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();

            // Declarar exchange (fanout para broadcast)
            await channel.ExchangeDeclareAsync(
                exchange: exchangeName,
                type: ExchangeType.Fanout,
                durable: true,
                autoDelete: false
            );

            var jsonMessage = JsonSerializer.Serialize(eventMessage);
            var body = Encoding.UTF8.GetBytes(jsonMessage);

            await channel.BasicPublishAsync(
                exchange: exchangeName,
                routingKey: string.Empty,
                body: body
            );

            _logger.LogInformation("Evento {EventType} publicado com sucesso em {Exchange}", 
                typeof(T).Name, exchangeName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao publicar evento {EventType}", typeof(T).Name);
            // Não propagar exceção para não bloquear o fluxo principal
        }
    }
}
