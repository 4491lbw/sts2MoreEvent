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
using MegaCrit.Sts2.Core.Rooms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoreEvent.Relics;

public sealed class HalfClock : RelicModel
{
    public override RelicRarity Rarity => RelicRarity.Event;
    public override bool ShowCounter => true;
    public override int DisplayAmount =>
        base.DynamicVars["TimeLast"].IntValue;

    private const int maximumTimeCount = 7200;

    public int counter = 0;

    public bool isGet = false;

    private SceneTreeTimer _afkTimer;
    protected override List<DynamicVar> CanonicalVars => [
        new DynamicVar("TimeLast", 10m),
        new EnergyVar("Enery", 1),
    ];
    public override Task BeforeCombatStart()
    {
        isGet = false;
        counter = 0;
        SceneTree tree = (SceneTree)Engine.GetMainLoop();
        _afkTimer = tree.CreateTimer((double)base.DynamicVars["TimeLast"].BaseValue);
        _afkTimer.Timeout += AutoTimeOut;
        InvokeDisplayAmountChanged();

        return Task.CompletedTask;
    }
    public override Task AfterCombatEnd(CombatRoom room)
    {
        if (_afkTimer != null)
        {
            _afkTimer.Timeout -= AutoTimeOut;
            _afkTimer = null;
        }

        return Task.CompletedTask;
    }
    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        isGet = false;
        return Task.CompletedTask;
    }
    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (isGet)
        {
            if (_afkTimer != null)
            {
                _afkTimer.Timeout -= AutoTimeOut;
                _afkTimer = null;
            }

            SceneTree tree = (SceneTree)Engine.GetMainLoop();
            _afkTimer = tree.CreateTimer((double)base.DynamicVars["TimeLast"].BaseValue);
            _afkTimer.Timeout += AutoTimeOut;
        }

        return Task.CompletedTask;
    }
    private void AutoTimeOut()
    {
        isGet = true;
        PlayerCmd.GainEnergy(base.DynamicVars.Energy.BaseValue, base.Owner);
        if(base.DynamicVars["TimeLast"].BaseValue < maximumTimeCount)
        {
            base.DynamicVars["TimeLast"].BaseValue += 5 * counter;
            InvokeDisplayAmountChanged();
        }
        else
        {
            base.DynamicVars["TimeLast"].BaseValue = maximumTimeCount;
            InvokeDisplayAmountChanged();
        }
    }
}