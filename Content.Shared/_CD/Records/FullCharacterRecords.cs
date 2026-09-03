using Content.Shared.Humanoid;
using Robust.Shared.Enums;
using Robust.Shared.Serialization;

namespace Content.Shared._CD.Records;

/// <summary>
/// Contains the full records information, not just stuff that is in the database.
/// </summary>
[Serializable, NetSerializable]
public sealed partial class FullCharacterRecords(
    PlayerProvidedCharacterRecords pRecords,
    uint? stationRecordsKey,
    string name,
    int age,
    string jobTitle,
    string jobIcon,
    string species,
    Gender gender,
    Sex sex,
    string? fingerprint,
    string? dna,
    NetEntity owner)
{
    [DataField]
    public PlayerProvidedCharacterRecords PRecords = pRecords;

    /// <summary>
    /// Key for the equivalent entry in the station records
    ///
    /// Sadly, this has to be a uint because StationRecordsKey is not serializable
    /// </summary>
    [DataField]
    public uint? StationRecordsKey = stationRecordsKey;

    /// <summary>
    ///     Name tied to this record.
    /// </summary>
    [DataField]
    public string Name = name;

    /// <summary>
    ///     Age of the person that this record represents.
    /// </summary>
    [DataField]
    public int Age = age;

    /// <summary>
    ///     Job title tied to this record.
    /// </summary>
    [DataField]
    public string JobTitle = jobTitle;

    /// <summary>
    ///     Job icon tied to this record.
    /// </summary>
    [DataField]
    public string JobIcon = jobIcon;

    /// <summary>
    ///     Species tied to this record.
    /// </summary>
    [DataField]
    public string Species = species;

    /// <summary>
    ///     Gender identity tied to this record.
    /// </summary>
    [DataField]
    public Gender Gender = gender;

    /// <summary>
    ///     Sex identity tied to this record.
    /// </summary>
    [DataField]
    public Sex Sex = sex;

    [DataField]
    public string? Fingerprint = fingerprint;

    /// <summary>
    ///     DNA of the person.
    /// </summary>
    [DataField]
    // ReSharper disable once InconsistentNaming
    public string? DNA = dna;

    /// <summary>
    /// The entity that owns this record. Should always nonnull inside CharacterRecordsComponent.
    /// </summary>
    [DataField]
    public NetEntity Owner = owner;
}
