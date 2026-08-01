using MegaCrit.Sts2.Core.Runs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MoreEvent.Patches;


public static class RunTimeHelper
{
    private static readonly FieldInfo PreviousRunTimeField =
        typeof(RunManager).GetField(
            "_prevRunTime",
            BindingFlags.Instance | BindingFlags.NonPublic
        )
        ?? throw new MissingFieldException(
            typeof(RunManager).FullName,
            "_prevRunTime"
        );

    public static void AdjustRunTime(long seconds)
    {
        RunManager manager = RunManager.Instance;

        long currentRunTime = manager.RunTime;
        long targetRunTime =
            Math.Max(0L, currentRunTime + seconds);

        if (manager.WinTime > 0L)
        {
            manager.WinTime = targetRunTime;
            return;
        }

        long previousRunTime =
            (long)PreviousRunTimeField.GetValue(manager)!;

        PreviousRunTimeField.SetValue(
            manager,
            previousRunTime
                + targetRunTime
                - currentRunTime
        );
    }
}