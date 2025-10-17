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

    public KafkaConsumerService(IHubContext<ChatHub> hubContext, IOptions<KafkaSettings> settings)
    {
        _hubContext = hubContext;
        _settings = settings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _settings.BootstrapServers,
            GroupId = "chat-consumer-group",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe([_settings.PublicTopic, _settings.PrivateTopic]);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var cr = consumer.Consume(stoppingToken);
                var msg = JsonSerializer.Deserialize<ChatMessage>(cr.Message.Value);

                if (msg == null) continue;

                var toConn = ChatHub.GetConnectionId(msg.ToUser!);
                var fromConn = ChatHub.GetConnectionId(msg.FromUser);

                if (toConn != null)
                        await _hubContext.Clients.Client(toConn).SendAsync("ReceivePrivateMessage", msg.FromUser, msg.Message);
            }
        }
        catch (OperationCanceledException)
        {
            consumer.Close();
        }
    }
}