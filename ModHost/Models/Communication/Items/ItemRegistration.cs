namespace ModHost.Models.Communication.Items;

public class ItemRegistration
{
	public string ItemDefinition { get; set; }
	public ItemType ItemType { get; set; }
	public object ItemPayload { get; set; }
	public String ItemGroup { get; set; }

	public int? MaxCount { get; set; }
	public int? MaxDamage { get; set; }

	public string? JukeboxSong { get; set; }
	public string? RepairableItem { get; set; }

	
	public bool Enchantable { get; set; } = false;
	public bool Fireproof { get; set; } = false;
	public Rarity? Rarity { get; set; } 
	public EquipmentSlot? EqiuppableSlot { get; set; }
	public EquipmentSlot? EquipableUnswappable { get; set; }
	public FoodPayload? Food { get; set; }

	public ItemRegistration(string itemDefinition, ItemType itemType, object itemPayload, string itemGroup)
	{
		ItemDefinition = itemDefinition;
		ItemType = itemType;
		ItemPayload = itemPayload;
		ItemGroup = itemGroup;
	}

	public ItemRegistration SetMaxCount(int maxCount)
	{
		MaxCount = maxCount;
		return this;
	}

	public ItemRegistration SetMaxDamage(int maxDamage)
	{
		MaxDamage = maxDamage;
		return this;
	}

	public ItemRegistration SetJukeboxSong(string jukeboxSong)
	{
		JukeboxSong = jukeboxSong;
		return this;
	}

	public ItemRegistration SetRepairItem(string item)
	{
		RepairableItem = item;
		return this;
	}

	public ItemRegistration SetRarity(Rarity rarity)
	{
		Rarity = rarity;
		return this;
	}

	public ItemRegistration SetEquippable(EquipmentSlot slot)
	{
		EqiuppableSlot = slot;
		return this;
	}

	public ItemRegistration SetEquippableUnswappable(EquipmentSlot slot)
	{
		EquipableUnswappable = slot;
		return this;
	}

	public ItemRegistration SetFood(FoodPayload food)
	{
		Food = food;
		return this;
	}
}