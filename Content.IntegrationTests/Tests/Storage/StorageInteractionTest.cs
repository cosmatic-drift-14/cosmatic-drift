// CD: Test Disabled. Ideally re-enable this someday.
using Content.Client.UserInterface.Systems.Hotbar.Widgets;
//using Content.Client.UserInterface.Systems.Storage.Controls;
using Content.IntegrationTests.Tests.Interaction;
using Content.Shared.Input;
using Content.Shared.PDA;
using Content.Shared.Storage;
using Content.Shared.Timing;
using Robust.Client.UserInterface;
using Robust.Shared.Containers;
using Robust.Shared.GameObjects;

//using Content.IntegrationTests.Tests.Interaction;
//using Content.Shared.PDA;
//using Content.Shared.Storage;
//using Robust.Shared.Containers;

//namespace Content.IntegrationTests.Tests.Storage;

//public sealed class StorageInteractionTest : InteractionTest
//{
//    /// <summary>
//    /// Check that players can interact with items in storage if the storage UI is open
//    /// </summary>
//    [Test]
//    public async Task UiInteractTest()
//    {
//        var sys = Server.System<SharedContainerSystem>();

//        await SpawnTarget("ClothingBackpack");
//        var backpack = ToServer(Target);

        // await Server.WaitPost(() => SEntMan.RemoveComponent<UseDelayComponent>(STarget!.Value));
        // await RunTicks(5);

        // // Activating the backpack opens the UI
        // await Activate();
        // Assert.That(IsUiOpen(StorageComponent.StorageUiKey.Key), Is.True);
        // Assert.That(IsUiOpen(PdaUiKey.Key), Is.False);

        // // Activating it again closes the UI
        // await Activate();
        // Assert.That(IsUiOpen(StorageComponent.StorageUiKey.Key), Is.False);

        // // Open it again
        // await Activate();
        // Assert.That(IsUiOpen(StorageComponent.StorageUiKey.Key), Is.True);

        // // Pick up a PDA
        // var pda = await PlaceInHands("PassengerPDA");
        // var sPda = ToServer(pda);
        // Assert.That(sys.IsEntityInContainer(sPda), Is.True);
        // Assert.That(sys.TryGetContainingContainer((sPda, null), out var container));
        // Assert.That(container!.Owner, Is.EqualTo(SPlayer));

//        // Pick up a PDA
//        var pda = await PlaceInHands("PassengerPDA");
//        var sPda = ToServer(pda);
//        Assert.That(sys.IsEntityInContainer(sPda), Is.True);
//        Assert.That(sys.TryGetContainingContainer((sPda, null), out var container));
//        Assert.That(container!.Owner, Is.EqualTo(SPlayer));

//        // Insert the PDA into the backpack
//        await Interact();
//        Assert.That(sys.TryGetContainingContainer((sPda, null), out container));
//        Assert.That(container!.Owner, Is.EqualTo(backpack));

//        // Use "e" / ActivateInWorld to open the PDA UI while it is still in the backpack.
//        var ctrl = GetStorageControl(pda);
//        await ClickControl(ctrl, ContentKeyFunctions.ActivateItemInWorld);
//        await RunTicks(10);
//        Assert.That(IsUiOpen(StorageComponent.StorageUiKey.Key), Is.True);
//        Assert.That(IsUiOpen(PdaUiKey.Key), Is.True);

//        // Click on the pda to pick it up and remove it from the backpack.
//        await ClickControl(ctrl, ContentKeyFunctions.MoveStoredItem);
//        await RunTicks(10);
//        Assert.That(sys.TryGetContainingContainer((sPda, null), out container));
//        Assert.That(container!.Owner, Is.EqualTo(SPlayer));

//        // UIs should still be open
//        Assert.That(IsUiOpen(StorageComponent.StorageUiKey.Key), Is.True);
//        Assert.That(IsUiOpen(PdaUiKey.Key), Is.True);
//    }
//}
