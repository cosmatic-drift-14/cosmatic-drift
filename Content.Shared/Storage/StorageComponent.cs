using Content.Shared.Whitelist;
using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared.Storage
{
    /// <summary>
    /// Handles generic storage with window, such as backpacks.
    /// </summary>
    [RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
    public sealed partial class StorageComponent : Component
    {
        // TODO: This fucking sucks
        [ViewVariables(VVAccess.ReadWrite), DataField("isOpen"), AutoNetworkedField]
        public bool IsUiOpen;

        [ViewVariables]
        public Container Container = default!;

        // TODO: Make area insert its own component.
        [DataField("quickInsert")]
        public bool QuickInsert; // Can insert storables by "attacking" them with the storage entity

        [DataField("clickInsert")]
        public bool ClickInsert = true; // Can insert stuff by clicking the storage entity with it

        /// <summary>
        /// Minimum delay between quick/area insert actions.
        /// </summary>
        /// <remarks>Used to prevent autoclickers spamming server with individual pickup actions.</remarks>
        public TimeSpan QuickInsertCooldown = TimeSpan.FromSeconds(0.5);

        /// <summary>
        /// Minimum delay between UI open actions.
        /// <remarks>Used to spamming opening sounds.</remarks>
        /// </summary>
        [DataField]
        public TimeSpan OpenUiCooldown = TimeSpan.Zero;

        [DataField("areaInsert")]
        public bool AreaInsert;  // "Attacking" with the storage entity causes it to insert all nearby storables after a delay
        
        /// <summary>
        /// Open the storage window when pressing E.
        /// When false you can still open the inventory using verbs.
        /// </summary>
        [DataField]
        public bool OpenOnActivate = true;


        [DataField("areaInsertRadius")]
        public int AreaInsertRadius = 1;

        /// <summary>
        /// Whitelist for entities that can go into the storage.
        /// </summary>
        [DataField("whitelist")]
        public EntityWhitelist? Whitelist;

        /// <summary>
        /// Blacklist for entities that can go into storage.
        /// </summary>
        [DataField("blacklist")]
        public EntityWhitelist? Blacklist;

        /// <summary>
        /// How much storage is currently being used by contained entities.
        /// </summary>
        [ViewVariables, DataField("storageUsed"), AutoNetworkedField]
        public int StorageUsed;

        /// <summary>
        /// Maximum capacity for storage.
        /// </summary>
        [DataField("capacity"), AutoNetworkedField]
        public int StorageCapacityMax = 10000;

        /// <summary>
        /// Sound played whenever an entity is inserted into storage.
        /// </summary>
        [DataField("storageInsertSound")]
        public SoundSpecifier? StorageInsertSound = new SoundCollectionSpecifier("storageRustle");

        /// <summary>
        /// Sound played whenever an entity is removed from storage.
        /// </summary>
        [DataField("storageRemoveSound")]
        public SoundSpecifier? StorageRemoveSound;

        /// <summary>
        /// Sound played whenever the storage window is opened.
        /// </summary>
        [DataField("storageOpenSound")]
        public SoundSpecifier? StorageOpenSound = new SoundCollectionSpecifier("storageRustle");

        /// <summary>
        /// Sound played whenever the storage window is closed.
        /// </summary>
        [DataField("storageCloseSound")]
        public SoundSpecifier? StorageCloseSound;

        [Serializable, NetSerializable]
        public sealed class StorageInsertItemMessage : EntityEventArgs
        {
            public readonly NetEntity StorageUid;

            public StorageInsertItemMessage(NetEntity storageUid)
            {
                StorageUid = storageUid;
            }
        }

        /// <summary>
        /// If true, sets StackVisuals.Hide to true when the container is closed
        /// Used in cases where there are sprites that are shown when the container is open but not
        /// when it is closed
        /// </summary>
        [DataField]
        public bool HideStackVisualsWhenClosed = true;

        [Serializable, NetSerializable]
        public enum StorageUiKey
        {
            Key,
        }

        /// <summary>
        /// Allow or disallow showing the "open/close storage" verb.
        /// This is desired on items that we don't want to be accessed by the player directly.
        /// </summary>
        [DataField]
        public bool ShowVerb = true;
    }

    [Serializable, NetSerializable]
    public sealed class StorageInteractWithItemEvent : EntityEventArgs
    {
        public readonly NetEntity InteractedItemUid;

        public readonly NetEntity StorageUid;

        public StorageInteractWithItemEvent(NetEntity interactedItemUid, NetEntity storageUid)
        {
            InteractedItemUid = interactedItemUid;
            StorageUid = storageUid;
        }
    }

    /// <summary>
    /// Network event for displaying an animation of entities flying into a storage entity
    /// </summary>
    [Serializable, NetSerializable]
    public sealed class AnimateInsertingEntitiesEvent : EntityEventArgs
    {
        public readonly NetEntity Storage;
        public readonly List<NetEntity> StoredEntities;
        public readonly List<NetCoordinates> EntityPositions;
        public readonly List<Angle> EntityAngles;

        public AnimateInsertingEntitiesEvent(NetEntity storage, List<NetEntity> storedEntities, List<NetCoordinates> entityPositions, List<Angle> entityAngles)
        {
            Storage = storage;
            StoredEntities = storedEntities;
            EntityPositions = entityPositions;
            EntityAngles = entityAngles;
        }
    }

    [ByRefEvent]
    public record struct StorageInteractUsingAttemptEvent(bool Cancelled = false);

    [NetSerializable]
    [Serializable]
    public enum StorageVisuals : byte
    {
        Open,
        HasContents,
        StorageUsed,
        Capacity
    }
}
