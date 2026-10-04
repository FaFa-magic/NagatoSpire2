using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.TestSupport;
using NagatoSpire2.NagatoSpire2Code.Characters;
using STS2RitsuLib;

namespace NagatoSpire2.NagatoSpire2Code.Audio;

internal static class NagatoCombatStartVoice
{
	private static IDisposable? _subscription;
	private static WeakReference<object>? _voicedCombat;

	public static void Register()
	{
		// Subscribe once; never replay a past combat-start notification at registration.
		_subscription ??= RitsuLibFramework.SubscribeLifecycle<CombatStartingEvent>(OnCombatStarting, false);
	}

	private static void OnCombatStarting(CombatStartingEvent evt)
	{
		if (TestMode.IsOn || evt.CombatState is not { } combat ||
			!combat.Players.Any(player => player.Character is NagatoCharacter))
			return;
		// Presentation-only, once per encounter even with multiple Nagato players.
		if (_voicedCombat is not null && _voicedCombat.TryGetTarget(out var previous) &&
			ReferenceEquals(previous, combat))
			return;
		_voicedCombat = new WeakReference<object>(combat);
		NagatoAudio.StopCharacterSelectVoice();
		SfxCmd.Play(NagatoAudio.CombatStartEvent);
	}
}
