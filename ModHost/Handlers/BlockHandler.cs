namespace ModHost.Handlers;

public class BlockHandler
{
	private readonly string _platform = "SERVER";
	private const string Handler = "BLOCK";
	private readonly ModHostBridge _bridge;

	public BlockHandler(ModHostBridge bridge)
	{
		_bridge = bridge;
	}

	public async Task<bool> RegisterBlock(string itemDefinition, string itemSound, bool shouldRegisterItem, string itemGroup)
	{
		return SafeBool(await _bridge.SendRequestAsync(Guid.NewGuid().ToString(), _platform, Handler, "REGISTER_BLOCK", $"{itemDefinition}|{itemSound};{shouldRegisterItem};{itemGroup}"));
	}

	private protected bool SafeBool(string value)
	{
		if (bool.TryParse(value, out bool result))
			return result;
		return false;
	}
}