using Content.IntegrationTests.Tests.Interaction;
using Content.Shared.PDA;
using Content.Shared.Storage;
using Robust.Shared.Containers;

namespace Content.IntegrationTests.Tests.Storage;

public sealed class StorageInteractionTest : InteractionTest
{
    /// <summary>
    /// Check that players can interact with items in storage if the storage UI is open
    /// </summary>
    [Test]
    public async Task UiInteractTest()
    {
        var sys = Server.System<SharedContainerSystem>();

        await SpawnTarget("ClothingBackpack");
        var backpack = ToServer(Target);

        // Initially no BUI is open.
        Assert.That(IsUiOpen(StorageComponent.StorageUiKey.Key), Is.False);
        Assert.That(IsUiOpen(PdaUiKey.Key), Is.False);

        // Activating the backpack opens the UI
        await Activate();
        Assert.That(IsUiOpen(StorageComponent.StorageUiKey.Key), Is.True);
        Assert.That(IsUiOpen(PdaUiKey.Key), Is.False);

        // Pick up a PDA
        var pda = await PlaceInHands("PassengerPDA");
        var sPda = ToServer(pda);
        Assert.That(sys.IsEntityInContainer(sPda), Is.True);
        Assert.That(sys.TryGetContainingContainer((sPda, null), out var container));
        Assert.That(container!.Owner, Is.EqualTo(SPlayer));

        // Insert the PDA into the backpack
        await Interact();
        Assert.That(sys.TryGetContainingContainer((sPda, null), out container));
        Assert.That(container!.Owner, Is.EqualTo(backpack));

        // UIs should still be open
        Assert.That(IsUiOpen(StorageComponent.StorageUiKey.Key), Is.True);
        Assert.That(IsUiOpen(PdaUiKey.Key), Is.True);
    }
}
