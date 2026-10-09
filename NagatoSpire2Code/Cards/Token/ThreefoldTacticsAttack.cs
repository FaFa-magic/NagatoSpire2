using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Token;

public sealed class ThreefoldTacticsAttack : NagatoChoiceOption
{
	public CardModel SourceCard { get; set; } = null!;

	public CardPlay SourcePlay { get; set; } = null!;

	protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(6m, ValueProp.Move)];

	public override async Task OnChosen(PlayerChoiceContext choiceContext)
	{
		if (SourcePlay.Target is not { IsHittable: true } target)
			return;

		await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
			.FromCard(SourceCard, SourcePlay)
			.Targeting(target)
			.WithHitFx("vfx/vfx_attack_blunt", null, "blunt_attack.mp3")
			.Execute(choiceContext);
	}

	protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2m);
}
