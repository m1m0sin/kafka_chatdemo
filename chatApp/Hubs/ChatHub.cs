using System;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using chatApp.Services;
using chatApp.DAO;

namespace chatApp.Hubs;

public class ChatHub : Hub
{
    private static readonly ConcurrentDictionary<string, string> UserConnections = new();
    private static readonly ConcurrentDictionary<string, string> ConnectedUsers = new();

    private readonly IKafkaProducerService kafkaProducer;

    public ChatHub(IKafkaProducerService kafkaProducer)
    {
        this.kafkaProducer = kafkaProducer;
    }

    public async Task<bool> RegisterUser(string userName)
    {
        // Verifica si ya existe
        if (UserConnections.ContainsKey(userName))
        {
            return false;
        }

        var connectionId = Context.ConnectionId;
        ConnectedUsers[connectionId] = userName;
        UserConnections[userName] = connectionId;

        var chatMessage = new ChatMessage { FromUser = userName, ToUser = "", Message = $"{userName} has joined the chat", Type = ChatMessageType.Connected };

        // Notifica a todos que un nuevo usuario entró
        await kafkaProducer.ProduceAsync(chatMessage);

        return true;
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (ConnectedUsers.TryRemove(Context.ConnectionId, out var userName))
        {
            UserConnections.TryRemove(userName, out _);
            var chatMessage = new ChatMessage { FromUser = userName, ToUser = "", Message = $"{userName} just left the chat", Type = ChatMessageType.Disconnected };
            await kafkaProducer.ProduceAsync(chatMessage);
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessageToAll(string user, string message)
    {
        var chatMessage = new ChatMessage { FromUser = user, ToUser = "", Message = message, Type = ChatMessageType.Message };
        await kafkaProducer.ProduceAsync(chatMessage);
    }

    public async Task SendPrivateMessage(string fromUser, string toUser, string message)
    {
        if (UserConnections.TryGetValue(toUser, out var connectionId))
        {
            var chatMessage = new ChatMessage { FromUser = fromUser, ToUser = toUser, Message = message, Type = ChatMessageType.Message };
            await kafkaProducer.ProduceAsync(chatMessage);
        }
    }

    public static async Task<string?> GetConnectionId(string user)
    {
        UserConnections.TryGetValue(user, out var connectionId);
        return await Task.FromResult(connectionId);
    } 

    public static async Task<List<string>> GetActiveUsers()
    {
        return await Task.FromResult(UserConnections.Keys.ToList());
    }
}