namespace ModHost.Models.Communication.Items;

public class FoodPayload
{
	public int Nutrition { get; set; }
	public int Saturation { get; set; }
	public bool CanAlwaysEat { get; set; }

	public FoodPayload(int nutrition, int saturation, bool canAlwaysEat)
	{
		Nutrition = nutrition;
		Saturation = saturation;
		CanAlwaysEat = canAlwaysEat;
	}
}