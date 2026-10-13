using System.Globalization;
using System.Text.Json;
using CardService.Application.Events;
using CardService.Application.UseCases;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace CardService.Infrastructure.Messaging;

public sealed class CreditApprovedConsumerService(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<CreditApprovedConsumerService> logger) : BackgroundService
{
    private const string ExchangeName = "credit.events";
    private const string QueueName = "card.credit-approved";
    private const string DeadLetterExchangeName = "card.events.dlx";
    private const string DeadLetterQueueName = "card.credit-approved.dead-letter";
    private const string RoutingKey = "credit.approved";
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var userName = configuration["RabbitMq:UserName"]
            ?? throw new InvalidOperationException("RabbitMq:UserName is required.");
        var password = configuration["RabbitMq:Password"]
            ?? throw new InvalidOperationException("RabbitMq:Password is required.");
        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMq:HostName"] ?? "localhost",
            Port = int.Parse(
                configuration["RabbitMq:Port"] ?? "5672",
                CultureInfo.InvariantCulture),
            UserName = userName,
            Password = password,
            ConsumerDispatchConcurrency = 1
        };

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ConsumeAsync(factory, stoppingToken);
            }
            catch (BrokerUnreachableException exception)
            {
                logger.LogWarning(exception, "RabbitMQ is unavailable; Card Service will retry.");
                await Task.Delay(RetryDelay, stoppingToken);
            }
            catch (OperationInterruptedException exception)
            {
                logger.LogWarning(exception, "RabbitMQ consumer was interrupted; Card Service will reconnect.");
                await Task.Delay(RetryDelay, stoppingToken);
            }
        }
    }

    private async Task ConsumeAsync(ConnectionFactory factory, CancellationToken cancellationToken)
    {
        await using var connection = await factory.CreateConnectionAsync(cancellationToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.ExchangeDeclareAsync(
            ExchangeName,
            ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);
        await channel.ExchangeDeclareAsync(
            DeadLetterExchangeName,
            ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);
        await channel.QueueDeclareAsync(
            DeadLetterQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken);
        await channel.QueueBindAsync(
            DeadLetterQueueName,
            DeadLetterExchangeName,
            RoutingKey,
            cancellationToken: cancellationToken);

        var queueArguments = new Dictionary<string, object?>
        {
            ["x-dead-letter-exchange"] = DeadLetterExchangeName,
            ["x-dead-letter-routing-key"] = RoutingKey
        };
        await channel.QueueDeclareAsync(
            QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: queueArguments,
            cancellationToken: cancellationToken);
        await channel.QueueBindAsync(
            QueueName,
            ExchangeName,
            RoutingKey,
            cancellationToken: cancellationToken);
        await channel.BasicQosAsync(0, 10, global: false, cancellationToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, delivery) =>
        {
            try
            {
                var message = JsonSerializer.Deserialize<CreditApprovedEvent>(
                    delivery.Body.Span,
                    JsonOptions) ?? throw new InvalidDataException("Credit event payload is empty.");
                await using var scope = scopeFactory.CreateAsyncScope();
                var handler = scope.ServiceProvider.GetRequiredService<CreditApprovedEventHandler>();
                var created = await handler.HandleAsync(message, cancellationToken);

                await channel.BasicAckAsync(delivery.DeliveryTag, multiple: false, cancellationToken);
                logger.LogInformation(
                    created
                        ? "Created card for credit application {CreditApplicationId} from event {EventId}."
                        : "Ignored duplicate approval event {EventId}.",
                    message.CreditApplicationId,
                    message.EventId);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                logger.LogError(exception, "Credit approval delivery failed and was sent to the dead-letter queue.");
                await channel.BasicNackAsync(
                    delivery.DeliveryTag,
                    multiple: false,
                    requeue: false,
                    cancellationToken);
            }
        };

        await channel.BasicConsumeAsync(
            QueueName,
            autoAck: false,
            consumer,
            cancellationToken);
        logger.LogInformation(
            "Consuming {RoutingKey} from exchange {Exchange} using queue {Queue}.",
            RoutingKey,
            ExchangeName,
            QueueName);
        await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
    }
}
