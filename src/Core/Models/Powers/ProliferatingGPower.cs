using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoreEvent.Buffs;
public sealed class ProliferatingGPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    protected override List<DynamicVar> CanonicalVars => [
    new DynamicVar("CardAmount", 0),   //累计抽卡数
    ];

    public override bool AllowNegative => false;
    public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
    {
        if(dealer.IsMonster && target == base.Owner)
        {
            base.DynamicVars["CardAmount"].BaseValue += base.Amount;
        }
    }
    public override async Task AfterBlockGained(Creature creature, decimal amount, ValueProp props, CardModel? cardSource)
    {
        if (creature.IsMonster)
        {
            base.DynamicVars["CardAmount"].BaseValue += base.Amount;
        }
    }
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        await CardPileCmd.Draw(choiceContext, base.DynamicVars["CardAmount"].BaseValue, base.Owner.Player);
        base.DynamicVars["CardAmount"].BaseValue = 0;

        CardModel card = base.CombatState.CreateCard<Infection>(base.Owner.Player);
        CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Discard, base.Owner.Player));
        await Cmd.Wait(0.5f);
    }
}
