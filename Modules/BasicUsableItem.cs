using Photon.Pun;

public class BasicUsableItem : ItemInstanceBehaviour
{
	private Player? player;

	public override void ConfigItem(ItemInstanceData data, PhotonView playerView)
	{
		player = GetComponentInParent<Player>();
	}
	private void Update()
	{
		if (player == null || !isHeldByMe || player.HasLockedInput()) return;
		if (player.input.clickWasPressed)
			TriggerAction();
	}

	private void TriggerAction()
	{
		Logger.Log("Item triggered!");
	}
}