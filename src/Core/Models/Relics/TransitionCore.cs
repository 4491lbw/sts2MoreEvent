using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace MoreEvent.Relics;

public sealed class TransitionCore : RelicModel
{
    public override RelicRarity Rarity => RelicRarity.Event;
    public override bool ShowCounter => true;
    public override int DisplayAmount => EffectCount;

    private int _effectCount = 0;

    [SavedProperty]
    public int EffectCount
    {
        get => _effectCount;
        private set
        {
            AssertMutable();
            _effectCount = value;
            base.DynamicVars["EffectCount"].BaseValue = _effectCount;
            InvokeDisplayAmountChanged();
        }
    }

    private bool isEffectGoing;
    private bool shouldMove = false;


    private enum EffectKind
    {
        Damage,
        Power
    }
    private struct StructEffect   // 储存上回合的一个待生效效果
    {
        public EffectKind Kind { get; set; }
        public decimal Amount { get; set; }
        public ModelId PowerId { get; set; }
    }

    // 存储即将生效的效果，由于不能存储Struct结构，使用string与struct结构进行对应
    private string _effectsData = string.Empty;
    private string OnGoingEffectsData =string.Empty;
    [SavedProperty]
    public string EffectsData
    {
        get => _effectsData;
        private set
        {
            AssertMutable();
            _effectsData = value ?? string.Empty;
        }
    }
    //对应struct的编码解码
    protected override List<DynamicVar> CanonicalVars => [
        new DynamicVar("EffectCount", EffectCount),
    ];

    private string Encode(String saveString, StructEffect structEffect)
    {
        String newSave = string.Format(CultureInfo.InvariantCulture, "{0}|{1}|{2}\n", structEffect.Kind, structEffect.Amount, structEffect.PowerId);
        return saveString + newSave;
    }
    private static IEnumerable<StructEffect> Decode(string encoded)
    {
        if (string.IsNullOrEmpty(encoded))
        {
            yield break;
        }
        using (var reader = new StringReader(encoded))
        {
            String line;
            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;
                string[] parts = line.Split("|");
                // 提取内容
                Enum.TryParse<EffectKind>(parts[0], out var _kind);
                if(_kind == EffectKind.Power)
                {
                    ModelId powerId = ModelId.Deserialize(parts[2]);
                    yield return new StructEffect
                    {
                        Kind = _kind,
                        Amount = decimal.Parse(parts[1], CultureInfo.InvariantCulture),
                        PowerId = powerId
                    };
                }
                else if(_kind == EffectKind.Damage)
                {
                    yield return new StructEffect
                    {
                        Kind = _kind,
                        Amount = decimal.Parse(parts[1], CultureInfo.InvariantCulture)
                    };
                }
            }
        }
    }

    // 战斗开始时，清除记录
    public override Task BeforeCombatStart()
    {
        ResetEffects();
        PromoteEffects();   // 将存储的EffectsData释放，准备OnGoingEffectsData的生效

        return Task.CompletedTask;
    }
    public override Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        EffectCount = 0;
        if (side == CombatSide.Enemy)
        {
            PromoteEffects();
        }
        return Task.CompletedTask;
    }
    // 效果生效：回合开始时生效效果、回合结束时结算伤害
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player == base.Owner && OnGoingEffectsData != string.Empty)
        {
            shouldMove = true;
            isEffectGoing = true;
            try
            {
                Flash();

                foreach (var item in Decode(OnGoingEffectsData))
                {
                    IReadOnlyList<Creature> targets = Owner.Creature.CombatState.HittableEnemies;
                    Creature? target = Owner.RunState.Rng.CombatTargets.NextItem(targets);  // 队列内每次效果均随机作用对象
                    if (target == null)
                        break;
                    if (item.Kind == EffectKind.Power)
                    {
                        PowerModel? canonicalPower = ModelDb.GetByIdOrNull<PowerModel>(item.PowerId);
                        if (canonicalPower != null)
                        {
                            await PowerCmd.Apply(choiceContext, canonicalPower.ToMutable(), target, item.Amount, Owner.Creature, null);
                        }
                    }
                }
            }
            finally
            {
                isEffectGoing = false;
            }
        }
        // 最后必须清除队列
        // OnGoingEffectsData = string.Empty;
    }
    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (shouldMove && side != CombatSide.Player && OnGoingEffectsData != string.Empty)
        {
            isEffectGoing= true;
            try
            {
                Flash();

                foreach (var item in Decode(OnGoingEffectsData))
                {
                    IReadOnlyList<Creature> targets = Owner.Creature.CombatState.HittableEnemies;
                    Creature? target = Owner.RunState.Rng.CombatTargets.NextItem(targets);  // 队列内每次效果均随机作用对象
                    if (target == null)
                        break;
                    if (item.Kind == EffectKind.Damage)
                    {
                        await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), target, item.Amount, ValueProp.Unpowered, Owner.Creature, null);
                    }
                    shouldMove = false;
                }
            }
            finally
            {
                isEffectGoing= false;
                OnGoingEffectsData = string.Empty;
            }
        }
        // 最后必须清除队列
    }
    // 伤害延迟记录 + 当前伤害取消
    public override decimal ModifyHpLostBeforeOstyLate(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (isEffectGoing || amount <= 0m || target.Side != CombatSide.Enemy || target.CombatState?.CurrentSide != CombatSide.Player)
        {
            return amount;
        }
        EffectCount += 1;
        EffectsData = Encode(
            EffectsData,
            new StructEffect
            {
                Kind = EffectKind.Damage,
                Amount = amount
            });

        return 0m;
    }

    // buff记录 + buff清除
    public override bool TryModifyPowerAmountReceived(PowerModel canonicalPower, Creature target, decimal amount, Creature? applier, out decimal modifiedAmount)
    {
        if (isEffectGoing ||  amount == 0m || target.Side != CombatSide.Enemy || target.CombatState.CurrentSide != CombatSide.Player)
        {
            modifiedAmount = amount;
            return false;
        }
        EffectCount += 1;

        modifiedAmount = 0m;

        EffectsData = Encode(EffectsData, new StructEffect { Kind = EffectKind.Power, Amount = amount, PowerId = canonicalPower.Id });

        return true;
    }

    // 处理上回合效果状态
    private void PromoteEffects()
    {
        if (string.IsNullOrEmpty(EffectsData))
        {
            return;
        }

        OnGoingEffectsData = EffectsData;
        EffectsData = string.Empty;
    }

    // 处理完当前效果后清除记录
    private void ResetEffects()
    {
        isEffectGoing = false;
        shouldMove = false;
    }
}