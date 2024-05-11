#nullable enable
using Content.Shared.Item;
using Content.Shared.Stacks;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.IntegrationTests.Tests;

[TestFixture]
public sealed class StackTest
{
    [Test]
    public async Task StackCorrectItemSize()
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;

        var protoManager = server.ResolveDependency<IPrototypeManager>();
        var compFact = server.ResolveDependency<IComponentFactory>();

        await Assert.MultipleAsync(async () =>
        {
            foreach (var entity in pair.GetPrototypesWithComponent<StackComponent>())
            {
                StackComponent? stackComponent = null;
                ItemComponent? itemComponent = null;
                await server.WaitPost(() =>
                {
                    entity.TryGetComponent(out stackComponent, compFact);
                    entity.TryGetComponent(out itemComponent, compFact);
                });
                if (stackComponent == null || itemComponent == null)
                    continue;

                if (!protoManager.TryIndex<StackPrototype>(stackComponent.StackTypeId, out var stackProto) ||
                    stackProto.ItemSize == null)
                    continue;

                var expectedSize = stackProto.ItemSize * stackComponent.Count;
                Assert.That(itemComponent.Size, Is.EqualTo(expectedSize), $"Prototype id: {entity.ID} has an item size of {itemComponent.Size} but expected size of {expectedSize}.");
            }
        });

        await pair.CleanReturnAsync();
    }
}
