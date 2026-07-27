using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoreEvent.Events;

//随波逐流事件
public class DriftWithFlow : EventModel
{
	private const string _v50Key = "V50";

	private const string _driftGoldKey = "DriftGold";

	private const string _headKey = "Head";

	private const string _relaxKey = "Relax";
	protected override List<DynamicVar> CanonicalVars => [
		new HealVar("Relax", 7m),
		new MaxHpVar(10m),
		new DynamicVar("V50", 50m),
		new DamageVar("Head", 6m, ValueProp.Unblockable | ValueProp.Unpowered),
		new DynamicVar("DriftGold", 28m),
	];

	//public override bool IsAllowed(RunState runState) => true; // 是否允许发生该事件

	// 生成事件的选项列表
	protected override List<EventOption> GenerateInitialOptions()
	{
		return [
			new EventOption(this, ActListen, InitialOptionKey("LISTEN")),
			new EventOption(this, ActRelax, InitialOptionKey("RELAX"))
		]; // 生成两个选项
	}

	private async Task ActListen()
	{
		int flag = 1;
		if (base.Owner.Gold >= 50)
		{
			flag = 3;
		}
		else
		{
			flag = 2;
		}
		switch (base.Rng.NextInt(flag))
		{
			case 0:
				await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), base.Owner.Creature, (DamageVar)base.DynamicVars["Head"], null, null);
				await PlayerCmd.GainGold(base.DynamicVars["DriftGold"].BaseValue, base.Owner, wasStolenBack: false);
				SetEventFinished(L10NLookup("DRIFT_WITH_FLOW.pages.LISTEN.C.description"));
				break;
			case 1:
				CardModel cardModel = (await CardSelectCmd.FromDeckForUpgrade(base.Owner, new CardSelectorPrefs(CardSelectorPrefs.UpgradeSelectionPrompt, 1))).FirstOrDefault();
				if (cardModel != null)
				{
					CardCmd.Upgrade(cardModel);
				}
				SetEventFinished(L10NLookup("DRIFT_WITH_FLOW.pages.LISTEN.A.description"));
				break;
			case 2:

				await PlayerCmd.LoseGold(base.DynamicVars["V50"].BaseValue, base.Owner, GoldLossType.Spent);
				await CreatureCmd.GainMaxHp(base.Owner.Creature, base.DynamicVars.MaxHp.BaseValue);
				SetEventFinished(L10NLookup("DRIFT_WITH_FLOW.pages.LISTEN.B.description"));
				break;
		}
	}
	private async Task ActRelax()
	{
		await CreatureCmd.Heal(base.Owner.Creature, base.DynamicVars["Relax"].BaseValue);
		SetEventFinished(L10NLookup("DRIFT_WITH_FLOW.pages.RELAX.description"));
	}
}
