using System.Linq;
using Content.Shared.Preferences;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._CD.Records;

[Serializable, NetSerializable]
public sealed class CharacterRecords
{
    private const int TextShortLen = 32;
    private const int TextMedLen = 128;
    private const int TextVeryLargeLen = 512;

    /* Basic info */

    // Additional data is fetched from the Profile

    // All
    public const int MaxHeight = 500;
    public int Height { get; private set; }
    public const int MaxWeight = 300;
    public int Weight { get; private set; }
    public string EmergencyContactName { get; private set; }
    public string EmergencyContactNumber { get; private set; }

    // Employment
    public bool HasWorkAuthorization { get; private set; }

    // Security
    public string IdentifyingFeatures { get; private set; }

    // Medical
    public string Allergies { get; private set; }
    public string DrugAllergies { get; private set; }
    public string PostmortemInstructions { get; private set; }
    // history, prescriptions, etc. would be a record below

    // "incidents"
    public List<RecordEntry> MedicalEntries { get; private set; }
    public List<RecordEntry> SecurityEntries { get; private set; }
    public List<RecordEntry> EmploymentEntries { get; private set; }

    [Serializable, NetSerializable]
    public sealed class RecordEntry
    {
        public string Title { get; private set; }
        // players involved, can be left blank (or with a generic "CentCom" etc.) for backstory related issues
        public string Involved { get; private set; }
        // Longer description of events.
        public string Description { get; private set; }

        public RecordEntry(string title, string involved, string desc)
        {
            Title = title;
            Involved = involved;
            Description = desc;
        }

        public bool MemberwiseEquals(RecordEntry other)
        {
            return Title == other.Title && Involved == other.Involved && Description == other.Description;
        }

        public void EnsureValid()
        {
            Title = ClampString(Title, TextMedLen);
            Involved = ClampString(Involved, TextMedLen);
            Description = ClampString(Description, TextVeryLargeLen);
        }
    }

    public CharacterRecords(
        bool hasWorkAuthorization,
        int height, int weight,
        string emergencyContactName,
        string emergencyContactNumber,
        string identifyingFeatures,
        string allergies, string drugAllergies,
        string postmortemInstructions,
        List<RecordEntry> medicalEntries, List<RecordEntry> securityEntries, List<RecordEntry> employmentEntries)
    {
        HasWorkAuthorization = hasWorkAuthorization;
        Height = height;
        Weight = weight;
        EmergencyContactName = emergencyContactName;
        EmergencyContactNumber = emergencyContactNumber;
        IdentifyingFeatures = identifyingFeatures;
        Allergies = allergies;
        DrugAllergies = drugAllergies;
        PostmortemInstructions = postmortemInstructions;
        MedicalEntries = medicalEntries;
        SecurityEntries = securityEntries;
        EmploymentEntries = employmentEntries;
    }

    public CharacterRecords(CharacterRecords other)
    {
        Height = other.Height;
        Weight = other.Weight;
        EmergencyContactName = other.EmergencyContactName;
        EmergencyContactNumber = other.EmergencyContactNumber;
        HasWorkAuthorization = other.HasWorkAuthorization;
        IdentifyingFeatures = other.IdentifyingFeatures;
        Allergies = other.Allergies;
        DrugAllergies = other.DrugAllergies;
        PostmortemInstructions = other.PostmortemInstructions;
        MedicalEntries = new List<RecordEntry>(other.MedicalEntries);
        SecurityEntries = new List<RecordEntry>(other.SecurityEntries);
        EmploymentEntries = new List<RecordEntry>(other.EmploymentEntries);
    }

