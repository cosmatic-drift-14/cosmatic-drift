using System.Diagnostics.Contracts;
using System.IO;
using Content.Client.Humanoid;
using Content.Shared.Preferences;
using Robust.Shared.GameObjects;

namespace Content.IntegrationTests.Tests._CD;

[TestFixture]
public sealed class CharacterImportTest
{
    // Disabled because it seems to be flaky -aquif Jun 26 2025
    //[Test]
    //public static async Task RoundTrip()
    //{
    //    await using var pair = await PoolManager.GetServerClient(new PoolSettings { InLobby = true });
    //    var server = pair.Server;
    //    await pair.Client.WaitIdleAsync();
    //    var entityManager = pair.Client.ResolveDependency<IEntityManager>();


    //    await server.WaitAssertion(() =>
    //    {
    //        var system = entityManager.System<HumanoidAppearanceSystem>();
    //        var profile = HumanoidCharacterProfile.Random();
    //        var stream = new MemoryStream();

    //        var dataNode = system.ToDataNode(profile);
    //        using var writer = new StreamWriter(stream);
    //        dataNode.Write(writer);
    //        writer.Flush();
    //        stream.Position = 0;

    //        var newProfile = system.FromStream(stream, pair.Client.Session!);
    //        Assert.That(newProfile.MemberwiseEquals(profile));
    //    });
    //    await pair.CleanReturnAsync();
    //}

    [TestCase(OldCDCharacterYaml, TestName = "OldCDCharacter")]
    [TestCase(CDCharacterWithMissingFieldsYaml, TestName = "CDCharacterWithMissingFields")]
    [TestCase(WizdenCharacterYaml, TestName = "WizdenCharacter")]
    public static async Task CanImportCharacter(string character)
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { InLobby = true });
        var server = pair.Server;
        await pair.Client.WaitIdleAsync();
        var entityManager = pair.Client.ResolveDependency<IEntityManager>();


        await server.WaitAssertion(() =>
        {
            var system = entityManager.System<HumanoidAppearanceSystem>();
            using var ss = StringStreamFrom(character);
            _ = system.FromStream(ss, pair.Client.Session!);
        });
        await pair.CleanReturnAsync();
    }

    [Pure]
    private static Stream StringStreamFrom(string s)
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(s);
        writer.Flush();
        stream.Position = 0;
        return stream;
    }

    private const string CDCharacterWithMissingFieldsYaml = """
        profile:
          preferenceUnavailable: SpawnAsOverflow
          spawnPriority: None
          appearance:
            markings: []
            skinColor: '#663614FF'
            eyeColor: '#808080FF'
            facialHairColor: '#001C23FF'
            facialHair: HumanFacialHairChaplin
            hairColor: '#001C23FF'
            hair: HumanHairHighfade
          gender: Male
          sex: Male
          species: Dwarf
          flavorText: ""
          name: Warner Coldsmith
          cosmaticDriftCustomSpeciesName: null
          cosmaticDriftCharacterRecords:
            employmentEntries: []
            securityEntries: []
            postmortemInstructions: Return home
            drugAllergies: None
            identifyingFeatures: ""
            hasWorkAuthorization: True
            emergencyContactName: ""
            weight: 70
            height: 170
          cosmaticDriftCharacterHeight: 0.8
          _loadouts: {}
          _traitPreferences: []
          _antagPreferences: []
          _jobPriorities:
            Passenger: High
        version: 1
        forkId: CD14
        ...
        """;

    private const string OldCDCharacterYaml = """
        profile:
          preferenceUnavailable: SpawnAsOverflow
          spawnPriority: None
          appearance:
            markings: []
            skinColor: '#663614FF'
            eyeColor: '#808080FF'
            facialHairColor: '#001C23FF'
            facialHair: HumanFacialHairChaplin
            hairColor: '#001C23FF'
            hair: HumanHairHighfade
          gender: Male
          sex: Male
          age: 43
          species: Dwarf
          flavorText: ""
          name: Warner Coldsmith
          cosmaticDriftCustomSpeciesName: null
          cosmaticDriftCharacterRecords:
            employmentEntries: []
            securityEntries: []
            medicalEntries: []
            postmortemInstructions: Return home
            drugAllergies: None
            allergies: None
            identifyingFeatures: ""
            hasWorkAuthorization: True
            emergencyContactName: ""
            weight: 70
            height: 170
          cosmaticDriftCharacterHeight: 0.8
          _loadouts: {}
          _traitPreferences: []
          _antagPreferences: []
          _jobPriorities:
            Passenger: High
        version: 1
        forkId: CD14
        ...
        """;

    private const string WizdenCharacterYaml = """
        profile:
          preferenceUnavailable: SpawnAsOverflow
          spawnPriority: None
          appearance:
            markings: []
            skinColor: '#B3F2E9FF'
            eyeColor: '#808080FF'
            facialHairColor: '#F5ED94FF'
            facialHair: FacialHairShaved
            hairColor: '#F5ED94FF'
            hair: HairBald
          gender: Epicene
          sex: Unsexed
          age: 52
          species: Arachnid
          flavorText: ""
          name: Phoneutria Thoracica
          _loadouts: {}
          _traitPreferences: []
          _antagPreferences: []
          _jobPriorities:
            Passenger: High
        version: 1
        forkId: wizards
        ...
        """;
}
