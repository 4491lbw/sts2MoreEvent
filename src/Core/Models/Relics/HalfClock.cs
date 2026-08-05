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
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;
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

    private long TimeStart;

    private SceneTreeTimer _afkTimer;

    private int _timeLast = 7;

    [SavedProperty]
    public int TimeLast
    {
        get => _timeLast;
        private set
        {
            AssertMutable();
            _timeLast = value;
            DynamicVars["TimeLast"].BaseValue = value;
            InvokeDisplayAmountChanged();
        }
    }

    protected override List<DynamicVar> CanonicalVars => [
        new DynamicVar("TimeLast", _timeLast),
        new EnergyVar("Energy", 1),
    ];
    public override Task AfterAutoPrePlayPhaseEnteredLate(PlayerChoiceContext choiceContext, Player player)
    {
        TimeShutter();
        TimeRun();

        return Task.CompletedTask;
    }
    public override Task AfterCombatEnd(CombatRoom room)
    {
        TimeShutter();
        counter = 0;
        return Task.CompletedTask;
    }
    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        TimeShutter();
        TimeRun();
        return Task.CompletedTask;
    }
    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!isGet && base.Owner == cardPlay.Card.Owner)
        {
            TimeShutter();
            TimeRun();
        }

        return Task.CompletedTask;
    }
    private void AutoTimeOut()
    {
        Flash();
        _afkTimer = null;
        isGet = true;
        counter += 1;
        PlayerCmd.GainEnergy(base.DynamicVars.Energy.BaseValue, base.Owner);
        if(base.DynamicVars["TimeLast"].BaseValue < maximumTimeCount)
        {
            base.DynamicVars["TimeLast"].BaseValue += 2 * counter;
            InvokeDisplayAmountChanged();
        }
        else
        {
            base.DynamicVars["TimeLast"].BaseValue = maximumTimeCount;
            InvokeDisplayAmountChanged();
        }
    }
    private void TimeRun()
    {
        SceneTree tree = (SceneTree)Engine.GetMainLoop();
        _afkTimer = tree.CreateTimer((double)base.DynamicVars["TimeLast"].BaseValue);
        _afkTimer.Timeout += AutoTimeOut;
    }
    private void TimeShutter()
    {
        isGet = false;
        if (_afkTimer != null)
        {
            _afkTimer.Timeout -= AutoTimeOut;
            _afkTimer = null;
        }
    }
}