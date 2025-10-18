using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Options;


namespace chatApp.Services;

public class KafkaTopicInitializer : IKafkaTopicInitializer
{
    private readonly KafkaSettings _settings;

    public KafkaTopicInitializer(IOptions<KafkaSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task EnsureTopicsExistAsync()
    {
        using var adminClient = new AdminClientBuilder(new AdminClientConfig
        {
            BootstrapServers = _settings.BootstrapServers
        }).Build();

        var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(10));
        var existingTopics = metadata.Topics.Select(t => t.Topic).ToHashSet();

        var topicsToCreate = new List<TopicSpecification>();

        void AddIfMissing(string topic)
        {
            if (!existingTopics.Contains(topic))
            {
                topicsToCreate.Add(new TopicSpecification
                {
                    Name = topic,
                    NumPartitions = _settings.NumPartitions,
                    ReplicationFactor = _settings.ReplicationFactor
                });
            }
        }

        AddIfMissing(_settings.PublicTopic);
        AddIfMissing(_settings.PrivateTopic);

        if (topicsToCreate.Any())
        {
            Console.WriteLine($"Creando topics Kafka: {string.Join(", ", topicsToCreate.Select(t => t.Name))}");
            try
            {
                await adminClient.CreateTopicsAsync(topicsToCreate);
                Console.WriteLine("Topics creados correctamente.");
            }
            catch (CreateTopicsException e)
            {
                foreach (var result in e.Results)
                    Console.WriteLine($"Error creando topic {result.Topic}: {result.Error.Reason}");
            }
        }
        else
        {
            Console.WriteLine("Todos los topics Kafka ya existen.");
        }
    }
}