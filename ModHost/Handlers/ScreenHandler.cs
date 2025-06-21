namespace ModHost.Handlers;

public class ScreenHandler
{
	private readonly string _platform = "CLIENT";
	private const string Handler = "SCREEN";
	private readonly ModHostBridge _bridge;

	public ScreenHandler(ModHostBridge bridge)
	{
		_bridge = bridge;
	}

	public void HandleEvent(string id, string platform, string handler, string eventType, string payload)
	{
        Console.WriteLine($"Unhandled screen event: {eventType} - {payload}");
	}

	public async Task<string> CurrentScreen()
	{
		return await _bridge.SendRequestAsync(Guid.NewGuid().ToString(), _platform, Handler, "CURRENTSCREEN");
	}

	public async Task<Guid> ShowNewScreen(string screenText)
	{
		Guid screenId = Guid.NewGuid();
		await _bridge.SendRequestAsync(screenId.ToString(), _platform, Handler, "OVERWRITESCREEN", screenText);
		return screenId;
	}

	public async Task<Guid> PushScreen(string commandId, string screenText)
	{
		Guid screenId = Guid.NewGuid();
		await _bridge.SendRequestAsync(screenId.ToString(), _platform, Handler, "REPLACESCREEN", screenText);
		return screenId;
	}

	public async Task ShowOldScreen(string screenId)
	{
		await _bridge.SendRequestAsync(Guid.NewGuid().ToString(), _platform, Handler, "REPLACEWITHEXISTING", screenId);
	}

	public async Task CloseScreen()
	{
		await _bridge.SendRequestAsync(Guid.NewGuid().ToString(), _platform, Handler, "CLOSESCREEN");
	}

	public async Task DeleteScreen(string commandId, string screenText)
	{
		await _bridge.SendRequestAsync(Guid.NewGuid().ToString(), _platform, Handler, "DELETESCREEN", screenText);
	}

	public async Task<string[]> ListScreens()
	{
		string result = await _bridge.SendRequestAsync(Guid.NewGuid().ToString(), _platform, Handler, "LISTSCREENS");
		string[] screens = result.Split("||");
		return screens;
	}
}