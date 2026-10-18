using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace NagatoSpire2.NagatoSpire2Code.Orbs;

[RegisterOrb(Inherit = true)]
public abstract class NagatoOrbModel : ModOrbTemplate
{
	public string VisualsPath =>
		$"res://NagatoSpire2/scenes/orbs/{StringHelper.Slugify(GetType().Name).ToLowerInvariant()}.tscn";
	public override OrbAssetProfile AssetProfile => new(
		IconPath: $"res://NagatoSpire2/images/orbs/{GetType().Name}.svg", VisualsScenePath: VisualsPath);
	public override decimal PassiveVal => 0m;
	public override decimal EvokeVal => 0m;
	public override ModOrbValueDisplayMode ValueDisplayMode => ModOrbValueDisplayMode.Hidden;
	public virtual bool IsFocusAffected => false;
	public sealed override bool AllowInRandomOrbPool => false;
	internal CardModel? HostCard { get; set; }

	protected decimal FocusValue(decimal value) =>
		IsMutable && Owner?.Creature.CombatState != null ? ModifyOrbValue(value) : value;

	protected override Node2D? TryCreateOrbSprite() =>
		PreloadManager.Cache.GetScene(VisualsPath).Instantiate<Node2D>();

	public override Task Passive(PlayerChoiceContext choiceContext, Creature? target)
	{
		ActivatePassive();
		return Task.CompletedTask;
	}

	public override Task<IEnumerable<Creature>> Evoke(PlayerChoiceContext choiceContext)
	{
		Creature[] targets = [Owner.Creature];
		ActivateEvoke(targets);
		return Task.FromResult<IEnumerable<Creature>>(targets);
	}

	protected override void AfterCloned()
	{
		base.AfterCloned();
		HostCard = null;
	}
}
