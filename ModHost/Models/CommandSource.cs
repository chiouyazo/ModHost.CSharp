using ModHost.Handlers;
using ModHost.Models.Server;

namespace ModHost.Models;

public abstract class CommandSource
{
	protected readonly CommandHandler _handler;
	protected readonly string _platform;
	protected readonly string _commandName;
	protected readonly string _context;
	
	public string ContextId { get; private set; }

	// ContextId is either commandId or providerId
	internal CommandSource(CommandHandler handler, string contextId, string platform, string commandName, string context)
	{
		_handler = handler;
		ContextId = contextId;
		_platform = platform;
		_commandName = commandName;
		_context = context;
	}

	public static ServerCommandSource GetServerContext(CommandHandler handler, string contextId, string commandName, string context)
	{
		return new ServerCommandSource(handler, contextId, "SERVER", commandName, context);
	}

	public async Task<bool> IsExecutedByPlayer()
	{
		string id = Guid.NewGuid().ToString();
		string response = await _handler.Bridge.SendRequestAsync(id, _platform, "COMMAND", $"QUERY_{_context}_SOURCE", $"{ContextId}:{_commandName}:IS_PLAYER");

		// Response could be: [CommandContextId]:true
		string[] parts = response.Split(':');
		return parts.Length >= 2 && parts[1].Trim().ToLower() == "true";
	}

	public async Task<string?> GetName()
	{
		string id = Guid.NewGuid().ToString();
		string response = await _handler.Bridge.SendRequestAsync(id, _platform, "COMMAND", $"QUERY_{_context}_SOURCE", $"{ContextId}:{_commandName}:NAME");
        
		string[] parts = response.Split(':', 2);
		return parts.Length >= 2 ? parts[1] : null;
	}

	public async Task<bool> HasPermissionLevel(int level)
	{
		string id = Guid.NewGuid().ToString();
		string response = await _handler.Bridge.SendRequestAsync(id, _platform, "COMMAND", $"QUERY_{_context}_SOURCE", $"{ContextId}:{_commandName}:HASPERMISSIONLEVEL:{level}");
        
		string[] parts = response.Split(':', 2);
		
		if (parts.Length >= 2)
		{
			if (!bool.TryParse(parts[1], out bool result))
				return false;
			return result;
		}

		return false;
	}

	public async Task<string> DisplayName()
	{
		string id = Guid.NewGuid().ToString();
		string response = await _handler.Bridge.SendRequestAsync(id, _platform, "COMMAND", $"QUERY_{_context}_SOURCE", $"{ContextId}:{_commandName}:DISPLAYNAME");
        
		string[] parts = response.Split(':', 2);
		
		return parts.Length >= 2 ? parts[1] : "";
	}

	public async Task<bool> IsSilent()
	{
		string id = Guid.NewGuid().ToString();
		string response = await _handler.Bridge.SendRequestAsync(id, _platform, "COMMAND", $"QUERY_{_context}_SOURCE", $"{ContextId}:{_commandName}:ISSILENT");
        
		string[] parts = response.Split(':', 2);
		
		if (parts.Length >= 2)
		{
			if (!bool.TryParse(parts[1], out bool result))
				return false;
			
			return result;
		}

		return false;
	}

	public async Task SendError(string message)
	{
		string id = Guid.NewGuid().ToString();
		await _handler.Bridge.SendRequestAsync(id, _platform, "COMMAND", $"QUERY_{_context}_SOURCE", $"{ContextId}:{_commandName}:SEND_ERROR:{message}");
	}

	public async Task SendFeedback(string message)
	{
		string id = Guid.NewGuid().ToString();
		await _handler.Bridge.SendRequestAsync(id, _platform, "COMMAND", $"QUERY_{_context}_SOURCE", $"{ContextId}:{_commandName}:SEND_FEEDBACK:{message}");
	}
}