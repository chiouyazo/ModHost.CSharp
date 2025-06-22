using ModHost.Handlers;

namespace ModHost.Models;

public class CommandContext
{
	private readonly CommandHandler _handler;
	private readonly string _platform;

	public bool Finalized { get; private set; }
	public string CommandContextId { get; }
	public string CommandName { get; }
	public string RawPayload { get; }

	public Dictionary<string, string> Payload { get; set; } = new Dictionary<string, string>();


	public CommandContext(CommandHandler handler, string commandContextId, string platform, string commandName, string rawPayload)
	{
		_handler = handler;
		_platform = platform;
		CommandContextId = commandContextId;
		RawPayload = rawPayload;
		CommandName = commandName;
		
		string[] arguments = rawPayload.Split("||");
		if (!arguments.Any())
			return;
		
		foreach (string argument in arguments)
		{
			if(!argument.Contains("="))
				continue;
			string[] strings = argument.Split("=", 2);
			Payload.Add(strings[0], strings[1]);
		}
	}
	
	public CommandSource GetSource()
	{
		return new CommandSource(_handler, CommandContextId, _platform, CommandName, "COMMAND");
	}
	
	public async Task SendFeedback(string message)
	{
		if (Finalized)
			throw new InvalidOperationException("Cannot send feedback after finalizing command.");

		await _handler.SendCommandFeedback(CommandContextId, message);
	}

	public async Task ExecuteMinecraftCommandAsync(string command)
	{
		await _handler.ExecuteMinecraftCommandAsync(command);
	}

	public async Task FinalizeAsync()
	{
		if (Finalized) 
			return;
		
		Finalized = true;
		await _handler.FinalizeCommand(CommandContextId);
	}
}