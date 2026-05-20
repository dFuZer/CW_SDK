using Photon.Pun;

/// <summary>
/// Minimal example of a holdable shop item built on the vanilla <see cref="ItemInstanceBehaviour"/> base.
/// Demonstrates input gating (only fires when held locally, input not locked) and a simple click action.
/// Attach this component to a prefab in CustomContent.RegisterTestItem.
/// </summary>
public class BasicUsableItem : ItemInstanceBehaviour
{
    /// <summary>
    /// Cached reference to the player holding this item instance. Resolved once in ConfigItem.
    /// </summary>
    private Player? player;

    /// <summary>
    /// Called when the item is bound to a player and synchronized PhotonView.
    /// Use this hook to cache references and register item-specific RPC handlers.
    /// </summary>
    public override void ConfigItem(ItemInstanceData data, PhotonView playerView)
    {
        player = GetComponentInParent<Player>();
    }

    /// <summary>
    /// Polls for primary click while the item is held by the local player.
    /// </summary>
    private void Update()
    {
        if (player == null || !isHeldByMe || player.HasLockedInput())
        {
            return;
        }

        if (player.input.clickWasPressed)
        {
            TriggerAction();
        }
    }

    /// <summary>
    /// Local-only action invoked on click. Replace with RPC calls for networked effects.
    /// </summary>
    private void TriggerAction()
    {
        Logger.Log("Item triggered!");
    }
}
