using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.ValueProps;
using MoreEvent.Cards;
using MoreEvent.Relics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoreEvent.Events;

public class DissolvedClock : EventModel
{
    private int times = 0;
    protected override List<DynamicVar> CanonicalVars => [
        new DamageVar("Fail", 5m, ValueProp.Unblockable | ValueProp.Unpowered),

    ];
    protected override List<EventOption> GenerateInitialOptions()
    {
        return [
            new EventOption(this, ActWake, InitialOptionKey("WAKE")),
            new EventOption(this, ActSink, InitialOptionKey("SINK")),
        ];
    }
    private async Task ActWake()
    {
        if(base.Rng.NextInt(1000) / 1000 > 1 - 1 / (2 + times))
        {
            times = 0;
            await RelicCmd.Obtain<BrokenPacketWatch>(base.Owner);
            SetEventFinished(L10NLookup("DISSOLVED_CLOCK.pages.WAKE.description"));
        }
        else
        {
            SetEventState(L10NLookup("DISSOLVED_CLOCK.pages.FAIL.description"),
            [
            new EventOption(this,
            async () => {
                await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), base.Owner.Creature, base.DynamicVars.Damage.BaseValue, ValueProp.Unblockable | ValueProp.Unpowered, null, null);
                times += 1;

                new EventOption(this, ActWake, InitialOptionKey("DISSOLVED_CLOCK.pages.FAIL.WAKE.description"));
            },"DISSOLVED_CLOCK.pages.FAIL.WAKE"),
            new EventOption(this,
            async () => {
                new EventOption(this, ActSink, InitialOptionKey("DISSOLVED_CLOCK.pages.FAIL.SINK.description"));
            },"DISSOLVED_CLOCK.pages.FAIL.SINK")
            ]);
        }
    }

    private async Task ActSink()
    {
        await RelicCmd.Obtain<HalfClock>(base.Owner);
        times = 0;
        SetEventFinished(L10NLookup("DISSOLVED_CLOCK.pages.SINK.description"));
    }
}