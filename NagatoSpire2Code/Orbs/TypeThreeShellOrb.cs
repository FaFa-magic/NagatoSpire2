using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace NagatoSpire2.NagatoSpire2Code.Orbs;

[RegisterOrb]
public sealed class TypeThreeShellOrb : NagatoShellOrb
{
	public override decimal EvokeVal => 1m;
	public override Color DarkenedColor => new("#A96668");
	public override OrbAssetProfile AssetProfile => new(
		IconPath: "res://NagatoSpire2/images/orbs/type_three_shell_orb.png",
		VisualsScenePath: "res://NagatoSpire2/scenes/orbs/type_three_shell_orb.tscn");

	public override async Task<IEnumerable<Creature>> Evoke(PlayerChoiceContext choiceContext)
	{
		ActivateEvoke([Owner.Creature]);
		PlayEvokeSfx();
		await PowerCmd.Apply<PlatingPower>(choiceContext, Owner.Creature, EvokeVal, Owner.Creature, null);
		return [Owner.Creature];
	}
}
