using Content.Shared.Humanoid;
using Robust.Shared.Enums;
using Robust.Shared.Serialization;

namespace Content.Shared._CD.Records;

/// <summary>
/// Contains the full records information, not just stuff that is in the database.
/// </summary>
[Serializable, NetSerializable]
public sealed class FullCharacterRecords
{
    [ViewVariables]
    public CharacterRecords CharacterRecords;

    // Yes it was easier to copy-paste this than embed GeneralStationRecords.

    /// <summary>
    ///     Name tied to this record.
    /// </summary>
    [ViewVariables]
    public string Name;

    /// <summary>
    ///     Age of the person that this record represents.
    /// </summary>
    [ViewVariables]
    public int Age;

    /// <summary>
    ///     Job title tied to this record.
    /// </summary>
    [ViewVariables]
    public string JobTitle;

    /// <summary>
    ///     Job icon tied to this record.
    /// </summary>
    [ViewVariables]
    public string JobIcon;

    /// <summary>
    ///     Species tied to this record.
    /// </summary>
    [ViewVariables]
    public string Species;

    /// <summary>
    ///     Gender identity tied to this record.
    /// </summary>
    [ViewVariables]
    public Gender Gender;

    /// <summary>
    ///     Sex identity tied to this record.
    /// </summary>
    [ViewVariables]
    public Sex Sex;

    [ViewVariables]
    public string? Fingerprint;

    /// <summary>
    ///     DNA of the person.
    /// </summary>
    [ViewVariables]
    public string? DNA;

    public FullCharacterRecords(CharacterRecords characterRecords, string name, int age, string jobTitle, string jobIcon, string species, Gender gender, Sex sex, string? fingerprint, string? dna)
    {
        CharacterRecords = characterRecords;
        Name = name;
        Age = age;
        JobTitle = jobTitle;
        JobIcon = jobIcon;
        Species = species;
        Gender = gender;
        Sex = sex;
        Fingerprint = fingerprint;
        DNA = dna;
    }
}
