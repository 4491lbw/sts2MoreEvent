using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoreEvent.Relics;

public sealed class BluePuppet : RelicModel
{
    public override RelicRarity Rarity => RelicRarity.Event;
    public override bool ShowCounter => true;

    private int _hpCollect = 6;
    public override int DisplayAmount =>
        base.DynamicVars["HpCollect"].IntValue;

    [SavedProperty]
    public int HpCollect
    {
        get => _hpCollect;
        private set
        {
            AssertMutable();
            _hpCollect = Math.Max(0, value);
            base.DynamicVars["HpCollect"].BaseValue = _hpCollect;
            InvokeDisplayAmountChanged();
        }
    }
    protected override List<DynamicVar> CanonicalVars => [
        new DynamicVar("HpCollect", HpCollect),
    ];
    public override decimal ModifyHpLostBeforeOstyLate(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != base.Owner.Creature || amount <= 0)
        {
            return amount;
        }
        if (HpCollect > 0)
        {
            Flash();
            HpCollect -= (int)amount;
            InvokeDisplayAmountChanged();
            return 0;
        }
        return amount;
    }
    private void StoreHealing(decimal amount)
    {
        if (amount <= 0m)
        {
            return;
        }
        // 舍弃小数部分
        decimal amountToStore = (int)Math.Floor(amount);
        if (amountToStore <= 0)
        {
            return;
        }
        HpCollect += (int)amountToStore;

        Flash();
    }
    [HarmonyPatch(
        typeof(CreatureCmd),
        nameof(CreatureCmd.Heal),
        new[]
        {
            typeof(Creature),
            typeof(decimal),
            typeof(bool)
        })]
    private static class CreatureCmdHealPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(
            Creature creature,
            decimal amount,
            ref Task __result)
        {
            BluePuppet? relic = creature.Player?.GetRelic<BluePuppet>();

            // 目标不是持有遗物的玩家，正常执行
            if (relic == null || amount <= 0m)
            {
                return true;
            }

            relic.StoreHealing(amount);

            // 为被跳过的异步方法提供一个已完成的Task
            __result = Task.CompletedTask;

            // false表示完全跳过CreatureCmd.Heal
            return false;
        }
    }

}