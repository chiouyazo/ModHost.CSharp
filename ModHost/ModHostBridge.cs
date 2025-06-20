using System.Net.Sockets;
using ModHost.Models;

namespace ModHost;

public class ModHostBridge : IDisposable
{
    private readonly string _modName;
    private readonly StreamWriter _writer;
    private readonly StreamReader _reader;
    private readonly TcpClient _client;
    private readonly Dictionary<string, TaskCompletionSource<string>> _pendingRequests = new();
    
    private readonly Dictionary<string, Func<CommandContext, Task>> _commandCallbacks = new Dictionary<string, Func<CommandContext, Task>>();
    private readonly Dictionary<string, Func<CommandSource, Task<bool>>?> _requirementCallbacks = new Dictionary<string, Func<CommandSource, Task<bool>>?>();
    
    public ModHostBridge(int port, string modName)
    {
        _modName = modName;
        _client = new TcpClient("127.0.0.1", port);
        
        NetworkStream stream = _client.GetStream();
        _reader = new StreamReader(stream);
        _writer = new StreamWriter(stream) { AutoFlush = true };
        
        Task.Run(ListenForResponses);
    }

    private async Task ListenForResponses()
    {
        await _writer.WriteLineAsync($"{Guid.NewGuid().ToString()}:HELLO:{_modName}");
        
        while (true)
        {
            string? line = await _reader.ReadLineAsync();
            if (line == null) break;

            string[] parts = line.Split(new[] { ':' }, 3);
            if (parts.Length < 3) continue;

            string id = parts[0];
            string eventType = parts[1];
            string payload = parts[2];

            if (_pendingRequests.TryGetValue(id, out TaskCompletionSource<string>? tcs))
            {
                tcs.SetResult(line);
                _pendingRequests.Remove(id);
            }
            else
            {
                // handle unsolicited events here
                await Task.Run(() => HandleEvent(id, eventType, payload));
            }
        }
    }

    private async Task HandleEvent(string id, string eventType, string payload)
    {
        if (eventType == "COMMAND_EXECUTED")
        {
            // Example payload: "command_with_arg|arg1=123,arg2=hello"
            string[] parts = payload.Split(':', 2);
            
            string commandName = parts[0];
            string commandPayload = parts.Length > 1 ? parts[1] : string.Empty;

            Dictionary<string, string> argsDict = commandPayload.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(pair => pair.Split('=', 2))
                .ToDictionary(kv => kv[0], kv => kv.Length > 1 ? kv[1] : "");
            
            if (_commandCallbacks.TryGetValue(commandName, out Func<CommandContext, Task>? callback))
            {
                CommandContext context = new CommandContext(this, id, commandName, commandPayload);
                _ = callback(context);
            }
        }
        else if (eventType == "COMMAND_REGISTERED")
        {
            string[] parts = payload.Split('|', 2);
            
            string commandName = parts[0];
            string commandPayload = parts.Length > 1 ? parts[1] : string.Empty;
            
            if (_commandCallbacks.TryGetValue(commandName, out Func<CommandContext, Task>? callback))
            {
                CommandContext context = new CommandContext(this, id, commandName, commandPayload);
                _ = callback(context);
            }
        }
        else if (eventType == "COMMAND_REQUIREMENT")
        {
            // Example payload: someCommand
            string[] split = payload.Split(':', 2);
            string commandName = split[0];
            string sourcePayload = split.Length > 1 ? split[1] : "";

            if (_requirementCallbacks.TryGetValue(commandName, out Func<CommandSource, Task<bool>>? callback))
            {
                CommandSource ctx = new CommandSource(this, id, commandName);
                bool allowed = callback == null || await callback(ctx);

                await _writer.WriteLineAsync($"{id}:COMMAND_REQUIREMENT_RESPONSE:{allowed}");
            }
        }
        else
        {
            Console.WriteLine($"Unhandled event: {eventType} - {payload}");
        }
    }

