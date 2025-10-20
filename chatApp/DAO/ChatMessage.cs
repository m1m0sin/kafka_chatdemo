namespace chatApp.DAO;

using System.Text.Json.Serialization;

public class ChatMessage
{
    [JsonPropertyName("id")]
    public Guid Id => Guid.NewGuid();

    [JsonPropertyName("type")]
    public ChatMessageType Type { get; set; } = ChatMessageType.Message;
    
    [JsonPropertyName("from_user")]
    public string FromUser { get; set; } = "";

    [JsonPropertyName("to_user")]
    public string ToUser { get; set; } = "";

    [JsonPropertyName("message")]
    public string Message { get; set; } = "";

    [JsonPropertyName("is_private")]
    public bool IsPrivate => !string.IsNullOrEmpty(ToUser);

    [JsonPropertyName("message_date")]
    public DateTimeOffset Date => DateTimeOffset.Now;
}