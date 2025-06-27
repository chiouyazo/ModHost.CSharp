using System.Net.Sockets;
using ModHost.Handlers;
using ModHost.Models;

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
        while (true)
        {
            try
            {
                _client = new TcpClient("127.0.0.1", port);
                break;
            }
            catch (SocketException)
            {
                Console.WriteLine("Connection refused. Retrying in .5 second...");
                Thread.Sleep(500);
            }
        }
        
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
        await _writer.WriteLineAsync($"{Guid.NewGuid().ToString()}:SERVER:SERVER:HELLO:{_modName}");

        int currEx = 0;
        int recurringEx = 0;
        
        while (true)
        {
            try
            {
                string? line = await _reader.ReadLineAsync();
                if (line == null) 
                    continue;

                string[] parts = line.Split(new[] { ':' }, 5);
                if (parts.Length < 5)
                {
                    Console.WriteLine($"Could not deserialize response {line}.");
                    continue;
                }

                string id = parts[0];
                string platform = parts[1];
                string handler = parts[2];
                string eventType = parts[3];
                string payload = parts[4];

                if (_pendingRequests.TryGetValue(id, out TaskCompletionSource<string>? tcs))
                {
                    tcs.SetResult(payload);
                    _pendingRequests.Remove(id);
                }
                else
                {
                    // handle unsolicited events here
                    await Task.Run(() => HandleEvent(id, platform, handler, eventType, payload));
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

    private async Task HandleEvent(string id, string platform, string handler, string eventType, string payload)
    {
        if (platform == "CLIENT")
        {
            await _clientBridge.HandleEvent(id, platform, handler, eventType, payload);
            return;
        }
        
        if (handler == "COMMAND")
            _commandHandler.HandleEvent(id, platform, handler, eventType, payload);
        else
        {
            Console.WriteLine($"Unknown handler: {handler}:{eventType}");
        }
    }

    public async Task<string> SendRequestAsync(string id, string platform, string handler, string eventType, string payload = "")
    {
        TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();
        _pendingRequests[id] = tcs;

        await _writer.WriteLineAsync($"{id}:{platform}:{handler}:{eventType}:{payload}");
        await _writer.FlushAsync();

        return await tcs.Task;
    }

    public async Task SendResponse(string id, string platform, string handler, string eventType, string payload)
    {
        await _writer.WriteLineAsync($"{id}:{platform}:{handler}:{eventType}:{payload}");
    }

    public void Dispose()
    {
        _writer.Dispose();
        _reader.Dispose();
        _client.Dispose();
    }
}