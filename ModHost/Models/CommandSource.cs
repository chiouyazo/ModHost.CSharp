using ModHost.Handlers;

namespace ModHost.Models;

public class CommandSource
{
	private readonly CommandHandler _handler;
	private readonly string _platform;
	private readonly string _commandName;
	private readonly string _context;
	
	public string ContextId { get; private set; }

	// ContextId is either commandId or providerId
	public CommandSource(CommandHandler handler, string contextId, string platform, string commandName, string context)
	{
		_handler = handler;
		ContextId = contextId;
		_platform = platform;
		_commandName = commandName;
		_context = context;
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
}