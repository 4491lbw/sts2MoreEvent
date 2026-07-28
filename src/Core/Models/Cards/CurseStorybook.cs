using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoreEvent.Cards;

public sealed class CurseStorybook : CardModel
{
    private int block_flag = 0;

    private bool isOnHand = false;
    public override int MaxUpgradeLevel => 0;

    //public override IEnumerable<CardKeyword> CanonicalKeywords =>
    //[
    //    CardKeyword.Unplayable,
    //];
    protected override List<DynamicVar> CanonicalVars => [
        new DynamicVar("BlockNeed", 99),
    ];
    public CurseStorybook()
    : base(1, CardType.Quest, CardRarity.Quest, TargetType.None)
    {
    }

    public override Task AfterBlockGained(Creature creature, decimal amount, ValueProp props, CardModel? cardSource)
    {
        isOnHand = base.Owner.PlayerCombatState?.Hand?.Cards.Contains(this) ?? false;

        if (creature == base.Owner.Creature && isOnHand)
        {
            CreatureCmd.LoseBlock(creature, amount);
            if(base.DynamicVars["BlockNeed"].BaseValue >= amount)
            {
                base.Owner.Deck.Cards.FirstOrDefault((CardModel c) => c is CurseStorybook && c.DynamicVars["BlockNeed"].BaseValue == base.DynamicVars["BlockNeed"].BaseValue).DynamicVars["BlockNeed"].BaseValue -= amount;
                base.DynamicVars["BlockNeed"].BaseValue -= amount;            
            }
            else
            {
                base.Owner.Deck.Cards.FirstOrDefault((CardModel c) => c is CurseStorybook && c.DynamicVars["BlockNeed"].BaseValue == base.DynamicVars["BlockNeed"].BaseValue).DynamicVars["BlockNeed"].BaseValue = 0;
                base.DynamicVars["BlockNeed"].BaseValue = 0;
            }

            if (base.DynamicVars["BlockNeed"].BaseValue == 0)
            {
                CardModel cardForRemove = base.Owner.Deck.Cards.FirstOrDefault((CardModel c) => c is CurseStorybook && c.DynamicVars["BlockNeed"].BaseValue == 0);
                CardModel cardForAdd = base.CombatState.CreateCard<GalacticRoamingGuide>(base.Owner);
                CardCmd.Transform(cardForRemove, cardForAdd, CardPreviewStyle.EventLayout);

                CardModel cardinHand = base.Owner.PlayerCombatState.Hand.Cards.FirstOrDefault(c => c is CurseStorybook && c.DynamicVars["BlockNeed"].BaseValue == 0);
                CardCmd.Transform(cardinHand, cardForAdd);
            }
        }

        return Task.CompletedTask;
    }
}
