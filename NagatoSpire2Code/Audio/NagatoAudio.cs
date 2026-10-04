using STS2RitsuLib.Audio;

namespace NagatoSpire2.NagatoSpire2Code.Audio;

public static class NagatoAudio
{
	private const string CharacterSelectVoiceChannel = "nagato_character_select_voice";
	private const string BankResource = "res://NagatoSpire2/sfx/Nagato.bank";
	private const string GuidResource = "res://NagatoSpire2/sfx/Nagato.guids.txt";
	private static IAudioHandle? _characterSelectVoice;
	private static bool _registered;
	private static bool _warnedPlaybackFailure;
	private static bool _warnedReleaseFailure;

	public const string AttackEvent = "event:/sfx/nagato/attack";
	public const string CastEvent = "event:/sfx/nagato/cast";
	public const string DeathEvent = "event:/sfx/nagato/death";
	public const string CharacterSelectEvent = "event:/sfx/nagato/character_select";
	public const string CombatStartEvent = "event:/sfx/nagato/combat_start";
	public const string OrbPassiveEvent = "event:/sfx/nagato/orb_passive";
	public const string OrbEvokeEvent = "event:/sfx/nagato/orb_evoke";
	public const string OrbChannelEvent = "event:/sfx/nagato/orb_channel";

	public const string CharacterTransitionEvent = "event:/sfx/nagato/character_transition";

	public static void PlayCharacterSelectVoice(float volume)
	{
		StopCharacterSelectVoice();
		// Do not lose a handle whose native release needs another cleanup attempt.
		if (_characterSelectVoice is not null)
			return;

		var playback = GameFmod.Playback.PlayOneShot(
			AudioSource.Event(CharacterSelectEvent),
			new AudioPlaybackOptions
			{
				Volume = volume,
				UseVanillaRouting = false,
				Scope = AudioLifecycleScope.Screen,
				AllowFadeOutOnStop = false,
				Routing = new AudioRoutingOptions
				{
					Channel = CharacterSelectVoiceChannel,
					ChannelMode = AudioChannelMode.ReplaceExisting,
					AllowFadeOutOnReplace = false
				}
			});
		_characterSelectVoice = playback.Handle;
		if ((!playback.Succeeded || _characterSelectVoice is null) && !_warnedPlaybackFailure)
		{
			_warnedPlaybackFailure = true;
			Godot.GD.PushWarning($"[NagatoSpire2] Character-select voice failed: {playback.Status} {playback.Message}");
		}
	}

	public static void StopCharacterSelectVoice()
	{
		if (_characterSelectVoice is null)
			return;
		_characterSelectVoice.Dispose();
		if (_characterSelectVoice.IsReleased)
		{
			_characterSelectVoice = null;
			_warnedReleaseFailure = false;
		}
		else if (!_warnedReleaseFailure)
		{
			_warnedReleaseFailure = true;
			Godot.GD.PushWarning("[NagatoSpire2] Character-select voice release failed; retaining the handle for cleanup retry.");
		}
	}

	public static void Register()
	{
		if (_registered)
			return;
		FmodStudioDeferredBankRegistration.RegisterBank(BankResource);
		FmodStudioDeferredBankRegistration.RegisterStudioGuidMappings(GuidResource);
		_registered = true;
	}
}
