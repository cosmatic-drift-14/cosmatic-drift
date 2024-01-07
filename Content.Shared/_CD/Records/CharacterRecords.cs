using System.Linq;
using Robust.Shared.Serialization;

namespace Content.Shared._CD.Records;

[Serializable, NetSerializable]
public sealed class CharacterRecords
{

    /* Basic info */

    // Additional data is fetched from the Profile

    // All
    public int Height { get; private set; }
    public int Weight { get; private set; }
    public string EmergencyContactName { get; private set; }
    public string EmergencyContactNumber { get; private set; }

    // Employment
    public bool HasWorkAuthorization { get; private set; }

    // Security
    public ClearanceLevel SecurityClearance { get; private set; }
    public string IdentifyingFeatures { get; private set; }

    // Medical
    public string Allergies { get; private set; }
    public string DrugAllergies { get; private set; }
    public string PostmortemInstructions { get; private set; }
    // history, prescriptions, etc. would be a record below

    // "incidents"
    public List<Record> Medical { get; private set; }
    public List<Record> Security { get; private set; }
    public List<Record> Employment { get; private set; }

    [Serializable, NetSerializable]
    public enum ClearanceLevel
    {
        Standard,
        Security,
        Command,
        HighCommand,
    }

    [Serializable, NetSerializable]
    public sealed class Record
    {
        public string Title { get; private set; }
        // players involved, can be left blank (or with a generic "CentCom" etc.) for backstory related issues
        public string Involved { get; private set; }
        // Longer description of events.
        public string Description { get; private set; }

        public Record(string title, string involved, string desc)
        {
            Title = title;
            Involved = involved;
            Description = desc;
        }

        public bool MemberwiseEquals(Record other)
        {
            return Title == other.Title && Involved == other.Involved && Description == other.Description;
        }
    }

    public CharacterRecords(
        bool hasWorkAuthorization,
        int height, int weight,
        string emergencyContactName,
        string emergencyContactNumber,
        ClearanceLevel securityClearance,
        string identifyingFeatures,
        string allergies, string drugAllergies,
        string postmortemInstructions,
        List<Record> medical, List<Record> security, List<Record> employment)
    {
        HasWorkAuthorization = hasWorkAuthorization;
        Height = height;
        Weight = weight;
        EmergencyContactName = emergencyContactName;
        EmergencyContactNumber = emergencyContactNumber;
        SecurityClearance = securityClearance;
        IdentifyingFeatures = identifyingFeatures;
        Allergies = allergies;
        DrugAllergies = drugAllergies;
        PostmortemInstructions = postmortemInstructions;
        Medical = medical;
        Security = security;
        Employment = employment;
    }

    public static CharacterRecords DefaultRecords()
    {
        return new CharacterRecords(
            hasWorkAuthorization: true,
            height: 170, weight: 70,
            emergencyContactName: "",
            emergencyContactNumber: "",
            securityClearance: ClearanceLevel.Standard,
            identifyingFeatures: "",
            allergies: "None",
            drugAllergies: "None",
            postmortemInstructions: "Return home",
            medical: new List<Record>(),
            security: new List<Record>(),
            employment: new List<Record>()
        );
    }

    public bool MemberwiseEquals(CharacterRecords other)
    {
        // This is ugly but is only used for integration tests.
        var test = Height == other.Height
                   && Weight == other.Weight
                   && EmergencyContactName == other.EmergencyContactName
                   && EmergencyContactNumber == other.EmergencyContactNumber
                   && HasWorkAuthorization == other.HasWorkAuthorization
                   && SecurityClearance == other.SecurityClearance
                   && IdentifyingFeatures == other.IdentifyingFeatures
                   && Allergies == other.Allergies
                   && DrugAllergies == other.DrugAllergies
                   && PostmortemInstructions == other.PostmortemInstructions;
        if (!test)
            return false;
        if (Medical.Count != other.Medical.Count)
            return false;
        if (Security.Count != other.Security.Count)
            return false;
        if (Employment.Count != other.Employment.Count)
            return false;
        if (Medical.Where((t, i) => !t.MemberwiseEquals(other.Medical[i])).Any())
        {
            return false;
        }
        if (Security.Where((t, i) => !t.MemberwiseEquals(other.Security[i])).Any())
        {
            return false;
        }
        if (Employment.Where((t, i) => !t.MemberwiseEquals(other.Employment[i])).Any())
        {
            return false;
        }

        return true;
    }
}
