namespace ModHost.Models.Communication.Items;

public class ItemRegistration
{
	public string ItemDefinition { get; set; }
	public ItemType ItemType { get; set; }
	public object ItemPayload { get; set; }
	public String ItemGroup { get; set; }

	public ItemRegistration(string itemDefinition, ItemType itemType, object itemPayload, string itemGroup)
	{
		ItemDefinition = itemDefinition;
		ItemType = itemType;
		ItemPayload = itemPayload;
		ItemGroup = itemGroup;
	}
}