using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MoreEvent.Cards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoreEvent.Events;

public class ThrillerNight : EventModel
{
    protected override List<DynamicVar> CanonicalVars => [
        new HealVar("FleeHp", 40),
        new DynamicVar("GalaCard", 1m),
    ];
    protected override List<EventOption> GenerateInitialOptions()
    {
        return [
            new EventOption(this, ActDance, InitialOptionKey("DANCE")),
            new EventOption(this, ActFlee, InitialOptionKey("FLEE"))
        ];
    }

    private async Task ActDance()
    {
        CardModel card = base.Owner.RunState.CreateCard<ThrillerStrike>(base.Owner);
        CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(card, PileType.Deck));
        await Cmd.CustomScaledWait(0.5f, 1.2f);

        SetEventFinished(L10NLookup("THRILLER_NIGHT.pages.DANCE.description"));
    }
    private async Task ActFlee()
    {
        base.DynamicVars["FleeHp"].BaseValue += base.Rng.NextInt(12);
        await CreatureCmd.Heal(base.Owner.Creature, base.DynamicVars["FleeHp"].BaseValue);
        await CardPileCmd.AddCurseToDeck<PoorSleep>(base.Owner);
        await Cmd.CustomScaledWait(0.5f, 1.2f);

        SetEventFinished(L10NLookup("THRILLER_NIGHT.pages.FLEE.description"));

    }
}
