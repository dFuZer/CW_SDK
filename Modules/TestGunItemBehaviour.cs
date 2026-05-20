using Photon.Pun;
using UnityEngine;
using Zorro.Core.Serizalization;

namespace ExampleMod;

public class TestGunItemBehaviour : ItemInstanceBehaviour
{
    public GameObject projectilePrefab = null!;
    public Transform firePoint = null!;

    public float maxCharge = 100f;
    public int maxCharges = 30;

    private BatteryEntry m_batteryEntry = null!;
    private Player playerHoldingItem = null!;
    private float sinceFire;

    public override void ConfigItem(ItemInstanceData data, PhotonView playerView)
    {
        playerHoldingItem = transform.root.GetComponent<Player>();
        if (!data.TryGetEntry<BatteryEntry>(out m_batteryEntry))
        {
            m_batteryEntry = new BatteryEntry
            {
                m_charge = maxCharge,
                m_maxCharge = maxCharge
            };
            data.AddDataEntry(m_batteryEntry);
        }

        itemInstance.RegisterRPC(ItemRPC.RPC1, RPCA_FireLaser);
    }

    private void FixedUpdate()
    {
        sinceFire += Time.fixedDeltaTime;
    }

    private void Update()
    {
        if (m_batteryEntry.m_charge <= 0f || !isHeldByMe || !Player.localPlayer.input.clickIsPressed
            || Player.localPlayer.HasLockedInput() || sinceFire <= 0.6f)
        {
            return;
        }

        Fire();
    }

    private void Fire()
    {
        m_batteryEntry.m_charge -= m_batteryEntry.m_maxCharge / maxCharges;
        m_batteryEntry.SetDirty();
        sinceFire = 0f;

        var binarySerializer = new BinarySerializer();
        binarySerializer.WriteFloat3(firePoint.position);
        binarySerializer.WriteFloat3(firePoint.forward);
        itemInstance.CallRPC(ItemRPC.RPC1, binarySerializer);
    }

    public void RPCA_FireLaser(BinaryDeserializer deserializer)
    {
        if (!isHeld)
        {
            return;
        }

        Vector3 position = deserializer.ReadFloat3();
        Vector3 forward = deserializer.ReadFloat3();
        sinceFire = 0f;

        Quaternion rotation = Quaternion.LookRotation(forward);
        GameObject obj = Object.Instantiate(projectilePrefab, position, rotation);
        if (!obj.activeSelf)
        {
            obj.SetActive(true);
        }

        obj.GetComponent<Projectile>().Ignore(transform.root, 1f);

        GamefeelHandler.instance.perlin.AddShake(transform.position, 2f, 0.15f, 15f, 40f);
        playerHoldingItem.refs.ragdoll.GetBodypart(BodypartType.Item).rig.AddForce(
            transform.forward * -200f, ForceMode.VelocityChange);
    }
}