    public async Task<string> SendRequestAsync(string id, string eventType, string payload = "")
    {
        TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();
        _pendingRequests[id] = tcs;

        await _writer.WriteLineAsync($"{id}:{eventType}:{payload}");
        await _writer.FlushAsync();

        return await tcs.Task;
    }
	
    public async Task ExecuteMinecraftCommandAsync(string rawCommand)
    {
        string id = Guid.NewGuid().ToString();
        // Returns either COMMAND_RESULT:Success COMMAND_RESULT:Error:(error) or COMMAND_RESULT:Server not available but I dont think it matters here
        await SendRequestAsync(id, "EXECUTE_COMMAND", rawCommand);
    }

    public async Task SendCommandFeedback(string commandId, string feedback)
    {
        string id = Guid.NewGuid().ToString();
        // Returns "{id}:COMMAND_FEEDBACK:OK" but doesnt matter?
        await SendRequestAsync(id, "COMMAND_FEEDBACK", $"{commandId}:{feedback}");
    }

    public async Task FinalizeCommand(string commandId)
    {
        string id = Guid.NewGuid().ToString();
        // Returns "{id}:COMMAND_FINALIZE:OK" but doesnt matter?
        await SendRequestAsync(id, "COMMAND_FINALIZE", commandId);
    }
    
    public async Task RegisterCommandAsync(string commandName, Func<CommandContext, Task> onExecuted, Func<CommandSource, Task<bool>>? requirement = null)
    {
        await RegisterManualCommandAsync(commandName, async context =>
        {
            await onExecuted(context);
            if (!context.Finalized)
                await context.FinalizeAsync();
        }, requirement);
    }
    
    public async Task RegisterCommandAsync(string commandName, List<CommandArgument> arguments, Func<CommandContext, Task> onExecuted, Func<CommandSource, Task<bool>>? requirement = null)
    {
        await RegisterManualCommandAsync(commandName, arguments, async context =>
        {
            await onExecuted(context);
            if (!context.Finalized)
                await context.FinalizeAsync();
        }, requirement);
    }

    public async Task RegisterManualCommandAsync(string commandName, Func<CommandContext, Task> onExecuted, Func<CommandSource, Task<bool>>? requirement = null)
    {
        string id = Guid.NewGuid().ToString();
        
        string response = await SendRequestAsync(id, "REGISTER_COMMAND", commandName);
        
        if (response == $"{id}:COMMAND_REGISTERED:{commandName}")
        {
            Console.WriteLine($"Registered command {commandName} successfully from C#.");
            _commandCallbacks.Add(commandName, onExecuted);
            _requirementCallbacks.Add(commandName, requirement);
        }
        else
        {
            throw new Exception($"Command registration failed: {response}");
        }
    }

    public async Task RegisterManualCommandAsync(string commandName, List<CommandArgument> arguments, Func<CommandContext, Task> onExecuted, Func<CommandSource, Task<bool>>? requirement = null)
    {
        string id = Guid.NewGuid().ToString();
        
        string argsPayload = string.Join(",", arguments.Select(arg => 
            $"{arg.Name}:{arg.Type}|{(arg.IsOptional ? "optional" : "required")}"));
        
        string payload = $"{commandName}|{argsPayload}";
        
        string response = await SendRequestAsync(id, "REGISTER_COMMAND", payload);
        // 1ff44493-4788-47ac-a87f-bd0dac6fcbdf:COMMAND_REGISTERED:testTest|Tester:integer|required,SecondTester:integer|optional
        if (response == $"{id}:COMMAND_REGISTERED:{payload}")
        {
            Console.WriteLine($"Registered command '{commandName}' with args successfully from C#.");
            _commandCallbacks.Add(commandName, onExecuted);
            _requirementCallbacks.Add(commandName, requirement);
        }
        else
        {
            throw new Exception($"Command with args registration failed: {response}");
        }
    }

    public void Dispose()
    {
        _writer.Dispose();
        _reader.Dispose();
        _client.Dispose();
    }
}