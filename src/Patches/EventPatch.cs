using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MoreEvent.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoreEvent.Patches
{
	[HarmonyPatch(typeof(Overgrowth), nameof(Overgrowth.AllEvents), MethodType.Getter)]
	public static class OvergrowthAllEventsPatch
	{
		static void Postfix(ref IEnumerable<EventModel> __result)
		{
			__result = __result
				.Concat(new[] { ModelDb.Event<ThreeSuns>() })
				.Concat(new[] { ModelDb.Event<WhereItIs>() })
				.Distinct();
		}
	}

	[HarmonyPatch(typeof(Overgrowth), nameof(Overgrowth.AllEvents), MethodType.Getter)]
	public static class UnderdocksAllEventsPatch
	{
		static void Postfix(ref IEnumerable<EventModel> __result)
		{
			__result = __result
				.Concat(new[] { ModelDb.Event<DriftWithFlow>() })
				.Concat(new[] { ModelDb.Event<Hodgepodge>() })
				.Concat(new[] { ModelDb.Event<Universe>() })
				.Distinct();
		}
	}
}
