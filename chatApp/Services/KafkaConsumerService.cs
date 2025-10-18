using Confluent.Kafka;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using chatApp.Hubs;
using System.Text.Json;
using chatApp.DAO;

namespace chatApp.Services;

public class KafkaConsumerService : BackgroundService, IKfakaConsumerService
{
    private readonly IHubContext<ChatHub> _hubContext;
    private readonly KafkaSettings _settings;
    private IConsumer<string, string>? _consumer;

    public KafkaConsumerService(IHubContext<ChatHub> hubContext, IOptions<KafkaSettings> settings)
    {
        _hubContext = hubContext;
        _settings = settings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();

        var config = new ConsumerConfig
        {
            BootstrapServers = _settings.BootstrapServers,
            GroupId = "chat-consumer-group",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true
        };

        _consumer = new ConsumerBuilder<string, string>(config).Build();
        _consumer.Subscribe(new[] { _settings.PublicTopic, _settings.PrivateTopic });

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var cr = _consumer.Consume(stoppingToken);
                    if (cr?.Message?.Value == null) continue;

                    var msg = JsonSerializer.Deserialize<ChatMessage>(cr.Message.Value);
                    if (msg == null) continue;

                    var toConn = await ChatHub.GetConnectionId(msg.ToUser!);
                    var activeUsers = await ChatHub.GetActiveUsers();

                    if (toConn == null)
                    {
                        switch (msg.Type)
                        {
                            case ChatMessageType.Connected:
                                await _hubContext.Clients.All.SendAsync("UserConnected", msg.FromUser, activeUsers, cancellationToken: stoppingToken);
                                break;
                            case ChatMessageType.Disconnected:
                                await _hubContext.Clients.All.SendAsync("UserDisconnected", msg.FromUser, activeUsers, cancellationToken: stoppingToken);
                                break;
                            case ChatMessageType.Message:
                                await _hubContext.Clients.All.SendAsync("ReceivePublicMessage", msg.FromUser, msg.Message, cancellationToken: stoppingToken);
                                break;
                        }
                        
                    }
                    else
                    {
                        await _hubContext.Clients.Client(toConn).SendAsync("ReceivePrivateMessage", msg.FromUser, msg.Message, cancellationToken: stoppingToken);
                    }
                }
                catch (ConsumeException ex)
                {
                    Console.WriteLine($"Kafka consume error: {ex.Error.Reason}");
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"Error deserializando mensaje: {ex.Message}");
                }
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Cancelación solicitada, cerrando consumidor Kafka...");
        }
        finally
        {
            _consumer?.Close();
            _consumer?.Dispose();
            Console.WriteLine("Consumidor Kafka cerrado correctamente.");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Deteniendo KafkaConsumerService...");
        _consumer?.Close();
        _consumer?.Dispose();
        await base.StopAsync(cancellationToken);
        Console.WriteLine("KafkaConsumerService detenido correctamente.");
    }
}
