using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.Timeline;
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

    private SceneTreeTimer _afkTimer;

    protected override List<DynamicVar> CanonicalVars => [
        new DynamicVar("TimeLast", 60m),
        new EnergyVar("Enery", 1),
    ];
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (_afkTimer != null)
        {
            _afkTimer.Timeout -= TimeOut;
            _afkTimer = null;
        }

        SceneTree tree = (SceneTree)Engine.GetMainLoop();
        _afkTimer = tree.CreateTimer((double)base.DynamicVars["TimeLast"].BaseValue);
        _afkTimer.Timeout += TimeOut;
    }
    private void TimeOut()
    {
        PlayerCmd.GainEnergy(base.DynamicVars.Energy.BaseValue, base.Owner);
    }
}