using Content.Shared.Roles;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._CD.JobSlotsConsole;

[Serializable, NetSerializable]
public enum JobSlotsConsoleUiKey : byte
{
    Key,
}

[Serializable, NetSerializable]
public sealed class JobSlotsConsoleState : BoundUserInterfaceState
{
    public readonly Dictionary<ProtoId<JobPrototype>, int?> Jobs;
    public readonly HashSet<ProtoId<JobPrototype>> BlacklistedJobs;
    public readonly bool Debug;

    public JobSlotsConsoleState(Dictionary<ProtoId<JobPrototype>, int?> jobs,
        HashSet<ProtoId<JobPrototype>> blacklistedJobs,
        bool debug)
    {
        Jobs = jobs;
        BlacklistedJobs = blacklistedJobs;
        Debug = debug;
    }
}

[Serializable, NetSerializable]
public sealed class JobSlotsConsoleAdjustMessage : BoundUserInterfaceMessage
{
    public readonly string JobId;
    public readonly int Adjustment;
    public readonly bool? SetInfinite;

    public JobSlotsConsoleAdjustMessage(string jobId, int adjustment, bool? setInfinite = null)
    {
        JobId = jobId;
        Adjustment = adjustment;
        SetInfinite = setInfinite;
    }
}
