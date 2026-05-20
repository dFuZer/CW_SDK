using UnityEngine;
using System.Reflection;
using DbsContentApi;

namespace ExampleMod;

/// <summary>
/// Central registry for all custom content shipped with this mod.
/// Loads embedded asset bundles, then registers a map, a monster, and shop items through DbsContentApi.
/// Bundle file names must match what you build in UNITY_CWSDK and ship beside the mod DLL.
/// </summary>
public static class CustomContent
{
    /// <summary>Asset bundle containing item/mob prefabs and UI sprites (built from UNITY_CWSDK).</summary>
    private const string BundleNameMod = "example_mod";

    /// <summary>Asset bundle containing the custom playable scene.</summary>
    private const string BundleNameMap = "example_scene";

    /// <summary>Shop tab used by example items; created once and reused in ItemConfig.category.</summary>
    public static ShopItemCategory TestCategory;

    /// <summary>Loaded mod content bundle; null if Init failed early.</summary>
    public static AssetBundle? BundleMod { get; private set; }

    /// <summary>Loaded map scene bundle; null if Init failed early.</summary>
    public static AssetBundle? BundleMap { get; private set; }

    /// <summary>
    /// Entry point called from <see cref="ExampleMod"/> on plugin load.
    /// Loads bundles from the executing assembly's directory, then registers all content types.
    /// </summary>
    public static void Init()
    {
        Logger.Log("Loading custom content...");

        // Bundles are expected next to EXAMPLE MOD.dll (see update.ps1 / workshop folder layout).
        BundleMod = ContentLoader.LoadAssetBundle(Assembly.GetExecutingAssembly(), BundleNameMod);
        if (BundleMod == null)
        {
            Logger.LogError($"Failed to load asset bundle '{BundleNameMod}'.");
            return;
        }

        BundleMap = ContentLoader.LoadAssetBundle(Assembly.GetExecutingAssembly(), BundleNameMap);
        if (BundleMap == null)
        {
            Logger.LogError($"Failed to load asset bundle '{BundleNameMap}'.");
            return;
        }

        RegisterMap();
        RegisterMob();
        RegisterItems();

        Logger.Log("Custom content successfully initialized.");
    }

    /// <summary>
    /// Registers the example map scene from the map bundle.
    /// Scene asset name inside the bundle must match your Unity scene (TestScene.unity → "TestScene").
    /// </summary>
    private static void RegisterMap()
    {
        string? scenePath = Maps.FindScenePath(BundleMap!, "TestScene");
        if (scenePath != null)
        {
            Maps.RegisterMap(
                BundleMap!,
                scenePath,
                "Example Map",
                mapId: "example_mod.example_map");
        }
        else
        {
            Logger.LogWarning("ExampleMap scene not found in bundle.");
        }
    }

    /// <summary>
    /// Registers a melee chaser bot cloned from DefaultCharacter.prefab in the mod bundle.
    /// MobSetupConfig wires up standard monster components (sync, animation, nav mesh, etc.).
    /// postSetup attaches Attack_Melee for close-range damage after the API finishes rig setup.
    /// </summary>
    private static void RegisterMob()
    {
        var config = new MobSetupConfig
        {
            visualRig = null,
            budget = new BudgetConfig { budgetCost = 1, rarity = 1f },
            controller = new ControllerConfig(),
            player = new PlayerConfig(),
            ragdoll = new RagdollConfig(),
            photonView = new PhotonViewConfig(),
            bot = new BotConfig(),
            navMesh = new NavMeshAgentConfig { height = 2f, radius = 1f, speed = 3.5f },
            addMonsterSyncer = true,
            addAnimRefHandler = true,
            addMonsterAnimationHandler = true,
            addHeadFollower = true,
            addGroundPos = true,
        };

        GameObject? mobPrefab = ContentLoader.LoadPrefabFromBundle(BundleMod!, "DefaultCharacter.prefab");
        if (mobPrefab == null)
        {
            Logger.LogWarning("DefaultCharacter.prefab not found in bundle.");
            return;
        }

        Mobs.RegisterMonster(mobPrefab, "DefaultCharacter", config,
            material: GameMaterial.M_Monster,
            postSetup: go =>
            {
                // Bot AI lives on a child object created by Mobs.RegisterMonster.
                GameObject botTestChar = Mobs.GetBotChildObject(go);
                Mobs.AddBotChaserComponent(botTestChar, new BotChaserConfig { targetDistance = 1.5f });

                var attackMeleeComponent = botTestChar.AddComponent<Attack_Melee>();
                attackMeleeComponent.damage = 50f;
                attackMeleeComponent.knockback = 5f;
                attackMeleeComponent.fallTime = 1f;
                attackMeleeComponent.cooldown = 1.5f;
                attackMeleeComponent.range = 2.5f;
                attackMeleeComponent.force = 800f;
                attackMeleeComponent.attackPart = BodypartType.Elbow_L;
                attackMeleeComponent.attackCurve = AnimationCurve.Constant(0f, 1f, 0.5f);
                attackMeleeComponent.collisionThreshold = 2f;
                attackMeleeComponent.additionalAttackPart = [];
                attackMeleeComponent.additionalAttackForceMultiplier = 0.5f;
            });
    }

    /// <summary>
    /// Creates the custom shop category and defers item registration until the API is ready.
    /// DeferRegistration avoids ordering issues with vanilla item tables during startup.
    /// </summary>
    private static void RegisterItems()
    {
        TestCategory = Items.RegisterCustomCategory("Test Category");

        Items.DeferRegistration(() =>
        {
            RegisterTestItem();
            RegisterTestGun();
        });
    }

