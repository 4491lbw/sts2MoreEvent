using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MoreEvent.Cards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoreEvent.Events;

public class WhereItIs : EventModel
{
    protected override List<EventOption> GenerateInitialOptions()
    {
        return [
            new EventOption(this, ActSeek, InitialOptionKey("SEEK")),
            new EventOption(this, ActLeave, InitialOptionKey("LEAVE")),
        ];
    }
    private async Task ActSeek()
    {
        List<CardModel> cards = (await CardSelectCmd.FromDeckForRemoval(prefs: new CardSelectorPrefs(CardSelectorPrefs.RemoveSelectionPrompt, 1), player: base.Owner)).ToList();
        await CardPileCmd.RemoveFromDeck(cards);

        await CardPileCmd.AddCurseToDeck<Clumsy>(base.Owner);
        await Cmd.CustomScaledWait(0.5f, 1.2f);

        SetEventFinished(L10NLookup("WHERE_IT_IS.pages.SEEK.description"));
    }
    private async Task ActLeave()
    {

        CardModel card = base.Owner.RunState.CreateCard<ProliferatingG>(base.Owner);
        CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(card, PileType.Deck));
        await Cmd.CustomScaledWait(0.5f, 1.2f);

        SetEventFinished(L10NLookup("WHERE_IT_IS.pages.LEAVE.description"));
    }
}
