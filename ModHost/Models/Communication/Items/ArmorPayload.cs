namespace ModHost.Models.Communication.Items;

// TODO: Add base class for itemregistration so that we dont parse an actual object in its ctor
public class ArmorPayload
{
	public int Durability { get; set; }
	public DefenseMap Defense { get; set; }
	public int EnchantmentValue { get; set; }
	public String EquipSound { get; set; }
	public float Toughness { get; set; }
	public float KnockbackResistance { get; set; }
	public string RepairIngredient { get; set; }
	public String AssetId { get; set; }
	
	public EquipmentType ArmorType { get; set; }

	public ArmorPayload(int durability, DefenseMap defense, int enchantmentValue, string equipSound, float toughness, float knockbackresistance, string repairIngredient, string assetId)
	{
		Durability = durability;
		Defense = defense;
		EnchantmentValue = enchantmentValue;
		EquipSound = equipSound;
		Toughness = toughness;
		KnockbackResistance = knockbackresistance;
		RepairIngredient = repairIngredient;
		AssetId = assetId;
	}
}