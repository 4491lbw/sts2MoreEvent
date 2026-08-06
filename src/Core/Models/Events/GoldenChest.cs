using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MoreEvent.Relics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoreEvent.Events;

public class GoldenChest : EventModel
{
    protected override List<EventOption> GenerateInitialOptions()
    {
        return [
            new EventOption(this, ActOpen, InitialOptionKey("OPEN")),
            new EventOption(this, ActDrop, InitialOptionKey("DROP")),
        ];
    }
    private async Task ActOpen()
    {
        RelicModel relic = RelicFactory.PullNextRelicFromFront(base.Owner, RelicRarity.Common, (RelicModel r) => r.IsAllowedInShops).ToMutable();
        await RelicCmd.Obtain(relic, base.Owner);
        SetEventFinished(L10NLookup("GOLDEN_CHEAT.pages.OPEN.description"));
    }
    private async Task ActDrop()
    {
        await RewardsCmd.OfferCustom(base.Owner,
        new List<Reward>(1)
        {
            new RelicReward(
            ModelDb.Relic<BluePuppet>().ToMutable(),
            base.Owner)
        });
        SetEventFinished(L10NLookup("GOLDEN_CHEAT.pages.DROP.description"));
    }
}
