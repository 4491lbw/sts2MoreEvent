using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoreEvent.Relics;

public sealed class HalfClock : RelicModel
{
    public override RelicRarity Rarity => RelicRarity.Event;
    public override bool ShowCounter => false;
    protected override List<DynamicVar> CanonicalVars => [
        new DynamicVar("TimeLast", 60m),
    ];
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {

    }

}