    public static CharacterRecords DefaultRecords()
    {
        return new CharacterRecords(
            hasWorkAuthorization: true,
            height: 170, weight: 70,
            emergencyContactName: "",
            emergencyContactNumber: "",
            identifyingFeatures: "",
            allergies: "None",
            drugAllergies: "None",
            postmortemInstructions: "Return home",
            medicalEntries: new List<RecordEntry>(),
            securityEntries: new List<RecordEntry>(),
            employmentEntries: new List<RecordEntry>()
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
                   && IdentifyingFeatures == other.IdentifyingFeatures
                   && Allergies == other.Allergies
                   && DrugAllergies == other.DrugAllergies
                   && PostmortemInstructions == other.PostmortemInstructions;
        if (!test)
            return false;
        if (MedicalEntries.Count != other.MedicalEntries.Count)
            return false;
        if (SecurityEntries.Count != other.SecurityEntries.Count)
            return false;
        if (EmploymentEntries.Count != other.EmploymentEntries.Count)
            return false;
        if (MedicalEntries.Where((t, i) => !t.MemberwiseEquals(other.MedicalEntries[i])).Any())
        {
            return false;
        }
        if (SecurityEntries.Where((t, i) => !t.MemberwiseEquals(other.SecurityEntries[i])).Any())
        {
            return false;
        }
        if (EmploymentEntries.Where((t, i) => !t.MemberwiseEquals(other.EmploymentEntries[i])).Any())
        {
            return false;
        }

        return true;
    }

    private static string ClampString(string str, int maxLen)
    {
        // We call RemoveMarkup because wizden does that for flavour text
        if (str.Length > maxLen)
        {
            return FormattedMessage.RemoveMarkup(str)[..maxLen];
        }
        return FormattedMessage.RemoveMarkup(str);
    }

    private static void EnsureValidEntries(List<RecordEntry> entries)
    {
        foreach (var entry in entries)
        {
            entry.EnsureValid();
        }
    }

    public void EnsureValid()
    {
        Height = Math.Clamp(Height, 0, MaxHeight);
        Weight = Math.Clamp(Weight, 0, MaxWeight);
        EmergencyContactName =
            ClampString(EmergencyContactName, HumanoidCharacterProfile.MaxNameLength);
        EmergencyContactNumber = ClampString(EmergencyContactNumber, TextShortLen);
        IdentifyingFeatures = ClampString(IdentifyingFeatures, TextMedLen);
        Allergies = ClampString(Allergies, TextMedLen);
        DrugAllergies = ClampString(DrugAllergies, TextMedLen);
        PostmortemInstructions = ClampString(PostmortemInstructions, TextMedLen);

        EnsureValidEntries(EmploymentEntries);
        EnsureValidEntries(MedicalEntries);
        EnsureValidEntries(SecurityEntries);
    }
    public CharacterRecords WithHeight(int height)
    {
        return new(this) { Height = height };
    }
    public CharacterRecords WithWeight(int weight)
    {
        return new(this) { Weight = weight };
    }
    public CharacterRecords WithWorkAuth(bool auth)
    {
        return new(this) { HasWorkAuthorization = auth };
    }
    public CharacterRecords WithContactName(string name)
    {
        return new(this) { EmergencyContactName = name};
    }
    public CharacterRecords WithContactNumber(string number)
    {
        return new(this) { EmergencyContactNumber = number};
    }
    public CharacterRecords WithWorkAuth(string number)
    {
        return new(this) { EmergencyContactNumber = number };
    }
    public CharacterRecords WithIdentifyingFeatures(string feat)
    {
        return new(this) { IdentifyingFeatures = feat};
    }
    public CharacterRecords WithAllergies(string s)
    {
        return new(this) { Allergies = s };
    }
    public CharacterRecords WithDrugAllergies(string s)
    {
        return new(this) { DrugAllergies = s };
    }
    public CharacterRecords WithPostmortemInstructions(string s)
    {
        return new(this) { PostmortemInstructions = s};
    }
    public CharacterRecords WithEmploymentEntries(List<RecordEntry> entries)
    {
        return new(this) { EmploymentEntries = entries};
    }
    public CharacterRecords WithMedicalEntries(List<RecordEntry> entries)
    {
        return new(this) { MedicalEntries = entries};
    }
    public CharacterRecords WithSecurityEntries(List<RecordEntry> entries)
    {
        return new(this) { SecurityEntries = entries};
    }
}
