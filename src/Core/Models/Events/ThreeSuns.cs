using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.ValueProps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoreEvent.Events;

public class ThreeSuns : EventModel
{
	protected override List<DynamicVar> CanonicalVars => [
		new HpLossVar(12m),
	];

	private static readonly HashSet<Type> RemovableCardTypes =
[
	typeof(SporeMind),
	typeof(Enthralled),
	typeof(Clumsy),
	typeof(Normality),
	typeof(Decay),
	typeof(Regret),    
	typeof(AscendersBane),
	typeof(Writhe),
	typeof(Guilty),
	typeof(CurseOfTheBell),
	typeof(BadLuck),
	typeof(Injury),
	typeof(PoorSleep),
	typeof(Greed),
	typeof(Shame),
	typeof(Doubt),
	typeof(Folly),
	typeof(Debt),
];

	protected override List<EventOption> GenerateInitialOptions()
	{
		return [
			new EventOption(this, ActFace, InitialOptionKey("FACE")),
			new EventOption(this, ActHide, InitialOptionKey("HIDE"))
		]; 
	}

	private async Task ActFace()
	{
		await RemoveCurse();
		await CreatureCmd.LoseMaxHp(new ThrowingPlayerChoiceContext(), base.Owner.Creature, base.DynamicVars.HpLoss.BaseValue, isFromCard: false);
		SetEventFinished(L10NLookup("THREE_SUNS.pages.FACE.description"));
	}

	private async Task ActHide()
	{
		SetEventFinished(L10NLookup("THREE_SUNS.pages.HIDE.description"));
	}


	private async Task RemoveCurse()
	{
		List<CardModel> cardsToRemove = base.Owner.Deck.Cards
			.Where(card => RemovableCardTypes.Contains(card.GetType()))
			.ToList();
		if(cardsToRemove != null)
		{
			foreach (CardModel card in cardsToRemove)
			{
				await CardPileCmd.RemoveFromDeck(card);
			}
		}
	}
}
