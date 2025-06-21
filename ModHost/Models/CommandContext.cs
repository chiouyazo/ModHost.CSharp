namespace ModHost.Models;

public class CommandContext
{
	private readonly ModHostBridge _bridge;
	private readonly string _platform;

	public bool Finalized { get; private set; }
	public string CommandContextId { get; }
	public string CommandName { get; }
	public string Payload { get; }


	public CommandContext(ModHostBridge bridge, string commandContextId, string platform, string commandName, string payload)
	{
		_bridge = bridge;
		_platform = platform;
		CommandContextId = commandContextId;
		Payload = payload;
		CommandName = commandName;
	}
	
	public CommandSource GetSource()
	{
		return new CommandSource(_bridge, CommandContextId, _platform, CommandName);
	}
	
	public async Task SendFeedback(string message)
	{
		if (Finalized)
			throw new InvalidOperationException("Cannot send feedback after finalizing command.");

		await _bridge.SendCommandFeedback(CommandContextId, _platform, message);
	}

	public async Task ExecuteMinecraftCommandAsync(string command)
	{
		await _bridge.ExecuteMinecraftCommandAsync(command, _platform);
	}

	public async Task FinalizeAsync()
	{
		if (Finalized) 
			return;
		
		Finalized = true;
		await _bridge.FinalizeCommand(CommandContextId, _platform);
	}
}