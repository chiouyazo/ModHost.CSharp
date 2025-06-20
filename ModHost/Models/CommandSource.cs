namespace ModHost.Models;

public class CommandSource
{
	private readonly ModHostBridge _bridge;
	private readonly string _commandId;
	private readonly string _commandName;

	public CommandSource(ModHostBridge bridge, string commandId, string commandName)
	{
		_bridge = bridge;
		_commandId = commandId;
		_commandName = commandName;
	}

	public async Task<bool> IsExecutedByPlayer()
	{
		string id = Guid.NewGuid().ToString();
		string response = await _bridge.SendRequestAsync(id, "QUERY_COMMAND_SOURCE", $"{_commandId}:{_commandName}:IS_PLAYER");

		// Response could be: [id]:COMMAND_SOURCE_RESPONSE:[CommandContextId]:true
		string[] parts = response.Split(':');
		return parts.Length >= 4 && parts[3].Trim().ToLower() == "true";
	}

	public async Task<string?> GetName()
	{
		string id = Guid.NewGuid().ToString();
		string response = await _bridge.SendRequestAsync(id, "QUERY_COMMAND_SOURCE", $"{_commandId}:{_commandName}:NAME");
        
		string[] parts = response.Split(':', 4);
		return parts.Length >= 4 ? parts[3] : null;
	}

	public async Task<bool> HasPermissionLevel(int level)
	{
		string id = Guid.NewGuid().ToString();
		string response = await _bridge.SendRequestAsync(id, "QUERY_COMMAND_SOURCE", $"{_commandId}:{_commandName}:HASPERMISSIONLEVEL:{level}");
        
		string[] parts = response.Split(':', 4);
		
		if (parts.Length >= 4)
		{
			if (!bool.TryParse(parts[3], out bool result))
				return false;
			return result;
		}

		return false;
	}
}