using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using Content.Server.Database;
using Content.Shared._CD.Records;

namespace Content.Server._CD.Records;

public static class RecordsSerialization
{
    private static int DeserializeInt(JsonElement e, string key, int def)
    {
        if (e.TryGetProperty(key, out var prop) && prop.TryGetInt32(out var v))
        {
            return v;
        }

        return def;
    }

    private static bool DeserializeBool(JsonElement e, string key, bool def)
    {
        if (e.TryGetProperty(key, out var v))
        {
            if (v.ValueKind == JsonValueKind.True)
                return true;
            if (v.ValueKind == JsonValueKind.False)
                return false;
        }

        return def;
    }

    [return: NotNullIfNotNull(nameof(def))]
    private static string? DeserializeString(JsonElement e, string key, string? def)
    {
        if (e.TryGetProperty(key, out var v))
        {
            if (v.ValueKind == JsonValueKind.String)
                return v.GetString() ?? def;
        }

        return def;
    }

    private static List<PlayerProvidedCharacterRecords.RecordEntry> DeserializeEntries(List<CDModel.CharacterRecordEntry> entries, CDModel.DbRecordEntryType ty)
    {
        return entries.Where(e => e.Type == ty)
            .Select(e => new PlayerProvidedCharacterRecords.RecordEntry(e.Title, e.Involved, e.Description))
            .ToList();
    }

    /// <summary>
    /// We need to manually deserialize CharacterRecords because the easy JSON deserializer does not
    /// do exactly what we want. More specifically, we need to more robustly handle missing and extra fields
    /// <br />
    /// <br />
    /// Missing fields are filled in with their default value, extra fields are simply ignored
    /// </summary>
    public static PlayerProvidedCharacterRecords Deserialize(JsonDocument json, List<CDModel.CharacterRecordEntry> entries)
    {
        var e = json.RootElement;
        var def = PlayerProvidedCharacterRecords.DefaultRecords();
        return new PlayerProvidedCharacterRecords(
            height: DeserializeInt(e, nameof(def.Height), def.Height),
            weight: DeserializeInt(e, nameof(def.Weight), def.Weight),
            emergencyContactName: DeserializeString(e, nameof(def.EmergencyContactName), def.EmergencyContactName),
            hasWorkAuthorization: DeserializeBool(e, nameof(def.HasWorkAuthorization), def.HasWorkAuthorization),
            identifyingFeatures: DeserializeString(e, nameof(def.IdentifyingFeatures), def.IdentifyingFeatures),
            allergies: DeserializeString(e, nameof(def.Allergies), def.Allergies), drugAllergies: DeserializeString(e, nameof(def.DrugAllergies), def.DrugAllergies),
            postmortemInstructions: DeserializeString(e, nameof(def.PostmortemInstructions), def.PostmortemInstructions),
            medicalEntries: DeserializeEntries(entries, CDModel.DbRecordEntryType.Medical),
            securityEntries: DeserializeEntries(entries, CDModel.DbRecordEntryType.Security),
            employmentEntries: DeserializeEntries(entries, CDModel.DbRecordEntryType.Employment));
    }

    private static CDModel.CharacterRecordEntry ConvertEntry(PlayerProvidedCharacterRecords.RecordEntry entry, CDModel.DbRecordEntryType type)
    {
        entry.EnsureValid();
        return new CDModel.CharacterRecordEntry()
            { Title = entry.Title, Involved = entry.Involved, Description = entry.Description, Type = type };
    }

    public static List<CDModel.CharacterRecordEntry> GetEntries(PlayerProvidedCharacterRecords records)
    {
        List<CDModel.CharacterRecordEntry> entries = new();
        foreach (var medical in records.MedicalEntries)
        {
            entries.Add(ConvertEntry(medical, CDModel.DbRecordEntryType.Medical));
        }
        foreach (var security in records.SecurityEntries)
        {
            entries.Add(ConvertEntry(security, CDModel.DbRecordEntryType.Security));
        }
        foreach (var employment in records.EmploymentEntries)
        {
            entries.Add(ConvertEntry(employment, CDModel.DbRecordEntryType.Employment));
        }
        return entries;
    }

    public static JsonDocument SerializeRecords(PlayerProvidedCharacterRecords pRecords)
    {
        // This is cursed, but we cannot use the normal JSON annotations inside of Content.Shared because it is a sandbox violation.
        //var node = JsonSerializer.SerializeToNode(pRecords)!;
        //node.AsObject().Remove(nameof(pRecords.MedicalEntries));
        //node.AsObject().Remove(nameof(pRecords.SecurityEntries));
        //node.AsObject().Remove(nameof(pRecords.EmploymentEntries));

        // Even though the method is called "Deserialize", we are serializing here. This is needed because you cannot
        // modify JSON documents.
        //return node.Deserialize<JsonDocument>()!;
        return JsonSerializer.SerializeToDocument(pRecords);
    }
}
