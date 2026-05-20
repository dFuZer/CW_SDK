using Photon.Pun;
using UnityEngine;
using Zorro.Core.Serizalization;

namespace ExampleMod;

/// <summary>
/// Chargeable laser gun item. Drains a <see cref="BatteryEntry"/> while the player holds click,
/// then fires a projectile on all clients via a registered item RPC so shots stay in sync.
/// The projectile prefab is cloned from the vanilla Dog laser in <see cref="CustomContent"/>.
/// </summary>
public class TestGunItemBehaviour : ItemInstanceBehaviour
{
    /// <summary>Runtime-created projectile prefab assigned during item registration.</summary>
    public GameObject projectilePrefab = null!;

    /// <summary>World-space origin and forward direction for the shot (typically a child named FirePoint).</summary>
    public Transform firePoint = null!;

    /// <summary>Maximum battery charge stored on the item instance.</summary>
    public float maxCharge = 100f;

    /// <summary>Number of shots before the battery is empty (charge cost = maxCharge / maxCharges per shot).</summary>
    public int maxCharges = 30;

    /// <summary>Persisted battery state synced across the network via ItemInstanceData.</summary>
    private BatteryEntry m_batteryEntry = null!;

    /// <summary>Player holding this item; used for recoil force on the item bodypart.</summary>
    private Player playerHoldingItem = null!;

    /// <summary>Cooldown timer incremented in FixedUpdate; prevents holding click to machine-gun.</summary>
    private float sinceFire;

    /// <summary>
    /// Initializes battery data, ensures a default charge exists on first spawn,
    /// and registers RPC1 handler used to replicate laser shots.
    /// </summary>
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

    /// <summary>
    /// Local input: drain battery and send fire RPC when conditions are met.
    /// Only the holding client runs this — remote clients receive RPCA_FireLaser instead.
    /// </summary>
    private void Update()
    {
        if (m_batteryEntry.m_charge <= 0f
            || !isHeldByMe
            || !Player.localPlayer.input.clickIsPressed
            || Player.localPlayer.HasLockedInput()
            || sinceFire <= 0.6f)
        {
            return;
        }

        Fire();
    }

    /// <summary>
    /// Deducts charge, marks battery dirty for sync, and sends aim data to all clients.
    /// Serialization uses BinarySerializer so RPC payload matches vanilla item RPC patterns.
    /// </summary>
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

    /// <summary>
    /// Network handler: spawns the projectile, ignores the shooter's root collider briefly,
    /// and applies camera/item shake plus recoil on the holder's ragdoll.
    /// </summary>
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

        // Prevent the projectile from immediately colliding with the shooter's own body.
        obj.GetComponent<Projectile>().Ignore(transform.root, 1f);

        GamefeelHandler.instance.perlin.AddShake(transform.position, 2f, 0.15f, 15f, 40f);
        playerHoldingItem.refs.ragdoll.GetBodypart(BodypartType.Item).rig.AddForce(
            transform.forward * -200f, ForceMode.VelocityChange);
    }
}
