using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Potions;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
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

public class Universe : EventModel
{
    protected override List<DynamicVar> CanonicalVars => [
        new DamageVar("Gang", 5m, ValueProp.Unblockable | ValueProp.Unpowered),
        new StringVar("PoorSleep", ModelDb.Card<PoorSleep>().Title),
        new StringVar("Fermentation", ModelDb.Card<Fermentation>().Title),
        new DynamicVar("TouchOfInsanity", 1),
    ];
    protected override List<EventOption> GenerateInitialOptions()
    {
        return [
            new EventOption(this, ActGang, InitialOptionKey("GANG")),
            new EventOption(this, ActFear, InitialOptionKey("FEAR")),
            new EventOption(this, ActSleep, InitialOptionKey("SLEEP")),
        ];
    }
    private async Task ActFear()
    {
        // 宇宙冷漠了
        CardModel card = base.Owner.RunState.CreateCard<CosmicIndifference>(base.Owner);
        CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(card, PileType.Deck));
        await Cmd.CustomScaledWait(0.5f, 1.2f);
        SetEventFinished(L10NLookup("UNIVERSE.pages.FEAR.description"));
    }
    private async Task ActSleep()
    {
        // 待补充
        await RelicCmd.Obtain<TransitionCore>(base.Owner);
        await CardPileCmd.AddCurseToDeck<PoorSleep>(base.Owner);
        SetEventFinished(L10NLookup("UNIVERSE.pages.SLEEP.description"));
    }
    private async Task ActGang()
    {
        await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), base.Owner.Creature, (DamageVar)base.DynamicVars["Gang"], null, null);
        SetEventState(L10NLookup("UNIVERSE.pages.GANG.description"),
        [
            new EventOption(this,
            async () => {
                CardModel card = base.Owner.RunState.CreateCard<Fermentation>(base.Owner);
                CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(card, PileType.Deck));
                await Cmd.CustomScaledWait(0.5f, 1.2f);
                SetEventFinished(L10NLookup("UNIVERSE.pages.GANG.GET.description"));
            }, "UNIVERSE.pages.GANG.options.GET" ),
            new EventOption(this,
            async () => {
                await RewardsCmd.OfferCustom(base.Owner, new List<Reward>(1)
                {
                    new PotionReward(ModelDb.Potion<TouchOfInsanity>().ToMutable() , base.Owner)
                });

                SetEventFinished(L10NLookup("UNIVERSE.pages.GANG.LEAVE.description"));
            }, "UNIVERSE.pages.GANG.options.LEAVE" )
        ]);
    }
}