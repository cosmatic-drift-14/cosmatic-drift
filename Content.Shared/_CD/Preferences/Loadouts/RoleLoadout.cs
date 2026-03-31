using System.Linq;
using Robust.Shared.Prototypes;

namespace Content.Shared.Preferences.Loadouts;

public sealed partial class RoleLoadout
{
    [DataField]
    public Dictionary<string, (ProtoId<LoadoutGroupPrototype>, ProtoId<LoadoutPrototype>)> LinkedGroups = new();

    public void MultiSlotValidation(ProtoId<LoadoutGroupPrototype> selectedGroup,
        ProtoId<LoadoutPrototype> selectedLoadout,
        IPrototypeManager prototypeManager)
    {
        var slotsSet = prototypeManager.Index(selectedLoadout).AdditionalRequiredSlots?.ToHashSet();
        var loadoutsToRemove = new List<(ProtoId<LoadoutGroupPrototype>, ProtoId<LoadoutPrototype>)>();

        if (slotsSet == null)
            return;

        foreach (var slot in slotsSet)
        {
            LinkedGroups[slot] = (selectedGroup, selectedLoadout);
        }

        foreach (var (groupProto, loadouts) in SelectedLoadouts)
        {
            var requiredSlots = new HashSet<string>();

            foreach (var loadout in loadouts)
            {
                var slots = prototypeManager.Index(loadout.Prototype).Equipment.Keys;

                // if this loadout occupies slots that our newly equipped loadout needs,
                foreach (var slot in slots)
                {

                    if (!slotsSet.Contains(slot))
                        continue;

                    // remove the loadout
                    loadoutsToRemove.Add((groupProto, loadout.Prototype));
                    slotsSet.Remove(slot);
                }
            }
        }

        foreach (var (group, loadout) in loadoutsToRemove)
        {
            RemoveLoadout(group, loadout, prototypeManager);
        }

        return;
    }

    public void CheckLinkedSLots(ProtoId<LoadoutGroupPrototype> selectedGroup,
        ProtoId<LoadoutPrototype> selectedLoadout,
        IPrototypeManager prototypeManager)
    {

        if (LinkedGroups.Count == 0 || LinkedGroups.ContainsValue((selectedGroup, selectedLoadout)))
            return;

        var slots = prototypeManager.Index(selectedLoadout).Equipment.Keys;

        foreach (var slot in slots)
        {
            if (!LinkedGroups.TryGetValue(slot, out var loadoutPair))
                continue;

            var (group, loadout) = loadoutPair;
            var slotsToRemove = LinkedGroups
                .Where(l => l.Value == LinkedGroups[slot])
                .ToDictionary()
                .Keys
                .ToList();

            foreach (var slotToRemove in slotsToRemove)
            {
                LinkedGroups.Remove(slotToRemove);
            }

            RemoveLoadout(group, loadout, prototypeManager);
        }
    }
}
