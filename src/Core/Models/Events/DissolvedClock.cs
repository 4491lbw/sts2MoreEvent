using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoreEvent.Events;

public class DissolvedClock : EventModel
{
    protected override List<EventOption> GenerateInitialOptions()
    {
        return [
            new EventOption(this, ActWake, InitialOptionKey("WAKE")),
            new EventOption(this, ActSink, InitialOptionKey("SINK")),
        ];
    }
    private async Task ActWake()
    {

    }

    private async Task ActSink()
    {

    }
}