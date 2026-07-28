using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.PotionPools;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.ValueProps;
using MoreEvent.Cards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoreEvent.Events;

public class PacketLoss : EventModel
{
    protected override List<DynamicVar> CanonicalVars => [
        new DynamicVar("TakeGoin", 42m + base.Rng.NextInt(12)),
    ];
    protected override List<EventOption> GenerateInitialOptions()
    {
        return [
            new EventOption(this, ActListen, InitialOptionKey("LISTEN")),
            new EventOption(this, ActTake, InitialOptionKey("TAKE"))
        ];
    }
    private async Task ActListen()
    {
        CardModel card = base.Owner.RunState.CreateCard<CurseStorybook>(base.Owner);
        CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(card, PileType.Deck));

        SetEventFinished(L10NLookup("PACKET_LOSS.pages.LISTEN.description"));
    }
    private async Task ActTake()
    {
        IEnumerable<PotionModel> items = base.Owner.Character.PotionPool.GetUnlockedPotions(base.Owner.UnlockState).Concat(ModelDb.PotionPool<SharedPotionPool>().GetUnlockedPotions(base.Owner.UnlockState));
        PotionModel potionModel = base.Owner.PlayerRng.Rewards.NextItem(items);
        if (potionModel != null)
        {
            await RewardsCmd.OfferCustom(base.Owner, new List<Reward>(1)
                {
                    new PotionReward(potionModel.ToMutable(), base.Owner)
                });
        }
        await PlayerCmd.GainGold(base.DynamicVars["TakeGoin"].BaseValue, base.Owner, wasStolenBack: false);
        SetEventFinished(L10NLookup("PACKET_LOSS.pages.TAKE.description"));
    }
    
}