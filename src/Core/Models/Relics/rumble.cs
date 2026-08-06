using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoreEvent.Relics;

public sealed class Rumble : RelicModel
{
    public override RelicRarity Rarity => RelicRarity.Event;
    public override bool ShowCounter => false;
    protected override List<DynamicVar> CanonicalVars => [
        new PowerVar<PoisonPower>(5m),
        new PowerVar<WeakPower>(1m)
    ];

    public override async Task BeforeCombatStartLate()
    {
        Flash();
        await PowerCmd.Apply<PoisonPower>(new ThrowingPlayerChoiceContext(), base.Owner.Creature, base.DynamicVars.Poison.BaseValue, null, null);
        await PowerCmd.Apply<PoisonPower>(new ThrowingPlayerChoiceContext(), base.Owner.Creature, base.DynamicVars.Weak.BaseValue, null, null);
        base.Status = RelicStatus.Disabled;
    }
}