    /// <summary>
    /// Locates the vanilla Dog laser projectile prefab via Resources.
    /// Reusing vanilla projectiles avoids authoring custom VFX/collision from scratch.
    /// </summary>
    private static GameObject? GetDogLaserProjectileSource()
    {
        GameObject? dogPrefab = Resources.Load<GameObject>("Dog");
        if (dogPrefab == null)
        {
            Logger.LogError("Dog prefab not found in Resources.");
            return null;
        }

        Transform? botTransform = dogPrefab.transform.Find("Bot")
            ?? dogPrefab.transform.Find("Bot_Dog");
        if (botTransform == null)
        {
            Logger.LogError("Bot child not found on Dog prefab.");
            return null;
        }

        Attack_Dog? attackDog = botTransform.GetComponent<Attack_Dog>();
        if (attackDog == null)
        {
            Logger.LogError("Attack_Dog component not found on Dog Bot.");
            return null;
        }

        if (attackDog.projectile == null)
        {
            Logger.LogError("Attack_Dog.projectile is null on Dog prefab.");
            return null;
        }

        return attackDog.projectile;
    }

    /// <summary>
    /// Clones the Dog laser, strips default Projectile settings, and attaches custom hit logic.
    /// The result is a DontDestroyOnLoad template instantiated by TestGunItemBehaviour when firing.
    /// </summary>
    private static GameObject? CreateTestGunProjectilePrefab(GameObject source)
    {
        GameObject shotPrefab = Object.Instantiate(source);
        shotPrefab.name = "TestGunLaser";

        // Remove original Projectile components so we can configure a clean projectile profile.
        foreach (Projectile existing in shotPrefab.GetComponents<Projectile>())
        {
            Object.DestroyImmediate(existing);
        }

        Projectile proj = shotPrefab.AddComponent<Projectile>();
        proj.velocity = 200f;
        proj.gravity = 0f;
        proj.upVelocity = 0f;
        proj.fall = 0f;
        proj.force = 0f;
        proj.damage = 0f;
        proj.postHitBehavior = Projectile.PostHitBehaviour.Destroy;
        proj.layerType = HelperFunctions.LayerType.Tangible;

        var projectileLogic = shotPrefab.AddComponent<TestGunProjectileLogic>();
        projectileLogic.force = 40f;
        projectileLogic.fall = 2f;

        shotPrefab.SetActive(false);
        Object.DontDestroyOnLoad(shotPrefab);
        Logger.Log($"Created Test Gun projectile from Dog laser: {source.name}");
        return shotPrefab;
    }

    /// <summary>
    /// Registers a simple clickable item from TestItem.prefab with apple material and shop metadata.
    /// </summary>
    private static void RegisterTestItem()
    {
        GameObject? itemPrefab = ContentLoader.LoadPrefabFromBundle(BundleMod!, "TestItem.prefab");
        if (itemPrefab == null)
        {
            Logger.LogWarning("ExampleItem.prefab not found in bundle.");
            return;
        }

        itemPrefab.AddComponent<BasicUsableItem>();
        GameMaterials.ApplyMaterial(itemPrefab, GameMaterial.M_Apple_1, true);

        Sprite? icon = BundleMod!.LoadAsset<Sprite>("example_icon");
        Items.RegisterItem(itemPrefab, new ItemConfig
        {
            displayName = "Test Item",
            price = 100,
            category = TestCategory,
            icon = icon,
            holdPos = new Vector3(0.3f, -0.3f, 0.7f)
        });
    }

    /// <summary>
    /// Registers the laser gun: wires projectile prefab, fire point, materials, and shop entry.
    /// Requires GetDogLaserProjectileSource and TestGun.prefab from the mod bundle.
    /// </summary>
    private static void RegisterTestGun()
    {
        GameObject? laserSource = GetDogLaserProjectileSource();
        if (laserSource == null)
        {
            return;
        }

        GameObject? shotPrefab = CreateTestGunProjectilePrefab(laserSource);
        if (shotPrefab == null)
        {
            return;
        }

        GameObject? itemPrefab = ContentLoader.LoadPrefabFromBundle(BundleMod!, "TestGun.prefab");
        if (itemPrefab == null)
        {
            Logger.LogWarning("TestGun.prefab not found in bundle.");
            return;
        }

        var behaviour = itemPrefab.AddComponent<TestGunItemBehaviour>();
        behaviour.projectilePrefab = shotPrefab;

        // FirePoint should be authored in the Unity prefab so shots originate from the barrel.
        Transform? firePoint = itemPrefab.transform.Find("FirePoint");
        if (firePoint == null)
        {
            Logger.LogWarning("FirePoint not found on TestGun; using item root transform.");
            behaviour.firePoint = itemPrefab.transform;
        }
        else
        {
            behaviour.firePoint = firePoint;
        }

        GameMaterials.ApplyMaterial(itemPrefab, GameMaterial.M_Metal, true);

        Sprite? icon = BundleMod!.LoadAsset<Sprite>("example_icon");
        Items.RegisterItem(itemPrefab, new ItemConfig
        {
            displayName = "Test Gun",
            price = 150,
            category = TestCategory,
            icon = icon,
            holdPos = new Vector3(0.3f, -0.3f, 0.7f),
            holdRot = new Vector3(0, 0, 0),
            useAlternativeHoldPos = true,
            alternativeHoldPos = new Vector3(0.2f, -0.22f, 0.7f)
        });
    }
}
