using System.Linq;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Preferences.Loadouts;

public sealed partial class RoleLoadout
{
    [DataField]
    public Dictionary<string, (ProtoId<LoadoutGroupPrototype>, ProtoId<LoadoutPrototype>)> LinkedSlots = new();
    [DataField]
    public List<(ProtoId<LoadoutGroupPrototype>, ProtoId<LoadoutPrototype>)> OccupiedGroups = new();

    public void MultiSlotValidation(ProtoId<LoadoutGroupPrototype> selectedGroup,
        ProtoId<LoadoutPrototype> selectedLoadout,
        IPrototypeManager prototypeManager)
    {
        var slotsSet = prototypeManager.Index(selectedLoadout).AdditionalRequiredSlots?.ToHashSet();

        if (slotsSet == null)
            return;

        foreach (var slot in slotsSet)
        {
            LinkedSlots[slot] = (selectedGroup, selectedLoadout);
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
                    OccupiedGroups.Add((groupProto, loadout.Prototype));
                    slotsSet.Remove(slot);

                }
            }
        }



        foreach (var (group, loadout) in OccupiedGroups)
        {
            RemoveLoadout(group, loadout, prototypeManager);
        }
    }

    public void EnsureMultislotsAreValidated(LoadoutGroupPrototype groupProto, IPrototypeManager prototypeManager)
    {
        foreach (var loadoutProtoId in groupProto.Loadouts)
        {
            MultiSlotValidation(groupProto.ID, loadoutProtoId, prototypeManager);
        }
    }

    public void CheckLinkedSLots(ProtoId<LoadoutGroupPrototype> selectedGroup,
        ProtoId<LoadoutPrototype> selectedLoadout,
        IPrototypeManager prototypeManager)
    {

        if (LinkedSlots.Count == 0 || LinkedSlots.ContainsValue((selectedGroup, selectedLoadout)))
            return;

        var slots = prototypeManager.Index(selectedLoadout).Equipment.Keys;

        foreach (var slot in slots)
        {
            if (!LinkedSlots.TryGetValue(slot, out var loadoutPair))
                continue;

            var (group, loadout) = loadoutPair;
            var slotsToRemove = LinkedSlots
                .Where(l => l.Value == LinkedSlots[slot])
                .ToDictionary()
                .Keys
                .ToList();

            foreach (var slotToRemove in slotsToRemove)
            {
                LinkedSlots.Remove(slotToRemove);
            }

            RemoveLoadout(group, loadout, prototypeManager);
        }
    }
}
