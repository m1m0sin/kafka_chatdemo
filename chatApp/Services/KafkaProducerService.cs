using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using chatApp.DAO;

namespace chatApp.Services;

public class KafkaProducerService : IKafkaProducerService
{
    private readonly KafkaSettings settings;

    public KafkaProducerService(IOptions<KafkaSettings> settings)
    {
        this.settings = settings.Value;
    }

    public async Task ProduceAsync(ChatMessage msg)
    {
        var config = new ProducerConfig { BootstrapServers = settings.BootstrapServers };
        using var producer = new ProducerBuilder<string, string>(config).Build();

        var topic = msg.IsPrivate ? settings.PrivateTopic : settings.PublicTopic;
        var value = JsonSerializer.Serialize(msg);

        var key = msg.FromUser ?? Guid.NewGuid().ToString();

        await producer.ProduceAsync(topic, new Message<string, string>
        {
            Key = key,
            Value = value
        });
    }
}