using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Potions;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rewards;
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
    private int rate = 0;
    protected override List<DynamicVar> CanonicalVars => [
        new DamageVar("WakeDamage", 5m, ValueProp.Unblockable | ValueProp.Unpowered),

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
        if (base.Rng.NextFloat() < 0.1 + 1 - 1.0f / (1 + rate))
        {
            rate = 0;
            await RelicCmd.Obtain<BrokenPacketWatch>(base.Owner);
            SetEventFinished(L10NLookup("DISSOLVED_CLOCK.pages.WAKE.description"));
        }
        else
        {
            SetEventState(L10NLookup("DISSOLVED_CLOCK.pages.WAKE.FAIL.description"),
            new EventOption[]
            {
                new EventOption(this,
                async ()=>{
                    await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), base.Owner.Creature, (DamageVar)base.DynamicVars["WakeDamage"], null, null);
                    rate += 1;
                    await ActWake();
                    }, "DISSOLVED_CLOCK.pages.WAKE.FAIL.options.WAKE"),
                new EventOption(this, ActSink, "DISSOLVED_CLOCK.pages.WAKE.FAIL.options.SINK")
            }
            );
        }
    }

    private async Task ActSink()
    {
        await RelicCmd.Obtain<HalfClock>(base.Owner);
        rate = 0;
        SetEventFinished(L10NLookup("DISSOLVED_CLOCK.pages.SINK.description"));
    }
}