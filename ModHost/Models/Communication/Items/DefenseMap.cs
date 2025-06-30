namespace ModHost.Models.Communication.Items;

public class DefenseMap 
{
	public int BootsDefense { get; set; }
	public int LeggingsDefense { get; set; }
	public int ChestplateDefense { get; set; }
	public int HelmetDefense { get; set; }
	public int BodyDefense { get; set; }

	public DefenseMap(int bootsDefense, int leggingsDefense, int chestplateDefense, int helmetDefense, int bodyDefense)
	{
		BootsDefense = bootsDefense;
		LeggingsDefense = leggingsDefense;
		ChestplateDefense = chestplateDefense;
		HelmetDefense = helmetDefense;
		BodyDefense = bodyDefense;
	}
}
