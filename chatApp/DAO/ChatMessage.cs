namespace chatApp.DAO;

public class ChatMessage
{
    public ChatMessageType Type { get; set; } = ChatMessageType.Message;
    
    public string FromUser { get; set; } = "";

    public string ToUser { get; set; } = "";

    public string Message { get; set; } = "";

    public bool IsPrivate => !string.IsNullOrEmpty(ToUser);

    public DateTimeOffset Date => DateTimeOffset.Now;
}