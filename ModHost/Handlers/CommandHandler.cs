using ModHost.Models;
using ModHost.Models.Server;

namespace ModHost.Handlers;

public class CommandHandler
{
	private readonly string _platform = "SERVER";
	
	internal readonly ModHostBridge Bridge;
	private readonly Dictionary<string, Func<CommandContext, Task>?> _commandCallbacks = new Dictionary<string, Func<CommandContext, Task>?>();
	private readonly Dictionary<string, Func<ServerCommandSource, Task<bool>>?> _requirementCallbacks = new Dictionary<string, Func<ServerCommandSource, Task<bool>>?>();
    private readonly Dictionary<string, Func<string, ServerCommandSource, Task<IEnumerable<string>>>> _suggestionCallbacks = new Dictionary<string, Func<string, ServerCommandSource, Task<IEnumerable<string>>>>();
    
	public CommandHandler(ModHostBridge bridge, bool isClient)
	{
		if (isClient)
			_platform = "CLIENT";
        
		Bridge = bridge;
	}
    
	public void HandleEvent(string id, string platform, string handler, string eventType, string payload)
	{
        string[] parts = payload.Split('|', 2);
        string commandName = parts[0];
        if (eventType == "COMMAND_EXECUTED")
        {
            // Example payload: "command_with_arg|arg1=123,arg2=hello"
            string commandPayload = parts.Length > 1 ? parts[1] : string.Empty;

            Dictionary<string, string> argsDict = commandPayload.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(pair => pair.Split('=', 2))
                .ToDictionary(kv => kv[0], kv => kv.Length > 1 ? kv[1] : "");
            
            if (_commandCallbacks.TryGetValue(commandName, out Func<CommandContext, Task>? callback))
            {
                CommandContext context = new CommandContext(this, id, platform, commandName, commandPayload);
                _ = callback(context);
            }
        }
        else if (eventType == "COMMAND_REGISTERED")
        {
            string commandPayload = parts.Length > 1 ? parts[1] : string.Empty;
            
            if (_commandCallbacks.TryGetValue(commandName, out Func<CommandContext, Task>? callback))
            {
                CommandContext context = new CommandContext(this, id, platform, commandName, commandPayload);
                _ = callback(context);
            }
        }
        else if (eventType == "COMMAND_REQUIREMENT")
        {
            // Example payload: someCommand
            string sourcePayload = parts.Length > 1 ? parts[1] : "";

            string final = sourcePayload == "" ? commandName : $"{commandName}:{sourcePayload}"; 

            if (_requirementCallbacks.TryGetValue(final, out Func<ServerCommandSource, Task<bool>>? callback))
            {
                ServerCommandSource ctx = new ServerCommandSource(this, id, platform, final, "COMMAND");
                
                _ = Task.Run(async () =>
                {
                    bool allowed = callback == null || await callback(ctx);
                    await Bridge.SendResponse(id, platform, handler, "COMMAND_REQUIREMENT_RESPONSE", allowed.ToString());
                });
            }
            else 
            {
                _ = Task.Run(async () =>
                {
                    await Bridge.SendResponse(id, platform, handler, "COMMAND_REQUIREMENT_RESPONSE", false.ToString());
                });
            }
        }
        else if (eventType == "SUGGESTION_REQUEST")
        {
            string requestId = id;
            string[] suggestionParts = payload.Split(':', 3);
            if (suggestionParts.Length < 2)
            {
                Console.WriteLine($"Invalid suggestion request payload: {payload}");
                return;
            }

            string providerId = suggestionParts[0];
            string suggestionRequestId = suggestionParts[1];
            string query = suggestionParts[2];

            if (_suggestionCallbacks.TryGetValue(providerId, out Func<string, ServerCommandSource, Task<IEnumerable<string>>>? callback))
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        IEnumerable<string> suggestions = await callback(query, new ServerCommandSource(this, providerId, platform, suggestionRequestId, "SUGGESTION"));
                        string result = string.Join(",", suggestions);
                        await Bridge.SendResponse(requestId, platform, handler, "SUGGESTION_RESPONSE", result);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error handling suggestion for '{providerId}': {ex.Message}");
                        await Bridge.SendResponse(requestId, platform, handler, "SUGGESTION_RESPONSE", "");
                    }
                });
            }
            else
            {
                Console.WriteLine($"No suggestion provider registered for '{providerId}'");
                _ = Bridge.SendResponse(requestId, platform, handler, "SUGGESTION_RESPONSE", "");
            }
        }
        else
        {
            Console.WriteLine($"Unhandled command event: {eventType} - {payload}");
        }
	}
    
    public async Task ExecuteMinecraftCommandAsync(string rawCommand)
    {
        string id = Guid.NewGuid().ToString();
        // Returns either COMMAND_RESULT:Success COMMAND_RESULT:Error:(error) or COMMAND_RESULT:Server not available but I dont think it matters here
        await Bridge.SendRequestAsync(id, _platform, "COMMAND", "EXECUTE_COMMAND", rawCommand);
    }

    public async Task SendCommandFeedback(string commandId, string feedback)
    {
        string id = Guid.NewGuid().ToString();
        // Returns "{id}:COMMAND_FEEDBACK:OK" but doesnt matter?
        await Bridge.SendRequestAsync(id, _platform, "COMMAND", "COMMAND_FEEDBACK", $"{commandId}:{feedback}");
    }

    public async Task FinalizeCommand(string commandId)
    {
        string id = Guid.NewGuid().ToString();
        // Returns "{id}:COMMAND_FINALIZE:OK" but doesnt matter?
        await Bridge.SendRequestAsync(id, _platform, "COMMAND", "COMMAND_FINALIZE", commandId);
    }

    public async Task FinalizeSuggestion(string commandId)
    {
        string id = Guid.NewGuid().ToString();
        await Bridge.SendRequestAsync(id, _platform, "COMMAND", "COMMAND_FINALIZE", commandId);
    }
    
    // public async Task RegisterCommandAsync(string commandName, Func<CommandContext, Task> onExecuted)
    // {
    //     await RegisterManualCommandAsync(commandName, async context =>
    //     {
    //         await onExecuted(context);
    //         if (!context.Finalized)
    //             await context.FinalizeAsync();
    //     }, requirement);
    // }
    //
    // public async Task RegisterCommandAsync(string commandName, List<CommandArgument> arguments, Func<CommandContext, Task> onExecuted)
    // {
    //     await RegisterManualCommandAsync(commandName, arguments, async context =>
    //     {
    //         await onExecuted(context);
    //         if (!context.Finalized)
    //             await context.FinalizeAsync();
    //     }, requirement);
    // }

    private async Task RegisterManualCommandAsync(string commandName, Func<CommandContext, Task> onExecuted)
    {
        string id = Guid.NewGuid().ToString();
        
        string response = await Bridge.SendRequestAsync(id, _platform, "COMMAND", "REGISTER_COMMAND", commandName);
        
        if (response == commandName)
        {
            Console.WriteLine($"Registered command {commandName} successfully from C#.");
        }
        else
        {
            throw new Exception($"Command registration failed: {response}");
        }
    }

    private async Task RegisterManualCommandAsync(string commandName, List<CommandArgument> arguments, Func<CommandContext, Task> onExecuted)
    {
        string id = Guid.NewGuid().ToString();
        
        string argsPayload = string.Join(",", arguments.Select(arg => 
            $"{arg.Name}:{arg.Type}|{(arg.IsOptional ? "optional" : "required")}"));
        
        string payload = $"{commandName}|{argsPayload}";
        
        string response = await Bridge.SendRequestAsync(id, _platform, "COMMAND", "REGISTER_COMMAND", payload);
        // 1ff44493-4788-47ac-a87f-bd0dac6fcbdf:PLATFORM:COMMAND_REGISTERED:testTest|Tester:integer|required,SecondTester:integer|optional
        if (response == payload)
        {
            Console.WriteLine($"Registered command '{commandName}' with args successfully from C#.");
        }
        else
        {
            throw new Exception($"Command with args registration failed: {response}");
        }
    }
    
    public CommandBuilder CreateCommand(string rootName)
    {
        return new CommandBuilder(rootName);
    }

    public async Task RegisterCommandAsync(CommandBuilder commandBuilder)
    {
        string commandDefinition = commandBuilder.BuildCommandDefinition();
        Console.WriteLine($"Registering command definition: {commandDefinition}");

        _requirementCallbacks[commandBuilder.Name] = commandBuilder.RequirementCallback;
        _commandCallbacks[commandBuilder.Name] = commandBuilder.ExecuteCallback;
        RegisterCallbacks(commandBuilder.SubCommands, commandBuilder.Name);
        
        await RegisterManualCommandAsync(commandDefinition, async context =>
        {
            if (commandBuilder.ExecuteCallback != null)
                await commandBuilder.ExecuteCallback(context);

            if (!context.Finalized)
                await context.FinalizeAsync();
        });
    }

    public void RegisterSuggestionProvider(string providerId, Func<string, ServerCommandSource, Task<IEnumerable<string>>> suggestionCallback)
    {
        _suggestionCallbacks[providerId] = suggestionCallback;
    }
    
    private void RegisterCallbacks(IReadOnlyList<CommandBuilder> builders, string rootCommand)
    {
        foreach (CommandBuilder builder in builders)
        {
            string fullPath = $"{rootCommand}:{builder.Name}";
            
            _requirementCallbacks[fullPath] = builder.RequirementCallback;
            _commandCallbacks[fullPath] = builder.ExecuteCallback;
            
            if(builder.SubCommands.Any())
                RegisterCallbacks(builder.SubCommands, fullPath);
        }
    }
}