using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Potions;
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
        new DynamicVar("TakeGold", 42m),
        new DynamicVar("ListenGold",79m)
    ];
    protected override List<EventOption> GenerateInitialOptions()
    {
        base.DynamicVars["TakeGold"].BaseValue += base.Rng.NextInt(12);
        base.DynamicVars["ListenGold"].BaseValue += base.Rng.NextInt(19);

        return [
            new EventOption(this, ActListen, InitialOptionKey("LISTEN")),
            new EventOption(this, ActTake, InitialOptionKey("TAKE"))
        ];
    }
    private async Task ActListen()
    {
        await PlayerCmd.GainGold(base.DynamicVars["TakeGold"].BaseValue, base.Owner, wasStolenBack: false);
        IEnumerable<PotionModel> items = from p in base.Owner.Character.PotionPool.GetUnlockedPotions(base.Owner.UnlockState).Concat(ModelDb.PotionPool<SharedPotionPool>().GetUnlockedPotions(base.Owner.UnlockState))
                                         where p.Rarity == PotionRarity.Rare
                                         select p
                                         ;
        PotionModel potionModel = base.Owner.PlayerRng.Rewards.NextItem(items);
        if (potionModel != null)
        {
            await RewardsCmd.OfferCustom(base.Owner, new List<Reward>(1)
                {
                    new PotionReward(potionModel.ToMutable(), base.Owner)
                });
        }

        CardModel card = base.Owner.RunState.CreateCard<CurseStorybook>(base.Owner);
        CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(card, PileType.Deck));

        SetEventFinished(L10NLookup("PACKET_LOSS.pages.LISTEN.description"));
    }
    private async Task ActTake()
    {
        await PlayerCmd.GainGold(base.DynamicVars["TakeGold"].BaseValue, base.Owner, wasStolenBack: false);

        IEnumerable<PotionModel> items = base.Owner.Character.PotionPool.GetUnlockedPotions(base.Owner.UnlockState).Concat(ModelDb.PotionPool<SharedPotionPool>().GetUnlockedPotions(base.Owner.UnlockState));
        PotionModel potionModel = base.Owner.PlayerRng.Rewards.NextItem(items);
        if (potionModel != null)
        {
            await RewardsCmd.OfferCustom(base.Owner, new List<Reward>(1)
                {
                    new PotionReward(potionModel.ToMutable(), base.Owner)
                });
        }
        SetEventFinished(L10NLookup("PACKET_LOSS.pages.TAKE.description"));
    }
    
}