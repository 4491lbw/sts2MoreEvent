using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoreEvent.Buffs;

public sealed class FermentationPower : PowerModel
{
	// 效果类型
	public override PowerType Type => PowerType.Buff;

	// 效果堆叠类型
	public override PowerStackType StackType => PowerStackType.Counter;     // 可叠加

	// 允许层数为负数
	public override bool AllowNegative => false;

	// 己方回合开始时触发
	public override async Task AfterPlayerTurnStartLate(PlayerChoiceContext choiceContext, Player player)
	{
		await PowerCmd.Apply<PoisonPower>(choiceContext, base.Owner, 2 * base.Amount, base.Owner, null);
	}

	public override decimal ModifyMaxEnergy(Player player, decimal amount)
	{
		if (player != base.Owner.Player)
		{
			return amount;
		}
		return amount + (decimal)base.Amount;
	}

	public override decimal ModifyHandDraw(Player player, decimal count)
	{
		if (player != base.Owner.Player)
		{
			return count;
		}
		return count + (decimal)base.Amount;
	}
}
