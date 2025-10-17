namespace chatApp.Services;

public class KafkaSettings
{
    public string BootstrapServers { get; set; } = string.Empty;
    
    public string PublicTopic { get; set; } = string.Empty;

    public string PrivateTopic { get; set; } = string.Empty;
}
