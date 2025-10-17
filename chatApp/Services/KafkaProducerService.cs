using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using chatApp.DAO;

namespace chatApp.Services;

public class KafkaProducerService : IKafkaProducerService
{
    private readonly IProducer<string, string> producer;
    private readonly KafkaSettings settings;

    public KafkaProducerService(IOptions<KafkaSettings> settings)
    {
        var config = new ProducerConfig { BootstrapServers = settings.Value.BootstrapServers };
        producer = new ProducerBuilder<string, string>(config).Build();
        this.settings = settings.Value;
    }

    public async Task ProduceAsync(ChatMessage msg)
    {
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