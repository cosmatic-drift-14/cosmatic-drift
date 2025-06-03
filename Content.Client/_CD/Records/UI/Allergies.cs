using System.Linq;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Client._CD.Records.UI;

public sealed class Allergies
{
    public static Dictionary<ReagentPrototype, FixedPoint2> ResolveAllergies(
        Dictionary<string, FixedPoint2> unresolvedAllergies)
    {
        var prototypeManager = IoCManager.Resolve<IPrototypeManager>();
        var allergies = new Dictionary<ReagentPrototype, FixedPoint2>();
        foreach (var entry in unresolvedAllergies)
        {
            if (!prototypeManager.TryIndex(entry.Key, out ReagentPrototype? reagent))
                continue;
            allergies.Add(reagent, entry.Value);
        }
        return allergies;
    }

    public static string GetAllergiesText(Dictionary<ReagentPrototype, FixedPoint2> allergies)
    {
        return string.Join(", ",
            allergies
                .Keys.Select(reagent => reagent.LocalizedName[0].ToString().ToUpper() + reagent.LocalizedName[1..]));
    }
}
