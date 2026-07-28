using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MoreEvent.Buffs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoreEvent.Cards;
public sealed class GalacticRoamingGuide : CardModel
{
    protected override List<DynamicVar> CanonicalVars => [
        new EnergyVar("Energy", 1),
        new DynamicVar("GalaCard", 1m),
    ];
    public GalacticRoamingGuide()
    : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PlayerCmd.GainEnergy(base.DynamicVars.Energy.BaseValue, base.Owner);
        await CardPileCmd.Draw(choiceContext, base.DynamicVars["GalaCard"].BaseValue, base.Owner);
    }
    protected override void OnUpgrade()
    {
        base.DynamicVars["GalaCard"].BaseValue += 1;
    }
}
