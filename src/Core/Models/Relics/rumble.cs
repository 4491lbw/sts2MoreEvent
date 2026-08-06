using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Saves.Runs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoreEvent.Relics;

public sealed class Rumble : RelicModel
{
    public override RelicRarity Rarity => RelicRarity.Event;
    public override bool ShowCounter => false;

    private bool _hasActive = false;
    [SavedProperty]
    public bool HasActive
    {
        get => _hasActive;
        private set
        {
            AssertMutable();
            _hasActive = value;
            base.Status = HasActive ? RelicStatus.Disabled : base.Status;
        }
    }
    protected override List<DynamicVar> CanonicalVars => [
        new PowerVar<PoisonPower>(5m),
        new PowerVar<WeakPower>(1m)
    ];

    public override async Task AfterAutoPrePlayPhaseEnteredLate(PlayerChoiceContext choiceContext, Player player)
    {
        base.Status = HasActive ? RelicStatus.Disabled : base.Status;
        if (base.Status != RelicStatus.Disabled)
        {
            Flash();
            await PowerCmd.Apply<PoisonPower>(new ThrowingPlayerChoiceContext(), base.Owner.Creature, base.DynamicVars.Poison.BaseValue, null, null);
            await PowerCmd.Apply<WeakPower>(new ThrowingPlayerChoiceContext(), base.Owner.Creature, base.DynamicVars.Weak.BaseValue, null, null);
            HasActive = true;
        }
    }
}
