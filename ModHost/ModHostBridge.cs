using System.Net.Sockets;
using System.Text.Json;
using ModHost.Handlers;
using ModHost.Models;
using ModHost.Models.Communication;

namespace ModHost;

public class ModHostBridge : IDisposable
{
    private readonly string _modName;
    private readonly StreamWriter _writer;
    private readonly StreamReader _reader;
    private readonly TcpClient _client;
    private readonly Dictionary<string, TaskCompletionSource<string>> _pendingRequests = new Dictionary<string, TaskCompletionSource<string>>();

    private readonly ClientBridge _clientBridge;
    
    private readonly CommandHandler _commandHandler;
    private readonly ItemHandler _itemHandler;
    private readonly BlockHandler _blockHandler;
    
    public ModHostBridge(int port, string modName)
    {
        _clientBridge = new ClientBridge(this);
        _commandHandler = new CommandHandler(this, false);
        _itemHandler = new ItemHandler(this);
        _blockHandler = new BlockHandler(this);
        
        _modName = modName;
        Console.WriteLine("Trying to establish connection to mod host...");
        while (true)
        {
            try
            {
                _client = new TcpClient("127.0.0.1", port);
                break;
            }
            catch (SocketException)
            {
                Thread.Sleep(250);
            }
        }
        Console.WriteLine("Established connection to mod host successfully.");
        
        NetworkStream stream = _client.GetStream();
        _reader = new StreamReader(stream);
        _writer = new StreamWriter(stream) { AutoFlush = true };
        
        Task.Run(ListenForResponses);
    }

    public CommandHandler GetCommandHandler()
    {
        return _commandHandler;
    }

    public CommandHandler GetClientCommandHandler()
    {
        return _clientBridge.GetCommandHandler();
    }

    public ScreenHandler GetScreenHandler()
    {
        return _clientBridge.GetScreenHandler();
    }

    public ItemHandler GetItemHandler()
    {
        return _itemHandler;
    }

    public BlockHandler GetBlockHandler()
    {
        return _blockHandler;
    }

    private async Task ListenForResponses()
    {
        // await _writer.WriteLineAsync($"{Guid.NewGuid().ToString()}:SERVER:SERVER:HELLO:{_modName}");

        int currEx = 0;
        int recurringEx = 0;
        
        while (true)
        {
            try
            {
                string? line = await _reader.ReadLineAsync();
                if (line == null) 
                    continue;

                MessageBase? message;
                try
                {
                    message = JsonSerializer.Deserialize<MessageBase>(line);
                }
                catch (JsonException)
                {
                    Console.WriteLine($"Invalid JSON: {line}");
                    continue;
                }
                
                if (message == null || string.IsNullOrWhiteSpace(message.Id)) 
                    continue;

                if (_pendingRequests.TryGetValue(message.Id, out TaskCompletionSource<string>? tcs))
                {
                    tcs.SetResult(message.GetPayload().Replace("\u0022", ""));
                    _pendingRequests.Remove(message.Id);
                }
                else
                {
                    await HandleEvent(message);
                }
            }
            catch (Exception ex)
            {
                if (currEx == ex.HResult)
                {
                    recurringEx++;
                }
                else
                {
                    currEx = ex.HResult;
                    recurringEx = 0;
                }

                if (recurringEx > 2)
                {
                    Console.WriteLine("Exited listening loop because minecraft has probably exited.");
                    break;
                }
                Console.WriteLine("An error occured while reading data:");
                Console.WriteLine(ex.ToString());
            }
        }
    }

    private async Task HandleEvent(MessageBase message)
    {
        if (message.Platform == "CLIENT")
        {
            await _clientBridge.HandleEvent(message);
            return;
        }

        if (message.Handler == "COMMAND")
            _commandHandler.HandleEvent(message);
        else
            Console.WriteLine($"Unknown handler: {message.Handler}:{message.Event}");
    }

    public async Task<string> SendRequestAsync(string id, string platform, string handler, string eventType, object payload)
    {
        TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();
        _pendingRequests[id] = tcs;
        
        MessageBase message = new MessageBase()
        {
            Id = id,
            Platform = platform,
            Handler = handler,
            Event = eventType,
            Payload = payload
        };

        string json = JsonSerializer.Serialize(message);
        await _writer.WriteLineAsync(json);
        await _writer.FlushAsync();

        return await tcs.Task;
    }

    public async Task SendResponse(string id, string platform, string handler, string eventType, string payload)
    {
        MessageBase message = new MessageBase()
        {
            Id = id,
            Platform = platform,
            Handler = handler,
            Event = eventType,
            Payload = payload
        };
        
        string json = JsonSerializer.Serialize(message);
        await _writer.WriteLineAsync(json);
        await _writer.FlushAsync();
    }

    public void Dispose()
    {
        _writer.Dispose();
        _reader.Dispose();
        _client.Dispose();
    }
}