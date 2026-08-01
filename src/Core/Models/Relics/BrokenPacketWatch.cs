using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoreEvent.Relics;

public sealed class BrokenPacketWatch : RelicModel
{
    public override RelicRarity Rarity => RelicRarity.Event;
    public override bool ShowCounter => false;

    private long CombatStartRunTime = -1L;
    public override Task BeforeCombatStart()
    {
        if (CombatStartRunTime < 0)
        {
            CombatStartRunTime = RunManager.Instance.RunTime;
        }

        return Task.CompletedTask;
    }
    public override async Task AfterCombatVictory(CombatRoom room)
    {
        CombatStartRunTime = -1L;


    }

}