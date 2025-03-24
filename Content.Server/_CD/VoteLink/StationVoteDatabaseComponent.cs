using Content.Shared._CD.CartridgeLoader.Cartridges;
using Robust.Shared.Audio;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server._CD.VoteLink;

/// <summary>
///     Handles the global VoteLink elements.
/// </summary>
[RegisterComponent] [Access(typeof(VoteLinkSystem))] [AutoGenerateComponentPause]
public sealed partial class StationVoteDatabaseComponent : Component
{
    /// <summary>
    ///     Currently active vote, if any.
    /// </summary>
    [DataField]
    public VoteData? ActiveVote;

    /// <summary>
    ///     History of completed votes.
    /// </summary>
    [DataField]
    public List<VoteData> VoteHistory = new();

    /// <summary>
    ///     Maximum number of votes to keep in history.
    /// </summary>
    [DataField]
    public int MaxHistorySize = 10;

    /// <summary>
    ///     Cooldown duration between creating votes.
    /// </summary>
    [DataField]
    public TimeSpan CooldownDuration = TimeSpan.FromMinutes(5);

    /// <summary>
    ///     When the next vote can be created.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))] [AutoPausedField]
    public TimeSpan Cooldown = TimeSpan.Zero;

    /// <summary>
    ///     Sound to play when announcing votes.
    /// </summary>
    [DataField]
    public SoundSpecifier
        AnnouncementSound = new SoundPathSpecifier("/Audio/Effects/newplayerping.ogg"); // if it works, it works

    /// <summary>
    ///     Announcement title loc string.
    /// </summary>
    [DataField]
    public LocId Title = "vote-link-announcement-title";

    /// <summary>
    ///     Announcement message loc string.
    /// </summary>
    [DataField]
    public LocId Message = "vote-link-announcement-new";
}
