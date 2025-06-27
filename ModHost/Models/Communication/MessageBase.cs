namespace ModHost.Models.Communication;

public class MessageBase
{
	public string Id { get; set; } = "";
	public string Platform { get; set; } = "";
	public string Handler { get; set; } = "";
	public string Event { get; set; } = "";
	public object Payload { get; set; } = "";

	public string GetPayload()
	{
		return Payload.ToString().Replace("\"", "");
	}
}