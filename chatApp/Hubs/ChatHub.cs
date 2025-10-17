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

        // Notifica a todos que un nuevo usuario entró
        await Clients.All.SendAsync("UserConnected", userName, UserConnections.Keys);

        return true;
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (ConnectedUsers.TryRemove(Context.ConnectionId, out var userName))
        {
            UserConnections.TryRemove(userName, out _);
            await Clients.All.SendAsync("UserDisconnected", userName, UserConnections.Keys);
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessageToAll(string user, string message)
    {
        var chatMessage = new ChatMessage { FromUser = user, ToUser = "", Message = message };
        await kafkaProducer.ProduceAsync(chatMessage);
    }

    public async Task SendPrivateMessage(string fromUser, string toUser, string message)
    {
        if (UserConnections.TryGetValue(toUser, out var connectionId))
        {
            var chatMessage = new ChatMessage { FromUser = fromUser, ToUser = toUser, Message = message };
            await kafkaProducer.ProduceAsync(chatMessage);
        }
    }

    public static string? GetConnectionId(string user)
    {
        UserConnections.TryGetValue(user, out var connectionId);
        return connectionId;
    } 

    public Task<List<string>> GetActiveUsers()
    {
        return Task.FromResult(UserConnections.Keys.ToList());
    }
}