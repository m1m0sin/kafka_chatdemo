namespace chatApp.Services;

using chatApp.DAO;

public interface IKafkaProducerService
{
    Task ProduceAsync(ChatMessage msg);
}
