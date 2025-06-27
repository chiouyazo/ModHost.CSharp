using ModHost.Handlers;
using ModHost.Models.Communication;

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
	
	public async Task HandleEvent(MessageBase message)
	{
		if (message.Handler == "COMMAND")
			_commandHandler.HandleEvent(message);
		else if (message.Handler == "SCREEN")
			_screenHandler.HandleEvent(message);
		else
		{
			Console.WriteLine($"Unknown client handler: {message.Handler}:{message.Event}");
		}
	}
}