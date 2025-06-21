using ModHost.Handlers;

namespace ModHost;

public class ClientBridge
{
	private readonly ModHostBridge _bridge;
	private readonly CommandHandler _commandHandler;
	private readonly ScreenHandler _screenHandler;

	public ClientBridge(ModHostBridge bridge)
	{
		_bridge = bridge;
		_commandHandler = new CommandHandler(_bridge, true);
		_screenHandler = new ScreenHandler(_bridge);
	}

	public CommandHandler GetCommandHandler()
	{
		return _commandHandler;
	}

	public ScreenHandler GetScreenHandler()
	{
		return _screenHandler;
	}
	
	public async Task HandleEvent(string id, string platform, string handler, string eventType, string payload)
	{
		if (handler == "COMMAND")
			_commandHandler.HandleEvent(id, platform, handler, eventType, payload);
		else if (handler == "SCREEN")
			_screenHandler.HandleEvent(id, platform, handler, eventType, payload);
		else
		{
			Console.WriteLine($"Unknown handler: {handler}:{eventType}");
		}
	}
}