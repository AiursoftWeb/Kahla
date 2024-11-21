using Aiursoft.AiurObserver;
using Aiursoft.AiurObserver.WebSocket;
using Aiursoft.Kahla.SDK.Models;

namespace Aiursoft.Kahla.SDK.Services;

public class KahlaMessagesRepo(string webSocketEndpoint)
{
    private ObservableWebSocket? _webSocket;
    private Task _listenTask = Task.CompletedTask;
    private readonly KahlaMessagesMemoryStore _messages = new();

    public async Task<KahlaMessagesRepo> ConnectAndMonitor(bool inCurrentThread = false)
    {
        var realEndpoint = webSocketEndpoint + "?start=" + _messages.PulledItemsOffset;
        _webSocket = await realEndpoint.ConnectAsWebSocketServer();
        await Push();
        _webSocket.Subscribe(OnNewWebSocketMessage);
        var listenTask = _webSocket.Listen();
        if (inCurrentThread)
        {
            await listenTask;
        }
        else
        {
            _listenTask = Task.Run(async () => await listenTask);
        }
        return this;
    }
    
    public async Task Send(ChatMessage message, bool waitShortTime = true)
    {
        _messages.Commit(message);
        await Push();
        if (waitShortTime)
        {
            await Task.Delay(100);
        }
    }
    
    public void CommitOnly(ChatMessage message)
    {
        _messages.Commit(message);
    }
    
    public IEnumerable<Commit<ChatMessage>> GetAllMessages()
    {
        return _messages.GetAllMessagesEnumerable();
    }

    public Commit<ChatMessage>? Head()
    {
        return _messages.GetHead();
    }

    public async Task WaitTilListenTaskComplete()
    {
        await _listenTask;
    }

    public async Task Disconnect()
    {
        if (_webSocket != null)
        {
            await _webSocket.Close();
        }

        _webSocket = null;
    }

    private Task OnNewWebSocketMessage(string content)
    {
        var commits = Extensions.Deserialize<Commit<ChatMessage>[]>(content);
        foreach (var commit in commits)
        {
            _messages.OnPulledMessage(commit);
        }
        return Task.CompletedTask;
    }

    private async Task Push()
    {
        if (_webSocket == null || !_webSocket.Connected)
        {
            throw new InvalidOperationException("WebSocket is not connected!");
        }

        var messages = _messages.Push().ToArray();
        if (messages.Any())
        {
            var content = Extensions.Serialize(messages);
            await _webSocket.Send(content);
        }
    }
}