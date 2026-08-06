using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.PotionPools;
using MegaCrit.Sts2.Core.Models.Potions;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.ValueProps;
using MoreEvent.Relics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoreEvent.Events;

public class Hodgepodge : EventModel
{
    private const string _relaxKey = "Relax";

    private const string _rudeKey = "Rude";

    private const string _kindKey = "Kind";

    private const string _memoryKey = "Memory";

    private const string _potionsKey = "Potions";
    protected override List<DynamicVar> CanonicalVars => [
        new HealVar("Relax", 10m),
        new DamageVar("Rude", 7m, ValueProp.Unblockable | ValueProp.Unpowered),
        new DynamicVar("Kind", 10m),
        new MaxHpVar("Memory", 5m),
        new DynamicVar("Potions", 15m),
        new DynamicVar("FoulPotions", 3),
    ];
    
    protected override List<EventOption> GenerateInitialOptions()
    {
        List<EventOption> options = new List<EventOption>
        {
            new EventOption(this, ActCollect, InitialOptionKey("COLLECT")),
            new EventOption(this, ActRelax, InitialOptionKey("RELAX")),
            new EventOption(this, ActDrink, InitialOptionKey("DRINK"))
        };
        if (base.Owner.RunState.Players.Count > 1)
        {
            options.Insert(0, new EventOption(this, ActExchange, InitialOptionKey("EXCHANGE")));
        }
        return options;
    }

    private async Task ActCollect()
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
        SetEventFinished(L10NLookup("HODGEPODGE.pages.COLLECT.description"));
    }
    private async Task ActRelax()
    {
        await CreatureCmd.Heal(base.Owner.Creature, base.DynamicVars["Relax"].BaseValue);
		SetEventFinished(L10NLookup("HODGEPODGE.pages.RELAX.description"));
    }
    private async Task ActDrink()
    {
        switch (base.Rng.NextInt(9))
        {
            case 0:
                await CreatureCmd.Heal(base.Owner.Creature, base.Owner.Creature.CurrentHp / 2);
                SetEventFinished(L10NLookup("HODGEPODGE.pages.DRINK.A.description"));
                break;
            case 1:
                SetEventFinished(L10NLookup("HODGEPODGE.pages.DRINK.G.description"));
                break;
            case 2:
                await RelicCmd.Obtain<Rumble>(base.Owner);
                List<CardModel> cards = (await CardSelectCmd.FromDeckForRemoval(prefs: new CardSelectorPrefs(CardSelectorPrefs.RemoveSelectionPrompt, 1), player: base.Owner)).ToList();
                await CardPileCmd.RemoveFromDeck(cards);
                SetEventFinished(L10NLookup("HODGEPODGE.pages.DRINK.C.description"));
                break;
            case 3:
                CardModel card = base.Owner.RunState.CreateCard<Snakebite>(base.Owner);
                CardCmd.Upgrade(card, CardPreviewStyle.None);
                CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(card, PileType.Deck));
                await Cmd.CustomScaledWait(0.5f, 1.2f);
                SetEventFinished(L10NLookup("HODGEPODGE.pages.DRINK.D.description"));
                break;
            case 4:
                await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), base.Owner.Creature, (DamageVar)base.DynamicVars["Rude"], null, null);
                SetEventFinished(L10NLookup("HODGEPODGE.pages.DRINK.E.description"));
                break;
            case 5:
                await CreatureCmd.Heal(base.Owner.Creature, base.DynamicVars["Kind"].BaseValue);
                SetEventFinished(L10NLookup("HODGEPODGE.pages.DRINK.F.description"));
                break;
            case 6:
                await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), base.Owner.Creature, (DamageVar)base.DynamicVars["Potions"], null, null);

                await RewardsCmd.OfferCustom(base.Owner, new List<Reward>(3)
                {
                    new PotionReward(ModelDb.Potion<TouchOfInsanity>().ToMutable() , base.Owner),
                    new PotionReward(ModelDb.Potion<TouchOfInsanity>().ToMutable() , base.Owner),
                    new PotionReward(ModelDb.Potion<TouchOfInsanity>().ToMutable() , base.Owner)
                });
                SetEventFinished(L10NLookup("HODGEPODGE.pages.DRINK.J.description"));
                break;
            case 7:
                await CreatureCmd.GainMaxHp(base.Owner.Creature, base.DynamicVars["Memory"].BaseValue);
                SetEventFinished(L10NLookup("HODGEPODGE.pages.DRINK.H.description"));
                break;
            case 8:
                RelicModel relic = RelicFactory.PullNextRelicFromFront(base.Owner, RelicRarity.Common, (RelicModel r) => r.IsAllowedInShops).ToMutable();
                await RelicCmd.Obtain(relic, base.Owner);
                SetEventFinished(L10NLookup("HODGEPODGE.pages.DRINK.I.description"));
                break;
        }
    }

    private async Task ActExchange()
    {
        List<Player> otherPlayers = base.Owner.RunState.Players.Where(player => player != base.Owner && player.Deck.Cards.Any(IsExchangeable)).ToList();
        Player targetPlayer = base.Rng.NextItem(otherPlayers);

        CardSelectorPrefs exchangePrefs = new CardSelectorPrefs(L10NLookup("HODGEPODGE.pages.EXCHANGE.PREFS"), 1)
        {
            RequireManualConfirmation = true
        };

        CardModel ownerCard = (await CardSelectCmd.FromDeckGeneric(base.Owner, exchangePrefs, IsExchangeable)).FirstOrDefault();
        CardModel targetCard = (await CardSelectCmd.FromDeckGeneric(targetPlayer, exchangePrefs, IsExchangeable)).FirstOrDefault();
        await CardPileCmd.RemoveFromDeck(new CardModel[2]{ownerCard, targetCard}, showPreview: false);
        // 重新定义Card对应Owner
        CardModel ownerGet = base.Owner.RunState.LoadCard(targetCard.ToSerializable(), base.Owner);
        CardModel targetGet = targetPlayer.RunState.LoadCard(ownerCard.ToSerializable(), targetPlayer);
        await CardPileCmd.Add(ownerGet, PileType.Deck);
        await CardPileCmd.Add(targetGet, PileType.Deck);

    }
    private static bool IsExchangeable(CardModel card)
    {
        return card.IsRemovable;
    }
}