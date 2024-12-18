using Content.Shared._CD.Telephone;
using Robust.Client.Graphics;

namespace Content.Client._CD.Telephone;

public sealed class TelephoneSystem : SharedTelephoneSystem
{
    [Dependency] private readonly IOverlayManager _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();
        if (!_overlay.HasOverlay<TelephoneOverlay>())
            _overlay.AddOverlay(new TelephoneOverlay());
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _overlay.RemoveOverlay<TelephoneOverlay>();
    }
}
