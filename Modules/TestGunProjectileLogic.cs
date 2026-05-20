using UnityEngine;

namespace ExampleMod;

public class TestGunProjectileLogic : MonoBehaviour
{
    public float force = 50f;
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
