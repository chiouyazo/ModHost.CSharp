using ModHost.Handlers;
using ModHost.Models.Server;

namespace ModHost.Models;

public class PlayerEntity : CommandSourceContext
{
	internal PlayerEntity(CommandHandler handler, string contextId, string platform, string commandName, string context) : base(handler, contextId, platform, commandName, context) { }
	
	public static ServerPlayerEntity GetServerPlayer(CommandHandler handler, string contextId, string commandName, string context)
	{
		return new ServerPlayerEntity(handler, contextId, "SERVER", commandName, context);
	}
	
	public async Task<int> Age()
	{
		return SafeInt(await SendRequest("PLAYER_AGE"));
	}
	
	public async Task<float> BodyYaw()
	{
		return SafeFloat(await SendRequest("PLAYER_BODY_YAW"));
	}
	
	public async Task<bool> CollidedSoftly()
	{
		return SafeBool(await SendRequest("PLAYER_COLLIDED_SOFTLY"));
	}
	
	public async Task<int> DeathTime()
	{
		return SafeInt(await SendRequest("PLAYER_DEATH_TIME"));
	}
	
	public async Task<int> DefaultMaxHealth()
	{
		return SafeInt(await SendRequest("PLAYER_DEFAULT_MAX_HEALTH"));
	}
	
	public async Task<float> DistanceTraveled()
	{
		return SafeFloat(await SendRequest("PLAYER_DISTANCE_TRAVELED"));
	}
	
	public async Task<int> ExperienceLevel()
	{
		return SafeInt(await SendRequest("PLAYER_EXPERIENCE_LEVEL"));
	}
	
	public async Task<int> ExperiencePickUpDelay()
	{
		return SafeInt(await SendRequest("PLAYER_EXPERIENCE_PICK_UP_DELAY"));
	}
	
	public async Task<float> ExperienceProgress()
	{
		return SafeFloat(await SendRequest("PLAYER_EXPERIENCE_PROGRESS"));
	}
	
	// public async Task<bool> ExplodedBy()
	// {
	// 	string id = Guid.NewGuid().ToString();
	// 	return SafeBool(await _handler.Bridge.SendRequestAsync(id, _platform, "COMMAND", $"QUERY_{_context}_SOURCE", $"{ContextId}:{_commandName}:PLAYER_EXPLODED_BY"));
	// }
	
	public async Task<double> FallDistance()
	{
		return SafeDouble(await SendRequest("PLAYER_FALL_DISTANCE"));
	}
	
	public async Task<float> ForwardSpeed()
	{
		return SafeFloat(await SendRequest("PLAYER_FORWARD_SPEED"));
	}
	
	public async Task<bool> GroundCollision()
	{
		return SafeBool(await SendRequest("PLAYER_GROUND_COLLISION"));
	}
	
	public async Task<bool> PlayerHandSwinging()
	{
		return SafeBool(await SendRequest("PLAYER_HAND_SWINGING"));
	}
	
	public async Task<int> HandSwingProgress()
	{
		return SafeInt(await SendRequest("PLAYER_HAND_SWING_PROGRESS"));
	}
	
	public async Task<int> HandSwingTicks()
	{
		return SafeInt(await SendRequest("PLAYER_HAND_SWING_TICKS"));
	}
	
	public async Task<float> HeadYaw()
	{
		return SafeFloat(await SendRequest("PLAYER_HEAD_YAW"));
	}
	
	public async Task<bool> HorizontalCollision()
	{
		return SafeBool(await SendRequest("PLAYER_HORIZONTAL_COLLISION"));
	}
	
	public async Task<int> HurtTime()
	{
		return SafeInt(await SendRequest("PLAYER_HURT_TIME"));
	}
	
	public async Task<bool> InPowderSnow()
	{
		return SafeBool(await SendRequest("PLAYER_IN_POWDER_SNOW"));
	}
	
	public async Task<bool> NoClip()
	{
		return SafeBool(await SendRequest("PLAYER_NO_CLIP"));
	}
	
	public async Task<float> SidewaysSpeed()
	{
		return SafeFloat(await SendRequest("PLAYER_SIDEWAYS_SPEED"));
	}
	
	public async Task<float> Speed()
	{
		return SafeFloat(await SendRequest("PLAYER_SPEED"));
	}
	
	public async Task<float> StrideDistance()
	{
		return SafeFloat(await SendRequest("PLAYER_STRIDE_DISTANCE"));
	}
	
	public async Task<int> StuckArrowTimer()
	{
		return SafeInt(await SendRequest("PLAYER_STUCK_ARROW_TIMER"));
	}
	
	public async Task<int> StuckStingerTimer()
	{
		return SafeInt(await SendRequest("PLAYER_STUCK_STINGER_TIMER"));
	}
	
	public async Task<int> TimeUntilRegen()
	{
		return SafeInt(await SendRequest("PLAYER_TIME_UNTIL_REGEN"));
	}
	
	public async Task<int> TotalExperience()
	{
		return SafeInt(await SendRequest("PLAYER_TOTAL_EXPERIENCE"));
	}
	
	public async Task<float> UpwardSpeed()
	{
		return SafeFloat(await SendRequest("PLAYER_UPWARD_SPEED"));
	}
	
	public async Task<bool> WasInPowderSnow()
	{
		return SafeBool(await SendRequest("PLAYER_WAS_IN_POWDER_SNOW"));
	}
}