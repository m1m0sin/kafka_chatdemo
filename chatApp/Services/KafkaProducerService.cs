using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using System.Text.Json.Serialization;
using System.Reflection;
using chatApp.DAO;

namespace chatApp.Services;

public class KafkaProducerService : IKafkaProducerService
{
    private readonly KafkaSettings settings;
    private readonly object jsonSchema = GenerateKafkaConnectSchema(typeof(ChatMessage));

    public KafkaProducerService(IOptions<KafkaSettings> settings)
    {
        this.settings = settings.Value;
    }

    public async Task ProduceAsync(ChatMessage msg)
    {
        var config = new ProducerConfig { BootstrapServers = settings.BootstrapServers };
        using var producer = new ProducerBuilder<string, string>(config).Build();

        var topic = msg.IsPrivate ? settings.PrivateTopic : settings.PublicTopic;

        var key = msg.FromUser ?? Guid.NewGuid().ToString();

        var messageWithSchema = new
        {
            schema = jsonSchema,
            payload = msg
        };

        var value = JsonSerializer.Serialize(messageWithSchema);

        await producer.ProduceAsync(topic, new Message<string, string>
        {
            Key = key,
            Value = value
        });
    }

    private static string GetJsonPropertyName(PropertyInfo prop)
    {
        var attr = prop.GetCustomAttribute<JsonPropertyNameAttribute>();
        if (attr != null)
            return attr.Name;

        return prop.Name;
    }

    private static string MapToKafkaType(Type type)
    {
        if (type == typeof(string) || type == typeof(Guid)) return "string";
        if (type == typeof(int) || type.IsEnum) return "int32";
        if (type == typeof(long)) return "int64";
        if (type == typeof(bool)) return "boolean";
        if (type == typeof(double) || type == typeof(float) || type == typeof(decimal)) return "float64";
        if (type == typeof(DateTime) || type == typeof(DateTimeOffset)) return "string";
        return "string";
    }

    private static object GenerateKafkaConnectSchema(Type type)
    {
        return new
        {
            type = "struct",
            fields = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(p => new
                {
                    field = GetJsonPropertyName(p),
                    type = MapToKafkaType(p.PropertyType)
                })
                .ToArray()
        };
    }
}