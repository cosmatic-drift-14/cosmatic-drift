using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared.Nutrition.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ToppableComponent : Component
{
    /// <summary>
    /// List of toppings to display
    /// </summary>
    [DataField, AutoNetworkedField]
    public List<string> Toppings = new();

    /// <summary>
    /// List of toppings to display that were submerged/mixed in
    /// </summary>
    [DataField, AutoNetworkedField]
    public List<string> Submerged = new();

    /// <summary>
    /// Maximum number of toppings this entity can have
    /// </summary>
    [DataField, AutoNetworkedField]
    public int MaxToppings = 5;

    /// <summary>
    /// Sound to be played during topping insertion
    /// </summary>
    [DataField]
    public SoundSpecifier? InsertionSound;

    /// <summary>
    /// ID of solution container to add topping solution to
    /// </summary>
    [DataField]
    public string Solution = "default";
}

