using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using MoreEvent.Buffs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoreEvent.Cards;

public sealed class Fermentation : CardModel
{
	protected override List<DynamicVar> CanonicalVars => [
		new EnergyVar("Energy", 1),
	];

	public Fermentation()
		: base(1, CardType.Power, CardRarity.Event, TargetType.Self) 
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await PowerCmd.Apply<FermentationPower>(choiceContext, base.Owner.Creature, base.DynamicVars["Energy"].BaseValue, base.Owner.Creature, this);
	}

	protected override void OnUpgrade()
	{
		base.EnergyCost.UpgradeBy(-1);
	}
}
