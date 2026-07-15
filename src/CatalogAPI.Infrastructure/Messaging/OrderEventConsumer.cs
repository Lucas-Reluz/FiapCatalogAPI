using System.Text;
using System.Text.Json;
using CatalogAPI.Domain.Events;
using CatalogAPI.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CatalogAPI.Infrastructure.Messaging;

public class OrderEventConsumer : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OrderEventConsumer> _logger;
    private IConnection? _connection;
    private IChannel? _channel;

    public OrderEventConsumer(
        IConfiguration configuration,
        IServiceProvider serviceProvider,
        ILogger<OrderEventConsumer> logger)
    {
        _configuration = configuration;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var rabbitMqSettings = _configuration.GetSection("RabbitMQ");
            var hostName = rabbitMqSettings["Host"] ?? "localhost";
            var port = int.Parse(rabbitMqSettings["Port"] ?? "5672");
            var userName = rabbitMqSettings["UserName"] ?? "guest";
            var password = rabbitMqSettings["Password"] ?? "guest";
            var queueName = rabbitMqSettings["OrderQueue"] ?? "catalog.orders.queue";
            var exchangeName = rabbitMqSettings["OrderExchange"] ?? "order.exchange";

            var factory = new ConnectionFactory
            {
                HostName = hostName,
                Port = port,
                UserName = userName,
                Password = password
            };

            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            // Declarar exchange e fila
            await _channel.ExchangeDeclareAsync(exchangeName, ExchangeType.Fanout, true, false);
            await _channel.QueueDeclareAsync(queueName, true, false, false);
            await _channel.QueueBindAsync(queueName, exchangeName, string.Empty);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var orderEvent = JsonSerializer.Deserialize<OrderCreatedEvent>(message);

                    if (orderEvent != null)
                    {
                        await ProcessOrderEventAsync(orderEvent);
                    }

                    await _channel.BasicAckAsync(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao processar mensagem");
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, true);
                }
            };

            await _channel.BasicConsumeAsync(queueName, false, consumer, stoppingToken);

            _logger.LogInformation("OrderEventConsumer iniciado e escutando fila {QueueName}", queueName);

            // Manter rodando até o cancellation
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("OrderEventConsumer cancelado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao iniciar OrderEventConsumer");
        }
    }

    private async Task ProcessOrderEventAsync(OrderCreatedEvent orderEvent)
    {
        _logger.LogInformation("Processando pedido {OrderId} para jogo {GameId} quantidade {Quantity}",
            orderEvent.OrderId, orderEvent.GameId, orderEvent.Quantity);

        using var scope = _serviceProvider.CreateScope();
        var gameRepository = scope.ServiceProvider.GetRequiredService<IGameRepository>();
        var eventPublisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();

        var game = await gameRepository.GetByIdAsync(orderEvent.GameId);

        if (game == null)
        {
            _logger.LogWarning("Jogo {GameId} não encontrado", orderEvent.GameId);
            return;
        }

        var success = game.TryReserveStock(orderEvent.Quantity);

        if (success)
        {
            await gameRepository.UpdateAsync(game);

            var stockReservedEvent = new StockReservedEvent(
                orderEvent.OrderId,
                game.Id,
                game.Title,
                orderEvent.Quantity
            );

            await eventPublisher.PublishAsync(stockReservedEvent);

            _logger.LogInformation("Estoque reservado para pedido {OrderId}", orderEvent.OrderId);
        }
        else
        {
            var stockInsufficientEvent = new StockInsufficientEvent(
                orderEvent.OrderId,
                game.Id,
                game.Title,
                orderEvent.Quantity,
                game.Stock
            );

            await eventPublisher.PublishAsync(stockInsufficientEvent);

            _logger.LogWarning("Estoque insuficiente para pedido {OrderId}", orderEvent.OrderId);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("OrderEventConsumer parando");

        if (_channel != null)
        {
            await _channel.CloseAsync();
            await _channel.DisposeAsync();
        }

        if (_connection != null)
        {
            await _connection.CloseAsync();
            await _connection.DisposeAsync();
        }

        await base.StopAsync(cancellationToken);
    }
}
