using System.Text.Json;
using ModHost.Models.Communication.Items;

namespace ModHost.Handlers;

public class ItemHandler
{
	private readonly string _platform = "SERVER";
	private const string Handler = "ITEM";
	private readonly ModHostBridge _bridge;

	public ItemHandler(ModHostBridge bridge)
	{
		_bridge = bridge;
	}

	public async Task<bool> RegisterItem(ItemRegistration item)
	{
		return SafeBool(await _bridge.SendRequestAsync(Guid.NewGuid().ToString(), _platform, Handler, "REGISTER_ITEM", item));
	}

	private protected bool SafeBool(string value)
	{
		if (bool.TryParse(value, out bool result))
			return result;
		return false;
	}
}