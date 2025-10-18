namespace chatApp.Services;

public class KafkaSettings
{
    public string BootstrapServers { get; set; } = string.Empty;
    
    public string PublicTopic { get; set; } = string.Empty;

    public string PrivateTopic { get; set; } = string.Empty;

    public int NumPartitions { get; set; } = 1;

    public short ReplicationFactor { get; set; } = 1;
}
