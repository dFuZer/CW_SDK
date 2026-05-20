using UnityEngine;

namespace ExampleMod;

/// <summary>
/// Custom hit behaviour for the Test Gun laser projectile.
/// Subscribes to the vanilla <see cref="Projectile.hitAction"/> and applies knockback/fall
/// when the raycast hits a player — damage is intentionally 0; only force is applied.
/// Added at runtime in <see cref="CustomContent.CreateTestGunProjectilePrefab"/>.
/// </summary>
public class TestGunProjectileLogic : MonoBehaviour
{
    /// <summary>Knockback force magnitude along the projectile's forward axis.</summary>
    public float force = 50f;

    /// <summary>Fall/stagger duration</summary>
    public float fall = 2f;

    private Projectile _projectile = null!;

    private void Awake()
    {
        _projectile = GetComponent<Projectile>();
    }

    private void OnEnable()
    {
        _projectile.hitAction += OnHit;
    }

    private void OnDisable()
    {
        _projectile.hitAction -= OnHit;
    }

    /// <summary>
    /// Applies physics impulse to players struck by the laser.
    /// Uses GetComponentInParent because raycast hits often land on child colliders/bodyparts.
    /// </summary>
    private void OnHit(RaycastHit hit)
    {
        Player? hitPlayer = hit.transform.GetComponentInParent<Player>();
        if (hitPlayer == null)
        {
            return;
        }

        hitPlayer.CallTakeDamageAndAddForceAndFallWithFallof(
            0f,
            transform.forward * force,
            fall,
            hit.point,
            1f);
    }
}
